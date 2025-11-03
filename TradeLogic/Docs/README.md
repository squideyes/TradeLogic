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
        var view = PM.GetView();
        if (view.State == PositionState.Flat)
        {
            PM.SubmitEntry(OrderType.Market, Side.Long, 1);
            PM.ArmExits(stopLoss: 100m, takeProfit: 110m);
        }
    }
}
```

## PositionManager API

```csharp
// Get current position state
PositionView view = PM.GetView();

// Submit entry (always FOK)
string orderId = PM.SubmitEntry(OrderType.Market, Side.Long, quantity);

// Arm exits (StopLoss and/or TakeProfit)
PM.ArmExits(stopLossPrice: 100m, takeProfitPrice: 110m);

// Replace exits
PM.ReplaceExits(newStopLoss: 99m, newTakeProfit: 111m);

// Flatten position immediately
PM.GoFlat();

// Reset after position closes
PM.Reset();
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
OnPM_ExitReplaced()
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

1. See [HowToCreateAStrategy.md](HowToCreateAStrategy.md)
2. Review [NinjaTrader/SimpleMAStrategy.cs](NinjaTrader/SimpleMAStrategy.cs)
3. Create your strategy inheriting from `TradeLogicStrategyBase`
4. Test in NinjaTrader simulation

