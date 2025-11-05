# TradeLogic

Event-driven position management library for single-position trading strategies with NinjaTrader integration.

## Overview

TradeLogic is a robust position management system that handles the complete lifecycle of a single trading position. It separates entry order submission from exit price configuration, allowing flexible order management. The system automatically manages OCO (One-Cancels-Other) exit orders after entry fills, and automatically flattens positions at end-of-session.

## Key Features

- **Single Position Management**: Manages one position at a time through its complete lifecycle (Flat → PendingEntry → Open → PendingExit/Closing → Closed)
- **Flexible Entry/Exit Management**: Submit entry orders without SL/TP, then set exit prices after entry or after fill
- **Automatic Exit Placement**: (A) Immediately after entry fill, fixed SL & TP OCO orders are placed based on deltas in current ATM
- **Strategy-Driven Exit Adjustment**: (B) Strategy can then move SL & TP using callbacks and strategy logic
- **FOK (Fill-or-Kill) Entries**: Ensures atomic entry fills to prevent partial positions
- **OCO Exit Orders**: Automatically manages stop-loss and take-profit as one-cancels-other orders
- **End-of-Session Flattening**: Automatically closes positions at configured session end time
- **Immutable Trade Records**: Complete trade history with entry/exit fills, fees, slippage tracking
- **Event-Driven Architecture**: Clean separation between position logic and order execution
- **Thread-Safe**: Actor-style concurrency with internal locking
- **NinjaTrader Integration**: Complete base class (`TradeLogicStrategyBase`) handles all plumbing
- **Future-Ready**: Designed to support multiple flights and scale legs in future versions

## Installation

### Step 1: Build the DLL

```bash
dotnet build TradeLogic/TradeLogic.csproj -c Release
```

The compiled DLL will be at: `TradeLogic/bin/Release/TradeLogic.dll`

### Step 2: Add DLL to NinjaTrader

1. Copy `TradeLogic.dll` to your NinjaTrader bin folder:
   - Default location: `C:\Users\<YourName>\Documents\NinjaTrader 8\bin\Custom\`
2. Restart NinjaTrader

### Step 3: Copy Base Class Files

Copy these files to your NinjaTrader Strategies folder:
- `Docs/NinjaTrader/TradeLogicStrategyBase.cs`
- `Docs/NinjaTrader/NinjaTraderLogger.cs`

Default location: `C:\Users\<YourName>\Documents\NinjaTrader 8\strategies\`

## Quick Start

Create a NinjaTrader strategy by inheriting from `TradeLogicStrategyBase` and implementing abstract methods:

```csharp
using NinjaTrader.NinjaScript.Strategies;
using TL = TradeLogic;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class MyStrategy : TradeLogicStrategyBase
    {
        // 1. Configure the PositionManager
        protected override TL.PositionConfig CreatePositionConfig()
        {
            return new TL.PositionConfig
            {
                Symbol = TL.SymbolHelper.Parse(Instrument.MasterInstrument.Name),
                TickSize = (decimal)Instrument.MasterInstrument.TickSize,
                PointValue = (decimal)Instrument.MasterInstrument.PointValue,
                IdPrefix = "MY",
                SlippageToleranceTicks = 1  // Slippage should be extremely rare and overwhelmingly 1 tick
            };
        }

        // 2. Handle tick-level updates (optional - for tick-level indicators)
        protected override void OnTick(TL.Tick tick)
        {
            // Called on every tick - use for tick-level indicator updates if needed
        }

        // 3. Implement your trading logic (called when bar closes)
        protected override void OnBar(TL.Bar bar)
        {
            var position = PM.GetPosition();

            if (position.State == TL.PositionState.Flat)
            {
                // Submit entry order without SL/TP
                PM.SubmitEntry(TL.OrderType.Market, TL.Side.Long, 1);
            }
            else if (position.State == TL.PositionState.PendingEntry)
            {
                // Calculate stop-loss and take-profit prices based on current ATM
                decimal currentPrice = bar.Close;
                decimal stopLoss = currentPrice - (10 * (decimal)TickSize);
                decimal takeProfit = currentPrice + (20 * (decimal)TickSize);

                // Set exit prices after entry submission
                PM.SetExitPrices(stopLoss, takeProfit);
            }
        }
    }
}
```

## How PositionManager Works

### Position Lifecycle State Machine

```
┌─────────┐
│  Flat   │ ← Initial state, no position
└────┬────┘
     │ PM.SubmitEntry()
     ▼
