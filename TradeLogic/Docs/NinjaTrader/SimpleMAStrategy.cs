using NinjaTrader.NinjaScript.Indicators;
using System;

namespace TradeLogic.NinjaTrader.Examples
{
    /// <summary>
    /// Simple Moving Average crossover strategy using TradeLogic.
    /// Demonstrates how clean strategy code can be when using TradeLogicStrategyBase.
    /// </summary>
    public class SimpleMAStrategy : TradeLogicStrategyBase
    {
        private SMA _fastMA;
        private SMA _slowMA;

        #region Properties

        [NinjaTrader.Gui.Tools.Range(1, int.MaxValue)]
        [NinjaTrader.Gui.Design.Display(Name = "Fast MA Period", Order = 1, GroupName = "Parameters")]
        public int FastPeriod { get; set; }

        [NinjaTrader.Gui.Tools.Range(1, int.MaxValue)]
        [NinjaTrader.Gui.Design.Display(Name = "Slow MA Period", Order = 2, GroupName = "Parameters")]
        public int SlowPeriod { get; set; }

        [NinjaTrader.Gui.Tools.Range(1, int.MaxValue)]
        [NinjaTrader.Gui.Design.Display(Name = "Quantity", Order = 3, GroupName = "Parameters")]
        public int Quantity { get; set; }

        [NinjaTrader.Gui.Tools.Range(1, int.MaxValue)]
        [NinjaTrader.Gui.Design.Display(Name = "Stop Loss Ticks", Order = 4, GroupName = "Parameters")]
        public int StopLossTicks { get; set; }

        [NinjaTrader.Gui.Tools.Range(1, int.MaxValue)]
        [NinjaTrader.Gui.Design.Display(Name = "Take Profit Ticks", Order = 5, GroupName = "Parameters")]
        public int TakeProfitTicks { get; set; }

        [NinjaTrader.Gui.Tools.Range(0.01, double.MaxValue)]
        [NinjaTrader.Gui.Design.Display(Name = "Fee Per Contract", Order = 6, GroupName = "Parameters")]
        public double FeePerContract { get; set; }

        #endregion

        protected override void OnSetDefaults()
        {
            base.OnSetDefaults();

            Description = "Simple MA crossover using TradeLogic";
            Name = "SimpleMAStrategy";

            FastPeriod = 10;
            SlowPeriod = 20;
            Quantity = 1;
            StopLossTicks = 10;
            TakeProfitTicks = 20;
            FeePerContract = 2.50;
        }

        protected override void OnConfigure()
        {
            _fastMA = SMA(FastPeriod);
            _slowMA = SMA(SlowPeriod);
        }

        protected override PositionConfig CreatePositionConfig()
        {
            return new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = Instrument.MasterInstrument.TickSize,
                PointValue = Instrument.MasterInstrument.PointValue,
                MinQty = 1,
                IdPrefix = "MA",
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
            return new FlatFeeModel((decimal)FeePerContract);
        }

        protected override void OnBarUpdateTradeLogic()
        {
            if (CurrentBar < Math.Max(FastPeriod, SlowPeriod))
                return;

            var view = PM.GetView();

            // Entry logic: Fast MA crosses above Slow MA
            if (CrossAbove(_fastMA, _slowMA, 1) && view.State == PositionState.Flat)
            {
                // Submit long entry
                PM.SubmitEntry(OrderType.Market, Side.Long, Quantity);

                // Arm exits
                decimal stopLoss = (decimal)Close[0] - (StopLossTicks * (decimal)TickSize);
                decimal takeProfit = (decimal)Close[0] + (TakeProfitTicks * (decimal)TickSize);
                PM.ArmExits(stopLoss, takeProfit);
            }
            // Entry logic: Fast MA crosses below Slow MA
            else if (CrossBelow(_fastMA, _slowMA, 1) && view.State == PositionState.Flat)
            {
                // Submit short entry
                PM.SubmitEntry(OrderType.Market, Side.Short, Quantity);

                // Arm exits
                decimal stopLoss = (decimal)Close[0] + (StopLossTicks * (decimal)TickSize);
                decimal takeProfit = (decimal)Close[0] - (TakeProfitTicks * (decimal)TickSize);
                PM.ArmExits(stopLoss, takeProfit);
            }
        }

        #region Optional: Override Event Handlers for Custom Behavior

        protected override void OnPM_PositionOpened(Guid positionId, PositionView view, ExitReason? exitReason)
        {
            Print($"Position Opened: {view.Side} {view.NetQuantity} @ {view.AvgEntryPrice}");
        }

        protected override void OnPM_PositionClosed(Guid positionId, PositionView view, ExitReason? exitReason)
        {
            Print($"Position Closed: {exitReason} - P&L: {view.RealizedPnL}");
        }

        protected override void OnPM_TradeFinalized(Trade trade)
        {
            Print($"Trade Finalized: {trade.Side} {trade.Quantity} - Net P&L: {trade.NetPnL}");
        }

        #endregion
    }
}

