# NinjaTrader Integration Setup Guide

This guide explains how to set up TradeLogic in NinjaTrader 8.

## Overview

The files in this folder are **templates** that you copy into NinjaTrader's Scripts folder. They are NOT compiled as part of the TradeLogic DLL.

- **TradeLogicStrategyBase.cs** - Abstract base class for your strategies (namespace: `NinjaTrader.NinjaScript.Strategies`)
- **NinjaTraderLogger.cs** - Logging implementation for NinjaTrader (namespace: `NinjaTrader.NinjaScript.Strategies`)
- **SETUP_INSTRUCTIONS.md** - This file

**Key Point:** These files use the `NinjaTrader.NinjaScript.Strategies` namespace so they compile correctly in NinjaTrader's environment where NinjaTrader types are available.

## Step 1: Build the TradeLogic DLL

From the TradeLogic project root:

```bash
dotnet build TradeLogic/TradeLogic.csproj -c Release
```

This creates `TradeLogic/bin/Release/TradeLogic.dll`

## Step 2: Copy Template Files to NinjaTrader

Copy these two files to your NinjaTrader Scripts folder:

```
C:\Users\[YourUsername]\Documents\NinjaTrader 8\bin\Custom\Strategies\
```

Files to copy:
- `TradeLogicStrategyBase.cs`
- `NinjaTraderLogger.cs`

## Step 3: Add TradeLogic.dll Reference in NinjaTrader

1. Open NinjaTrader
2. Go to **Tools → Edit → Compile Strategy**
3. In the Compile Strategy window, click **Add Reference**
4. Browse to `TradeLogic/bin/Release/TradeLogic.dll`
5. Click **OK**

## Step 4: Create Your Strategy

Create a new strategy file in the Strategies folder that inherits from `TradeLogicStrategyBase`:

```csharp
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;
using TradeLogic;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class MyTradeLogicStrategy : TradeLogicStrategyBase
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
            // Your trading logic here
            // Use PM.SubmitEntry() to enter trades
            // Use PM.GoFlat() to exit
        }
    }
}
```

## Step 5: Compile and Test

1. In NinjaTrader, go to **Tools → Edit → Compile Strategy**
2. Select your strategy file
3. Click **Compile**
4. If there are errors, check that:
   - TradeLogic.dll is referenced
   - TradeLogicStrategyBase.cs and NinjaTraderLogger.cs are in the Strategies folder
   - Your strategy inherits from TradeLogicStrategyBase

## Important Notes

### Why These Files Are Templates

- **TradeLogicStrategyBase.cs** and **NinjaTraderLogger.cs** are NOT part of the TradeLogic.dll
- They are templates that you copy into NinjaTrader
- This is because they depend on NinjaTrader types (Strategy, Order, etc.) which are only available in NinjaTrader
- The TradeLogic.dll is venue-agnostic and has no NinjaTrader dependencies

### Namespace Design

TradeLogicStrategyBase.cs and NinjaTraderLogger.cs use the `NinjaTrader.NinjaScript.Strategies` namespace because:

1. **They depend on NinjaTrader types** - `Strategy`, `Order`, `OrderState`, etc.
2. **They must compile in NinjaTrader's environment** - Not in the TradeLogic DLL
3. **Direct access to NinjaTrader enums** - No need for fully qualified names:
   ```csharp
   if (State == State.SetDefaults)           // ✓ Works in NinjaTrader
   Calculate = Calculate.OnBarClose;         // ✓ Works in NinjaTrader
   ```

### TradeLogic Type References

TradeLogicStrategyBase.cs imports TradeLogic types directly:
```csharp
using TradeLogic;
using TradeLogic.Logging;

// Direct access to TradeLogic types:
protected PositionManager PM { get; private set; }
protected abstract PositionConfig CreatePositionConfig();
protected abstract IFeeModel CreateFeeModel();
```

This works because both namespaces are imported and there are no naming conflicts.

### Logging

NinjaTraderLogger forwards all log entries to NinjaTrader's `Print()` method, which outputs to the Output window.

Log entry types:
- **TextLogEntry** - General text messages
- **ErrorLogEntry** - Error messages with error codes
- **StateTransitionLogEntry** - Position state changes
- **OrderLogEntry** - Order submissions and updates
- **FillLogEntry** - Trade fills
- **TradeLogEntry** - Completed trades with P&L

## Troubleshooting

### "TradeLogic.dll not found"
- Make sure you built the DLL: `dotnet build TradeLogic/TradeLogic.csproj -c Release`
- Add the reference in NinjaTrader: **Tools → Edit → Compile Strategy → Add Reference**

### "TradeLogicStrategyBase not found"
- Copy TradeLogicStrategyBase.cs to your Strategies folder
- Make sure it's in: `C:\Users\[YourUsername]\Documents\NinjaTrader 8\bin\Custom\Strategies\`

### "NinjaTraderLogger not found"
- Copy NinjaTraderLogger.cs to your Strategies folder
- Make sure it's in the same folder as TradeLogicStrategyBase.cs

### Compilation errors about "State", "Calculate", etc.
- These are NinjaTrader enums that should be available when compiling in NinjaTrader
- Make sure you're using NinjaTrader 8 (not NinjaTrader 7)
- Restart NinjaTrader if you just added the DLL reference

## Next Steps

1. Review the PositionManager API in the main README.md
2. Look at the example strategy above
3. Implement your trading logic in `OnBarUpdateTradeLogic()`
4. Test in Simulation mode first
5. Monitor the Output window for log messages