┌──────────────┐
│ PendingEntry │ ← Entry order submitted, waiting for fill
└──────┬───────┘
       │ OnOrderFilled()
       ▼
┌──────────┐
│   Open   │ ← Position filled, OCO exits submitted
└────┬─────┘
     │ SL/TP hit, GoFlat(), or End-of-Session
     ▼
┌─────────────┐
│   Closing   │ ← Exit order submitted
└──────┬──────┘
       │ OnOrderFilled()
       ▼
┌─────────┐
│ Closed  │ ← Trade finalized, ready for next entry
└────┬────┘
     │ (returns to Flat for next trade)
     ▼
```

### Trading Decision Logic

The PositionManager makes the following automatic decisions:

1. **Entry Submission** (`PM.SubmitEntry()`):
   - Validates state is `Flat`
   - Creates FOK (Fill-or-Kill) entry order (without SL/TP)
   - Transitions to `PendingEntry`

2. **Exit Price Configuration** (`PM.SetExitPrices()`):
   - Can be called from `PendingEntry` or `Open` state
   - Stores armed stop-loss and take-profit prices
   - If already `Open`, immediately submits OCO exit orders
   - Fires `ExitArmed` event

3. **Entry Fill** (`OnOrderFilled` for entry):
   - Updates position quantity and average entry price
   - Transitions to `Open`
   - **(A) Immediately after entry fill, submits OCO exit orders** if exit prices are armed
   - Fires `PositionOpened` event

4. **Exit Management**:
   - **Stop-Loss**: Stop or StopLimit order at armed SL price
   - **Take-Profit**: Limit order at armed TP price
   - **OCO Group**: Both exits linked; one fill cancels the other
   - **GTD (Good-Till-Date)**: Exits expire at session end
   - **(B) Strategy can move SL & TP using callbacks** and strategy logic

5. **Manual Flatten** (`PM.GoFlat()`):
   - **If PendingEntry**: Cancels working entry order (Limit/Stop/StopLimit)
   - **If Open**: Cancels working SL/TP orders and submits immediate market exit order
   - Transitions to `Closing` (or back to `Flat` if entry canceled)

6. **End-of-Session**:
   - Detected by `OnTick()` when current time >= session end
   - **If PendingEntry**: Automatically cancels working entry order (Limit/Stop/StopLimit)
   - **If Open**: Automatically submits market exit if no exits working
   - Transitions to `Closing` (or back to `Flat` if entry canceled)

7. **Exit Fill** (`OnOrderFilled` for exit):
   - Updates realized P&L
   - Cancels remaining OCO exit
   - Transitions to `Closed`
   - Fires `PositionClosed` and `TradeFinalized` events
   - Returns to `Flat` for next trade

### Risk Management Features

- **Flexible Exit Configuration**: Set exit prices after entry submission or after entry fill
- **Automatic Exit Placement**: OCO exits placed immediately after entry fill with armed prices
- **FOK Entries**: Prevents partial fills that could leave you with unexpected position size
- **Slippage Tracking**: Compares actual fill price to intended price, warns if exceeds tolerance
- **Commission Tracking**: Calculates total fees per trade using Tradovate fees
- **Session Protection**: Automatically flattens positions at end of trading session
- **Automatic Cancellation**: Pending entries automatically canceled at end-of-session

## PositionManager API

### Methods

```csharp
// Get current position state (thread-safe snapshot)
PositionView position = PM.GetPosition();

// Submit entry order without SL/TP
string orderId = PM.SubmitEntry(
    OrderType.Market,           // Market, Limit, Stop, or StopLimit
    Side.Long,                  // Long or Short
    1,                          // Quantity (e.g., 1, 10, 100)
    limitPrice: null,           // Optional: for Limit/StopLimit entry orders
    stopPrice: null);           // Optional: for Stop/StopLimit entry orders

