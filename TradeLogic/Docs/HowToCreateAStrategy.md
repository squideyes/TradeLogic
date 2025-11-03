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
- OnPM_ExitReplaced()
- OnPM_PositionUpdated()
- OnPM_PositionClosing()
- OnPM_PositionClosed()
- OnPM_TradeFinalized()
- OnPM_ErrorOccurred()

## Access PositionManager

Use the PM property:

- PM.GetView() - Get current position state
- PM.SubmitEntry() - Submit entry order
- PM.ArmExits() - Arm exit orders
- PM.ReplaceExits() - Replace exit orders
- PM.GoFlat() - Flatten position
- PM.Reset() - Reset after position closes
