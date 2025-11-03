# How to Create a Strategy

## Setup

1. Build TradeLogic to produce TradeLogic.dll
2. Add reference to TradeLogic.dll in your NinjaTrader strategy project
3. Add using statements:
   `csharp
   using TradeLogic;
   using TradeLogic.NinjaTrader;
   `

## Create Your Strategy

Inherit from TradeLogicStrategyBase and implement three methods.

See NinjaTrader/SimpleMAStrategy.cs for a complete working example.

## Required Methods

### CreatePositionConfig()
Return a configured PositionConfig with your position parameters.

### CreateFeeModel()
Return an IFeeModel (typically FlatFeeModel).

### OnBarUpdateTradeLogic()
Implement your trading logic here. Called on each bar after time is updated.

## Optional Methods

### OnConfigure()
Initialize indicators here.

### OnSetDefaults()
Override to customize strategy defaults.

### OnTradeLogicInitialized()
Called after PositionManager is created.

### OnTradeLogicTerminated()
Called when strategy terminates.

## Event Handlers

Override any of these for custom behavior:

- OnPM_OrderSubmitted()
- OnPM_OrderAccepted()
- OnPM_OrderRejected()
- OnPM_OrderCanceled()
- OnPM_OrderExpired()
- OnPM_OrderWorking()
- OnPM_OrderPartiallyFilled()
- OnPM_OrderFilled()
- OnPM_PositionOpened()
- OnPM_ExitArmed()
- OnPM_PositionUpdated()
- OnPM_PositionClosing()
- OnPM_PositionClosed()
- OnPM_TradeFinalized()
- OnPM_ErrorOccurred()

## Access PositionManager

Use the PM property:

- `PM.GetPosition()` - Get current position state
- `PM.SubmitEntry()` - Submit entry with mandatory exits
- `PM.GoFlat()` - Flatten position

## Submit Entry

Use `SubmitEntry()` to submit an entry with mandatory stop loss and take profit:

```csharp
decimal stopLoss = (decimal)Close[0] - (10 * (decimal)TickSize);
decimal takeProfit = (decimal)Close[0] + (20 * (decimal)TickSize);
PM.SubmitEntry(OrderType.Market, Side.Long, 1, stopLoss, takeProfit);
```

Exits are mandatory and always submitted atomically with the entry.