// Set exit prices after entry submission or after entry fill
// Can be called from PendingEntry or Open state
PM.SetExitPrices(
    stopLossPrice: 4500m,       // SL price
    takeProfitPrice: 4550m,     // TP price
    stopLimitPrice: null);      // Optional: limit price for StopLimit SL exit

// Cancel a specific order by client order ID
PM.CancelOrder(clientOrderId);

// Manually flatten position (cancels SL/TP, submits market exit)
PM.GoFlat();

// Feed market data (called automatically by TradeLogicStrategyBase)
PM.OnTick(new Tick(timestamp, close, bid, ask, volume));

// Order lifecycle callbacks (called automatically by TradeLogicStrategyBase)
PM.OnOrderAccepted(orderUpdate);
PM.OnOrderRejected(orderUpdate);
PM.OnOrderCanceled(orderUpdate);
PM.OnOrderExpired(orderUpdate);
PM.OnOrderWorking(orderUpdate);
PM.OnOrderPartiallyFilled(clientOrderId, fillId, price, quantity, fillTime);
PM.OnOrderFilled(clientOrderId, fillId, price, quantity, fillTime);
```

### ITickHandler Interface

PositionManager implements `ITickHandler` for tick-driven processing:

```csharp
public interface ITickHandler
{
    void OnTick(Tick tick);  // Called on every tick - only ticks, not bars
}
```

**Important**: PositionManager only handles ticks. Bars are synthesized by `BarConsolidator` which consolidates ticks into fixed-period bars and calls `PM.HandleBar()` internally (not public). This ensures PositionManager remains purely tick-driven.

### PositionView Properties

```csharp
public sealed class PositionView
{
    public PositionState State { get; }        // Current state
    public Side? Side { get; }                 // Long/Short (null if Flat)
    public int NetQuantity { get; }            // Signed quantity (+ for long, - for short)
    public decimal AvgEntryPrice { get; }      // Average entry fill price
    public decimal RealizedPnl { get; }        // Realized P&L (includes fees)
    public decimal UnrealizedPnl { get; }      // Unrealized P&L (always 0 in current impl)
    public DateTime? OpenedUtc { get; }        // When position opened
    public DateTime? ClosedUtc { get; }        // When position closed
    public Symbol Symbol { get; }              // Instrument symbol
    public decimal? ArmedStopLoss { get; }     // Armed SL price
    public decimal? ArmedTakeProfit { get; }   // Armed TP price
}
```

## Events

### Strategy Abstract Methods (Implement in Your Strategy)

These are the abstract methods you must implement in your strategy:

```csharp
// Bar closed (called when bar period completes)
// This is where you implement your trading logic
protected override void OnBar(TL.Bar bar)
{
    Print($"Bar closed: O={bar.Open} H={bar.High} L={bar.Low} C={bar.Close} V={bar.Volume}");
    // Implement your trading logic here
}

// Tick received (called on every tick)
// Use for tick-level indicator updates if needed
protected override void OnTick(TL.Tick tick)
{
    // Optional: update tick-level indicators
}

// Position opened (entry filled, exits submitted)
protected virtual void OnPM_PositionOpened(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason)
{
    Print($"Position opened: {view.Side} {view.NetQuantity} @ {view.AvgEntryPrice}");
}

// Position closed (exit filled)
protected virtual void OnPM_PositionClosed(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason)
{
    Print($"Position closed: {exitReason}");
}

// Trade finalized (complete trade record available)
protected virtual void OnPM_TradeFinalized(Guid positionId, TL.Trade trade)
{
    Print($"Trade: {trade.Side} {trade.NetQty} @ {trade.AvgEntryPrice} → {trade.AvgExitPrice}");
    Print($"P&L: {trade.RealizedPnl:C}, Fees: {trade.TotalFees:C}, Slippage: {trade.Slippage:C}");
    Print($"Exit Reason: {trade.ExitReason}");
}

