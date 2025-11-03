# TradeLogic

Event-driven position management library for single-position trading strategies.

## Features

- Single position lifecycle management (entry → open → exit)
- FOK (Fill-or-Kill) entries
- OCO (One-Cancels-Other) exits (StopLoss + TakeProfit)
- End-of-Session flattening
- Immutable Trade records
- Event-driven communication
- Thread-safe with actor-style concurrency

## Installation

### Step 1: Build the DLL

```bash
dotnet build TradeLogic/TradeLogic.csproj -c Release
```

The compiled DLL will be at: `TradeLogic/bin/Release/net48/TradeLogic.dll`

### Step 2: Add DLL to NinjaTrader

1. Open NinjaTrader 8
2. Go to **Tools → Import → Import NinjaScript**
3. Select the `TradeLogic.dll` file
4. Click **Import**

The DLL will be added to your NinjaTrader bin folder and available for reference in your strategies.

### Step 3: Reference in Your Strategy

In your NinjaTrader strategy file, add the using statement:

```csharp
using TradeLogic;
```

## Quick Start

Inherit from `TradeLogicStrategyBase` and implement three methods:

```csharp
public class MyStrategy : TradeLogicStrategyBase
{
    protected override PositionConfig CreatePositionConfig()
    {
        return new PositionConfig
        {
            Symbol = Symbol.ES,
            TickSize = Instrument.MasterInstrument.TickSize,
            PointValue = Instrument.MasterInstrument.PointValue,
            MinQty = 1,
            IdPrefix = "MY",
            MarketableLimitOffsetTicks = 2,
            UseStopLimitForSL = false,
            SlippageToleranceTicks = 4,
            Session = new SessionConfig
            {
                TimeZoneId = "Eastern Standard Time",
                SessionStartLocal = new TimeSpan(9, 30, 0),
                SessionEndLocal = new TimeSpan(16, 0, 0)
            }
        };
    }

    protected override IFeeModel CreateFeeModel()
    {
        return new FlatFeeModel(2.50m);
    }

    protected override void OnBarUpdateTradeLogic()
    {
        var position = PM.GetPosition();
        if (position.State == PositionState.Flat)
        {
            PM.SubmitEntry(OrderType.Market, Side.Long, 1, 100m, 110m);
        }
    }
}
```

## PositionManager API

```csharp
// Get current position state
PositionView position = PM.GetPosition();

// Submit entry with exits (mandatory)
string orderId = PM.SubmitEntry(OrderType.Market, Side.Long, quantity,
    stopLossPrice: 100m, takeProfitPrice: 110m);

// Flatten position immediately
PM.GoFlat();
```

## Position States

```
Flat → PendingEntry → Open → PendingExit/Closing → Closed
```

## Events

Override these in your strategy:

```csharp
OnPM_OrderSubmitted()
OnPM_OrderAccepted()
OnPM_OrderRejected()
OnPM_OrderCanceled()
OnPM_OrderExpired()
OnPM_OrderWorking()
OnPM_OrderPartiallyFilled()
OnPM_OrderFilled()
OnPM_PositionOpened()
OnPM_ExitArmed()
OnPM_PositionUpdated()
OnPM_PositionClosing()
OnPM_PositionClosed()
OnPM_TradeFinalized()
OnPM_ErrorOccurred()
```

## Documentation

- **[HowToCreateAStrategy.md](HowToCreateAStrategy.md)** - Complete guide
- **[NinjaTrader/SimpleMAStrategy.cs](NinjaTrader/SimpleMAStrategy.cs)** - Example strategy

## Getting Started

### 1. Install the DLL

Follow the [Installation](#installation) steps above.

### 2. Create Your Strategy

Create a new NinjaScript strategy file in NinjaTrader:

```csharp
using TradeLogic;
using NinjaTrader.Cbi;
using NinjaTrader.Instrument;
using NinjaTrader.Core;
using NinjaTrader.Data;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class MyStrategy : TradeLogicStrategyBase
    {
        protected override PositionConfig CreatePositionConfig()
        {
            return new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = Instrument.MasterInstrument.TickSize,
                PointValue = Instrument.MasterInstrument.PointValue,
                MinQty = 1,
                IdPrefix = "MY",
                MarketableLimitOffsetTicks = 2,
                UseStopLimitForSL = false,
                SlippageToleranceTicks = 4,
                Session = new SessionConfig
                {
                    TimeZoneId = "Eastern Standard Time",
                    SessionStartLocal = new TimeSpan(9, 30, 0),
                    SessionEndLocal = new TimeSpan(16, 0, 0)
                }
            };
        }

        protected override IFeeModel CreateFeeModel()
        {
            return new FlatFeeModel(2.50m);  // $2.50 per contract
        }

        protected override void OnBarUpdateTradeLogic()
        {
            var position = PM.GetPosition();

            if (position.State == PositionState.Flat)
            {
                // Your entry logic here
                PM.SubmitEntry(OrderType.Market, Side.Long, 1,
                    stopLossPrice: 100m, takeProfitPrice: 110m);
            }
        }
    }
}
```

### 3. Review Examples

- **[HowToCreateAStrategy.md](HowToCreateAStrategy.md)** - Complete guide with all available methods and events
- **[NinjaTrader/SimpleMAStrategy.cs](NinjaTrader/SimpleMAStrategy.cs)** - Full example strategy with moving average logic

### 4. Test in NinjaTrader

1. Compile your strategy in NinjaTrader
2. Add it to a chart
3. Run in **Simulation** mode first
4. Monitor the Output window for any errors
5. Once tested, run live trading

