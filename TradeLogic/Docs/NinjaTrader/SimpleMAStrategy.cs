using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TL = TradeLogic;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Simple strategy designed to EXERCISE TradeLogicStrategyBase with maximum trade volume.
    /// NOT designed for profit - designed to test all TradeLogic functionality.
    ///
    /// VERSION: 1.0.3
    ///
    /// Exercises:
    /// - Market entries (every other trade)
    /// - Limit entries (every other trade)
    /// - Manual GoFlat() calls (periodic)
    /// - Stop loss and take profit exits
    /// - High frequency trading to stress-test position management
    /// </summary>
    public class SimpleMaStrategy : TradeLogicStrategyBase
    {
        private SMA _fastMA;
        private SMA _slowMA;
        private int _tradeCount = 0;
        private bool _useMarketEntry = true;
        private Random _random = new Random();
        private int _lastGoFlatTradeCount = -1;

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast MA Period", Order = 1, GroupName = "Parameters")]
        public int FastPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Slow MA Period", Order = 2, GroupName = "Parameters")]
        public int SlowPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Stop Loss Ticks", Order = 3, GroupName = "Parameters")]
        public double StopLossTicks { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Take Profit Ticks", Order = 4, GroupName = "Parameters")]
        public double TakeProfitTicks { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Limit Entry Offset Ticks", Order = 5, GroupName = "Parameters")]
        public double LimitOffsetTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "GoFlat Every N Trades", Order = 6, GroupName = "Parameters")]
        public int GoFlatFrequency { get; set; }

        #endregion

        #region TradeLogicStrategyBase Implementation

        protected override void OnSetDefaults()
        {
            base.OnSetDefaults();

            Description = @"Exercise TradeLogicStrategyBase with maximum trade volume";
            Name = "SimpleMaStrategy";

            // Override base class defaults for tick-by-tick trading
            Calculate = Calculate.OnEachTick;  // CRITICAL for Market Replay!
            EntriesPerDirection = 1;
            EntryHandling = EntryHandling.AllEntries;
            IsExitOnSessionCloseStrategy = false;
            OrderFillResolution = OrderFillResolution.Standard;
            StartBehavior = StartBehavior.WaitUntilFlat;
            TimeInForce = NinjaTrader.Cbi.TimeInForce.Gtc;
            BarsRequiredToTrade = 5;

            // Aggressive parameters for MAXIMUM TRADES (not profit!)
            FastPeriod = 2;          // Very fast = lots of crossovers
            SlowPeriod = 5;          // Very fast = lots of crossovers
            StopLossTicks = 1;       // SUPER tight = immediate exits
            TakeProfitTicks = 2;     // SUPER tight = immediate exits
            LimitOffsetTicks = 1;    // Close to market
            GoFlatFrequency = 3;     // GoFlat every 3rd trade
        }

        protected override void OnConfigure()
        {
            base.OnConfigure();
            // Add any additional configuration here
        }

        protected override TL.PositionConfig CreatePositionConfig()
        {
            return new TL.PositionConfig
            {
                Symbol = TL.SymbolHelper.Parse(Instrument.MasterInstrument.Name),
                TickSize = (decimal)Instrument.MasterInstrument.TickSize,
                PointValue = (decimal)Instrument.MasterInstrument.PointValue,
                IdPrefix = "SMA",
                SlippageToleranceTicks = 1  // Slippage should be extremely rare and overwhelmingly 1 tick
            };
        }

        protected override void OnTradeLogicInitialized()
        {
            base.OnTradeLogicInitialized();

            // Initialize indicators
            _fastMA = SMA(FastPeriod);
            _slowMA = SMA(SlowPeriod);

            // Add to chart for visualization
            AddChartIndicator(_fastMA);
            AddChartIndicator(_slowMA);

            Print($"SimpleMaStrategy v1.0.3 initialized - Fast: {FastPeriod}, Slow: {SlowPeriod}");
            Print($"Session: Start 6:30 AM, End 4:30 PM ET");
            Print($"Calculate: OnEachTick, SL={StopLossTicks} ticks, TP={TakeProfitTicks} ticks");
        }

        protected override void OnBarUpdateTradeLogic()
        {
            if (CurrentBar < 1)
                return;

            var position = PM.GetPosition();

            // Debug: Print state periodically
            if (_tradeCount < 5 || _tradeCount % 10 == 0)
                Print($"[Bar {CurrentBar}] State={position.State}, TradeCount={_tradeCount}, Time={Time[0]:HH:mm:ss}");

            // Exercise GoFlat() - call it periodically when position is open
            if (position.State == TL.PositionState.Open &&
                _tradeCount > 0 &&
                _tradeCount % GoFlatFrequency == 0 &&
                _lastGoFlatTradeCount != _tradeCount)
            {
                // 80% chance to GoFlat - we want to exercise this!
                if (_random.NextDouble() < 0.8)
                {
                    Print($"[GoFlat] Manually closing trade #{_tradeCount}");
                    _lastGoFlatTradeCount = _tradeCount;
                    PM.GoFlat();  // TradeLogicStrategyBase will log this
                    return;
                }
            }

            // Only enter when flat - wait for previous position to close
            if (position.State != TL.PositionState.Flat)
                return;

            // Enter on every bar when flat, alternating direction
            bool bullishCross = (_tradeCount % 2 == 0);  // Even trades = Long
            bool bearishCross = (_tradeCount % 2 == 1);  // Odd trades = Short

            // Calculate prices - tight stops/targets for quick exits
            decimal currentPrice = (decimal)Close[0];
            decimal tickSize = (decimal)Instrument.MasterInstrument.TickSize;
            decimal stopTicks = (decimal)StopLossTicks;
            decimal targetTicks = (decimal)TakeProfitTicks;

            // Alternate between Market and Limit to exercise both order types
            _useMarketEntry = !_useMarketEntry;
            _tradeCount++;

            if (bullishCross)
            {
                decimal stopLoss = currentPrice - (stopTicks * tickSize);
                decimal takeProfit = currentPrice + (targetTicks * tickSize);

                if (_useMarketEntry)
                {
                    // Market entry - exercises immediate fills
                    PM.SubmitEntry(TL.OrderType.Market, TL.Side.Long, 1, stopLoss, takeProfit);
                }
                else
                {
                    // Limit entry - exercises working orders
                    decimal limitPrice = currentPrice - ((decimal)LimitOffsetTicks * tickSize);
                    PM.SubmitEntry(TL.OrderType.Limit, TL.Side.Long, 1, stopLoss, takeProfit, limitPrice: limitPrice);
                }
            }
            else if (bearishCross)
            {
                decimal stopLoss = currentPrice + (stopTicks * tickSize);
                decimal takeProfit = currentPrice - (targetTicks * tickSize);

                if (_useMarketEntry)
                {
                    // Market entry - exercises immediate fills
                    PM.SubmitEntry(TL.OrderType.Market, TL.Side.Short, 1, stopLoss, takeProfit);
                }
                else
                {
                    // Limit entry - exercises working orders
                    decimal limitPrice = currentPrice + ((decimal)LimitOffsetTicks * tickSize);
                    PM.SubmitEntry(TL.OrderType.Limit, TL.Side.Short, 1, stopLoss, takeProfit, limitPrice: limitPrice);
                }
            }
        }

        #endregion

        // No need to override event handlers - TradeLogicStrategyBase already logs everything
        // through NinjaTraderLogger which outputs to the Output window via Print()
        //
        // All these events are automatically logged:
        // - OrderSubmitted, OrderAccepted, OrderWorking, OrderFilled, etc.
        // - PositionOpened, PositionClosed, TradeFinalized
        // - ErrorOccurred
        //
        // Override them only if you need custom behavior beyond logging
    }
}