// Error occurred (e.g., slippage warning, cancel request)
protected virtual void OnPM_ErrorOccurred(string code, string message, object context)
{
    Print($"TradeLogic ERROR [{code}]: {message}");
    // Base class handles CANCEL_REQUEST automatically
}
```

## NinjaTrader Integration

### How TradeLogicStrategyBase Works

`TradeLogicStrategyBase` is an abstract base class that handles all the plumbing between NinjaTrader and TradeLogic:

1. **Initialization** (State.DataLoaded):
   - Creates `PositionManager` with your config
   - Creates `BarConsolidator` to consolidate ticks into bars
   - Subscribes to all PositionManager events
   - Maps events to virtual methods you can override

2. **Market Data** (OnBarUpdate):
   - Converts NinjaTrader bar data to `Tick` objects
   - Calls `PM.OnTick(tick)` to update PositionManager with tick data (PositionManager is purely tick-driven)
   - Calls your `OnTick(tick)` method for tick-level processing
   - Calls `BarConsolidator.ProcessTick(tick)` to accumulate ticks into fixed-period bars
   - When bar period completes, BarConsolidator calls `PM.HandleBar(bar)` internally
   - PositionManager raises `OnBarClosed` event, triggering your `OnBar(bar)` method

3. **Bar-Driven Trading Logic**:
   - Your `OnBar(bar)` method is called when each bar closes
   - Use `bar.Open`, `bar.High`, `bar.Low`, `bar.Close`, `bar.Volume` for trading decisions
   - Most indicators update on bar close; some update on every tick via `OnTick(tick)`
   - PositionManager only sees ticks; bars are synthesized by BarConsolidator

4. **Order Routing** (Internal):
   - Receives order specs from PositionManager
   - Translates to NinjaTrader order methods (EnterLong, ExitShort, etc.)
   - Maintains bidirectional mapping between client order IDs and NT orders

5. **Order Updates** (Internal):
   - Receives NinjaTrader order state changes
   - Translates to TradeLogic callbacks
   - Forwards to PositionManager

6. **Cleanup** (State.Terminated):
   - Unsubscribes from all events
   - Calls your `OnTradeLogicTerminated()` method

### Configuration Reference

#### PositionConfig Properties

```csharp
public sealed class PositionConfig
{
    // Instrument identification
    public Symbol Symbol { get; set; }                    // ES, NQ, CL, etc.

    // Pricing (set from Instrument.MasterInstrument)
    public decimal TickSize { get; set; }                 // Minimum price increment
    public decimal PointValue { get; set; }               // Dollar value per point

