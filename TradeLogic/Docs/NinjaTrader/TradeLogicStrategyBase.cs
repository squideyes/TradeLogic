using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.Collections.Generic;
using TL = TradeLogic;
using TLLogging = TradeLogic.Logging;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Abstract base class for NinjaTrader strategies using TradeLogic.
    /// Handles all the plumbing: event wiring, order mapping, and callback forwarding.
    /// </summary>
    public abstract class TradeLogicStrategyBase : Strategy
    {
        protected TL.PositionManager PM { get; private set; }

        private Dictionary<string, Order> _clientOrderIdToNTOrder = new Dictionary<string, Order>();
        private Dictionary<Order, string> _ntOrderToClientOrderId = new Dictionary<Order, string>();
        private Dictionary<Order, int> _previousFilledQty = new Dictionary<Order, int>();
        private TLLogging.ILogger _logger;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                OnSetDefaults();
            }
            else if (State == State.Configure)
            {
                OnConfigure();
            }
            else if (State == State.DataLoaded)
            {
                _logger = new NinjaTraderLogger(this);
                var config = CreatePositionConfig();
                var idGen = new TL.GuidIdGenerator();

                PM = new TL.PositionManager(config, idGen, _logger);
                SubscribeToPositionManagerEvents();
                OnTradeLogicInitialized();
            }
            else if (State == State.Terminated)
            {
                if (PM != null)
                {
                    UnsubscribeFromPositionManagerEvents();
                    OnTradeLogicTerminated();
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            var tick = new TL.Tick(
                Time[0],
                (decimal)Close[0],
                (decimal)GetCurrentBid(),
                (decimal)GetCurrentAsk(),
                (int)Volume[0]
            );
            PM.OnTick(tick);

            OnBarUpdateTradeLogic();
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice,
            int quantity, int filled, double averageFillPrice, OrderState orderState,
            DateTime time, ErrorCode error, string comment)
        {
            base.OnOrderUpdate(order, limitPrice, stopPrice, quantity, filled, averageFillPrice, orderState, time, error, comment);

            if (PM == null || !_ntOrderToClientOrderId.TryGetValue(order, out string clientOrderId))
                return;

            var update = new TL.OrderUpdate(
                clientOrderId,
                order.OrderId,
                MapNTOrderStateToTradeLogicStatus(orderState),
                error != ErrorCode.NoError ? error.ToString() : null
            );

            switch (orderState)
            {
                case OrderState.Accepted:
                    PM.OnOrderAccepted(update);
                    break;

                case OrderState.Working:
                    PM.OnOrderWorking(update);
                    break;

                case OrderState.Rejected:
                    PM.OnOrderRejected(update);
                    break;

                case OrderState.Cancelled:
                    PM.OnOrderCanceled(update);
                    break;

                case OrderState.Filled:
                    PM.OnOrderFilled(clientOrderId, order.OrderId, (decimal)averageFillPrice, filled, time);
                    _previousFilledQty[order] = filled;
                    break;

                case OrderState.PartFilled:
                    int partialQty = filled - GetPreviousFilledQty(order);
                    PM.OnOrderPartiallyFilled(clientOrderId, Guid.NewGuid().ToString(),
                        (decimal)averageFillPrice, partialQty, time);
                    _previousFilledQty[order] = filled;
                    break;
            }
        }

        #region Abstract Methods - Strategy Must Implement

        /// <summary>
        /// Create the PositionConfig for TradeLogic.
        /// Called during State.DataLoaded.
        /// </summary>
        protected abstract TL.PositionConfig CreatePositionConfig();

        /// <summary>
        /// Called during OnBarUpdate after OnClock has been called.
        /// Implement your trading logic here.
        /// </summary>
        protected abstract void OnBarUpdateTradeLogic();

        #endregion

        #region Virtual Methods - Strategy Can Override

        /// <summary>
        /// Called during State.SetDefaults.
        /// Override to set strategy defaults.
        /// </summary>
        protected virtual void OnSetDefaults()
        {
            Calculate = Calculate.OnBarClose;
            EntriesPerDirection = 1;
            EntryHandling = EntryHandling.AllEntries;
            IsExitOnSessionCloseStrategy = false;
            OrderFillResolution = OrderFillResolution.Standard;
            StartBehavior = StartBehavior.WaitUntilFlat;
            TimeInForce = NinjaTrader.Cbi.TimeInForce.Gtc;
            BarsRequiredToTrade = 20;
        }

        /// <summary>
        /// Called during State.Configure.
        /// Override to configure strategy settings.
        /// </summary>
        protected virtual void OnConfigure() { }

        /// <summary>
        /// Called after TradeLogic PositionManager is initialized.
        /// Override to perform additional initialization.
        /// </summary>
        protected virtual void OnTradeLogicInitialized() { }

        /// <summary>
        /// Called when strategy is terminated.
        /// Override to perform cleanup.
        /// </summary>
        protected virtual void OnTradeLogicTerminated() { }

        #endregion

        #region TradeLogic Event Handlers - Internal (Do Not Override)

        private void OnPM_OrderSubmitted(Guid positionId, TL.OrderSnapshot orderSnapshot)
        {
            var spec = orderSnapshot.Spec;
            Order ntOrder = null;

            if (spec.IsEntry)
            {
                ntOrder = SubmitEntryOrder(spec);
            }
            else if (spec.IsExit)
            {
                ntOrder = SubmitExitOrder(spec);
            }

            if (ntOrder != null)
            {
                _clientOrderIdToNTOrder[spec.ClientOrderId] = ntOrder;
                _ntOrderToClientOrderId[ntOrder] = spec.ClientOrderId;
            }
        }

        private void OnPM_OrderAccepted(Guid positionId, TL.OrderSnapshot orderSnapshot) { }
        private void OnPM_OrderRejected(Guid positionId, TL.OrderSnapshot orderSnapshot) { }
        private void OnPM_OrderCanceled(Guid positionId, TL.OrderSnapshot orderSnapshot) { }
        private void OnPM_OrderExpired(Guid positionId, TL.OrderSnapshot orderSnapshot) { }
        private void OnPM_OrderWorking(Guid positionId, TL.OrderSnapshot orderSnapshot) { }
        private void OnPM_OrderPartiallyFilled(Guid positionId, TL.OrderSnapshot orderSnapshot, TL.Fill fill) { }
        private void OnPM_OrderFilled(Guid positionId, TL.OrderSnapshot orderSnapshot, TL.Fill fill) { }
        private void OnPM_PositionOpened(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason) { }
        private void OnPM_ExitArmed(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason) { }
        private void OnPM_PositionUpdated(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason) { }
        private void OnPM_PositionClosing(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason) { }
        private void OnPM_PositionClosed(Guid positionId, TL.PositionView view, TL.ExitReason? exitReason) { }
        private void OnPM_TradeFinalized(Guid positionId, TL.Trade trade) { }

        private void OnPM_ErrorOccurred(string code, string message, object context)
        {
            Print($"TradeLogic ERROR [{code}]: {message}");

            if (code == "CANCEL_REQUEST" && context is TL.OrderSnapshot os)
            {
                if (_clientOrderIdToNTOrder.TryGetValue(os.Spec.ClientOrderId, out Order ntOrder))
                {
                    CancelOrder(ntOrder);
                }
            }
        }

        #endregion

        #region Private Helpers

        private void SubscribeToPositionManagerEvents()
        {
            PM.OrderSubmitted += (posId, snap) => OnPM_OrderSubmitted(posId, snap);
            PM.OrderAccepted += (posId, snap) => OnPM_OrderAccepted(posId, snap);
            PM.OrderRejected += (posId, snap) => OnPM_OrderRejected(posId, snap);
            PM.OrderCanceled += (posId, snap) => OnPM_OrderCanceled(posId, snap);
            PM.OrderExpired += (posId, snap) => OnPM_OrderExpired(posId, snap);
            PM.OrderWorking += (posId, snap) => OnPM_OrderWorking(posId, snap);
            PM.OrderPartiallyFilled += (posId, snap, fill) => OnPM_OrderPartiallyFilled(posId, snap, fill);
            PM.OrderFilled += (posId, snap, fill) => OnPM_OrderFilled(posId, snap, fill);
            PM.PositionOpened += (posId, view, reason) => OnPM_PositionOpened(posId, view, reason);
            PM.ExitArmed += (posId, view, reason) => OnPM_ExitArmed(posId, view, reason);
            PM.PositionUpdated += (posId, view, reason) => OnPM_PositionUpdated(posId, view, reason);
            PM.PositionClosing += (posId, view, reason) => OnPM_PositionClosing(posId, view, reason);
            PM.PositionClosed += (posId, view, reason) => OnPM_PositionClosed(posId, view, reason);
            PM.TradeFinalized += (posId, trade) => OnPM_TradeFinalized(posId, trade);
            PM.ErrorOccurred += OnPM_ErrorOccurred;
        }

        private void UnsubscribeFromPositionManagerEvents()
        {
            PM.OrderSubmitted -= OnPM_OrderSubmitted;
            PM.OrderAccepted -= OnPM_OrderAccepted;
            PM.OrderRejected -= OnPM_OrderRejected;
            PM.OrderCanceled -= OnPM_OrderCanceled;
            PM.OrderExpired -= OnPM_OrderExpired;
            PM.OrderWorking -= OnPM_OrderWorking;
            PM.OrderPartiallyFilled -= OnPM_OrderPartiallyFilled;
            PM.OrderFilled -= OnPM_OrderFilled;
            PM.PositionOpened -= OnPM_PositionOpened;
            PM.ExitArmed -= OnPM_ExitArmed;
            PM.PositionUpdated -= OnPM_PositionUpdated;
            PM.PositionClosing -= OnPM_PositionClosing;
            PM.PositionClosed -= OnPM_PositionClosed;
            PM.TradeFinalized -= OnPM_TradeFinalized;
            PM.ErrorOccurred -= OnPM_ErrorOccurred;
        }

        private Order SubmitEntryOrder(TL.OrderSpec spec)
        {
            if (spec.OrderType == TL.OrderType.Market)
            {
                return spec.Side == TL.Side.Long
                    ? EnterLong(spec.Quantity, spec.ClientOrderId)
                    : EnterShort(spec.Quantity, spec.ClientOrderId);
            }
            else if (spec.OrderType == TL.OrderType.Limit)
            {
                return spec.Side == TL.Side.Long
                    ? EnterLongLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId)
                    : EnterShortLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId);
            }
            else if (spec.OrderType == TL.OrderType.Stop)
            {
                return spec.Side == TL.Side.Long
                    ? EnterLongStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId)
                    : EnterShortStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId);
            }
            return null;
        }

        private Order SubmitExitOrder(TL.OrderSpec spec)
        {
            Order dummyEntry = null;
            if (_clientOrderIdToNTOrder.Count > 0)
            {
                foreach (var kvp in _clientOrderIdToNTOrder)
                {
                    if (kvp.Value.OrderState == OrderState.Filled)
                    {
                        dummyEntry = kvp.Value;
                        break;
                    }
                }
            }

            if (spec.OrderType == TL.OrderType.Market)
            {
                return spec.Side == TL.Side.Long
                    ? ExitLong(0, spec.Quantity, spec.ClientOrderId, dummyEntry?.Name ?? "")
                    : ExitShort(0, spec.Quantity, spec.ClientOrderId, dummyEntry?.Name ?? "");
            }
            else if (spec.OrderType == TL.OrderType.Limit)
            {
                return spec.Side == TL.Side.Long
                    ? ExitLongLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "")
                    : ExitShortLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "");
            }
            else if (spec.OrderType == TL.OrderType.Stop)
            {
                return spec.Side == TL.Side.Long
                    ? ExitLongStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "")
                    : ExitShortStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "");
            }
            return null;
        }

        private TL.OrderStatus MapNTOrderStateToTradeLogicStatus(OrderState ntState)
        {
            switch (ntState)
            {
                case OrderState.Accepted: return TL.OrderStatus.Accepted;
                case OrderState.Working: return TL.OrderStatus.Working;
                case OrderState.Filled: return TL.OrderStatus.Filled;
                case OrderState.PartFilled: return TL.OrderStatus.PartiallyFilled;
                case OrderState.Cancelled: return TL.OrderStatus.Canceled;
                case OrderState.Rejected: return TL.OrderStatus.Rejected;
                default: return TL.OrderStatus.New;
            }
        }

        private int GetPreviousFilledQty(Order order)
        {
            return _previousFilledQty.TryGetValue(order, out int qty) ? qty : 0;
        }

        #endregion
    }
}