    // Order parameters
    public string IdPrefix { get; set; }                  // Prefix for order IDs
    public int SlippageToleranceTicks { get; set; }       // Max acceptable slippage (default: 1 tick)
}
```

**Default Values**:
- `SlippageToleranceTicks = 1` - Slippage should be extremely rare (< 1%) and overwhelmingly one tick

**Important**: Always set `Symbol`, `TickSize`, and `PointValue` from `Instrument.MasterInstrument`:

```csharp
Symbol = TL.SymbolHelper.Parse(Instrument.MasterInstrument.Name),
TickSize = (decimal)Instrument.MasterInstrument.TickSize,
PointValue = (decimal)Instrument.MasterInstrument.PointValue,
```

This ensures accurate P&L calculations and price rounding.

#### BarPeriod Configuration

Configure the bar consolidation period in your strategy's `OnSetDefaults()` method:

```csharp
protected override void OnSetDefaults()
{
    BarPeriod = TimeSpan.FromMinutes(1);  // Default: 1 minute bars
    // Or use other periods:
    // BarPeriod = TimeSpan.FromSeconds(5);   // 5-second bars
    // BarPeriod = TimeSpan.FromMinutes(5);   // 5-minute bars
}
```

The BarConsolidator:
- Accumulates ticks into fixed-period bars
- Fires `OnBar(bar)` when each bar period completes
- Bars are aligned to session start (first bar's start time)
- All OHLCV data is preserved and passed to your strategy

#### SessionConfig (Constant)

The trading session is fixed at **6:30 AM - 4:30 PM Eastern Time** and cannot be changed:

```csharp
public static class SessionConfig
{
    public static readonly TimeSpan SessionStartLocal = new TimeSpan(6, 30, 0);   // 6:30 AM ET
    public static readonly TimeSpan SessionEndLocal = new TimeSpan(16, 30, 0);    // 4:30 PM ET
    public static readonly string TimeZoneId = "Eastern Standard Time";
}
```

The PositionManager uses these times to:
- Set GTD (Good-Till-Date) expiration on exit orders at 4:30 PM ET
- Automatically flatten positions at 4:30 PM ET
- Prevent entries after session end

#### Commission Fees

Commission fees are automatically calculated using **Tradovate fees** based on the symbol:

```csharp
public static class TradovateFees
{
    public static decimal GetFee(Symbol symbol);
    public static decimal ComputeCommission(Fill fill, Symbol symbol);
}
```

**Standard Tradovate Fees (2025)**:
- E-mini futures (ES, NQ, TY, FV, US): $0.85 per contract
- Micro futures (MES, MNQ, MCL): $0.25 per contract
- Full-size futures (CL, GC): $1.29 per contract
- Currencies (EU, JY, BP): $0.85 per contract

Fees are applied automatically - no configuration needed.

## Complete Example

See **[NinjaTrader/SimpleMaStrategy.cs](NinjaTrader/SimpleMaStrategy.cs)** for a complete working example with:
- Moving average crossover logic
- Dynamic stop-loss and take-profit calculation
- Event handling for trade tracking
- Proper NinjaTrader integration

## Testing

### Unit Tests

The library includes comprehensive unit tests covering:
- Position state transitions
- Order lifecycle (submit, accept, fill, reject, cancel, expire)
- P&L calculations
- Exit logic (stop-loss, take-profit, manual flatten, end-of-session)
- Slippage tracking
- Fee calculations

Run tests:
```bash
dotnet test
```

### NinjaTrader Testing

1. **Compile**: Tools → Edit NinjaScript → Compile
2. **Simulation**: Test with Sim101 account first
3. **Market Replay**: Use Market Replay for realistic testing
4. **Monitor Output**: Watch Output window for TradeLogic events
5. **Verify Trades**: Check Trades tab for completed trades

## Architecture

### Design Principles

1. **Single Responsibility**: PositionManager manages position lifecycle, TradeLogicStrategyBase handles NinjaTrader integration
2. **Immutability**: Trade records are immutable; PositionView is a snapshot
3. **Event-Driven**: Clean separation via events; no tight coupling
4. **Thread-Safe**: Internal locking protects state; all public methods are thread-safe
5. **Fail-Fast**: Validates state transitions; throws exceptions for invalid operations

### Key Types

- **PositionManager**: Core position management logic, implements `ITickHandler`
- **PositionConfig**: Configuration for position behavior
- **PositionView**: Immutable snapshot of current position state
- **Trade**: Immutable record of completed trade
- **OrderSpec**: Specification for an order to be submitted
- **OrderSnapshot**: Current state of an order
- **Fill**: Record of an order fill
- **Tick**: Market data snapshot (timestamp, price, bid, ask, volume)
- **Bar**: Immutable OHLCV bar with open timestamp
- **BarConsolidator**: Consolidates ticks into fixed-period bars
- **ITickHandler**: Interface for tick-driven components (OnTick, OnBar methods)
- **IIdGenerator**: Interface for generating unique order IDs
- **ILogger**: Interface for logging

## Troubleshooting

### Common Issues

**"Cannot access internal events"**
- Internal events are not accessible to strategies. Use the public events: `PositionOpened`, `PositionClosed`, `TradeFinalized`, `ErrorOccurred`.

**"Entry received partial fill under FOK"**
- FOK orders should never partially fill. This indicates a broker issue. The PositionManager will request cancellation.

**"Entry slippage exceeded tolerance"**
- Actual fill price differed significantly from intended price. Check market conditions and adjust `SlippageToleranceTicks`.

**Position not flattening at end of session**
- Verify `SessionConfig` times are correct for your timezone
- Ensure `OnTick()` is being called (check that `OnBarUpdate` is firing)

**Orders not submitting**
- Check NinjaTrader Output window for errors
- Verify TradeLogic.dll is in bin\Custom folder
- Ensure TradeLogicStrategyBase.cs is in strategies folder

## Additional Documentation

- **[NinjaTrader/SETUP_INSTRUCTIONS.md](NinjaTrader/SETUP_INSTRUCTIONS.md)** - Detailed setup guide
- **[NinjaTrader/SimpleMaStrategy.cs](NinjaTrader/SimpleMaStrategy.cs)** - Complete example strategy

