using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.Collections.Generic;

namespace TradeLogic.NinjaTrader
{
    /// <summary>
    /// Abstract base class for NinjaTrader strategies using TradeLogic.
    /// Handles all the plumbing: event wiring, order mapping, and callback forwarding.
    /// </summary>
    public abstract class TradeLogicStrategyBase : Strategy
    {
        protected PositionManager PM { get; private set; }

        private Dictionary<string, Order> _clientOrderIdToNTOrder = new Dictionary<string, Order>();
        private Dictionary<Order, string> _ntOrderToClientOrderId = new Dictionary<Order, string>();
        private Dictionary<Order, int> _previousFilledQty = new Dictionary<Order, int>();
        private NinjaTraderLogger _logger;

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
                var feeModel = CreateFeeModel();
                var idGen = new GuidIdGenerator();

                PM = new PositionManager(config, feeModel, idGen, _logger);
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

            var tick = new Tick(
                Time[0],
                (decimal)Close[0],
                (decimal)GetCurrentBid(),
                (decimal)GetCurrentAsk(),
                (int)Volume[0]
            );
            PM.OnClock(tick);

            OnBarUpdateTradeLogic();
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice,
            int quantity, int filled, double averageFillPrice, OrderState orderState,
            DateTime time, ErrorCode error)
        {
            if (!_ntOrderToClientOrderId.TryGetValue(order, out string clientOrderId))
                return;

            var update = new OrderUpdate(
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
        protected abstract PositionConfig CreatePositionConfig();

        /// <summary>
        /// Create the fee model for TradeLogic.
        /// Called during State.DataLoaded.
        /// </summary>
        protected abstract IFeeModel CreateFeeModel();

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
            TimeInForce = TimeInForce.Gtc;
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

        #region TradeLogic Event Handlers - Virtual (Override to Customize)

        protected virtual void OnPM_OrderSubmitted(Guid positionId, OrderSnapshot orderSnapshot)
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

        protected virtual void OnPM_OrderAccepted(Guid positionId, OrderSnapshot orderSnapshot) { }
        protected virtual void OnPM_OrderRejected(Guid positionId, OrderSnapshot orderSnapshot) { }
        protected virtual void OnPM_OrderCanceled(Guid positionId, OrderSnapshot orderSnapshot) { }
        protected virtual void OnPM_OrderExpired(Guid positionId, OrderSnapshot orderSnapshot) { }
        protected virtual void OnPM_OrderWorking(Guid positionId, OrderSnapshot orderSnapshot) { }
        protected virtual void OnPM_OrderPartiallyFilled(Guid positionId, OrderSnapshot orderSnapshot, Fill fill) { }
        protected virtual void OnPM_OrderFilled(Guid positionId, OrderSnapshot orderSnapshot, Fill fill) { }
        protected virtual void OnPM_PositionOpened(Guid positionId, PositionView view, ExitReason? exitReason) { }
        protected virtual void OnPM_ExitArmed(Guid positionId, PositionView view) { }
        protected virtual void OnPM_ExitReplaced(Guid positionId, PositionView view) { }
        protected virtual void OnPM_PositionUpdated(Guid positionId, PositionView view, ExitReason? exitReason) { }
        protected virtual void OnPM_PositionClosing(Guid positionId, PositionView view, ExitReason? exitReason) { }
        protected virtual void OnPM_PositionClosed(Guid positionId, PositionView view, ExitReason? exitReason) { }
        protected virtual void OnPM_TradeFinalized(Trade trade) { }

        protected virtual void OnPM_ErrorOccurred(string code, string message, object context)
        {
            Print($"TradeLogic ERROR [{code}]: {message}");

            if (code == "CANCEL_REQUEST" && context is OrderSnapshot os)
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
            PM.OrderSubmitted += OnPM_OrderSubmitted;
            PM.OrderAccepted += OnPM_OrderAccepted;
            PM.OrderRejected += OnPM_OrderRejected;
            PM.OrderCanceled += OnPM_OrderCanceled;
            PM.OrderExpired += OnPM_OrderExpired;
            PM.OrderWorking += OnPM_OrderWorking;
            PM.OrderPartiallyFilled += OnPM_OrderPartiallyFilled;
            PM.OrderFilled += OnPM_OrderFilled;
            PM.PositionOpened += OnPM_PositionOpened;
            PM.ExitArmed += OnPM_ExitArmed;
            PM.ExitReplaced += OnPM_ExitReplaced;
            PM.PositionUpdated += OnPM_PositionUpdated;
            PM.PositionClosing += OnPM_PositionClosing;
            PM.PositionClosed += OnPM_PositionClosed;
            PM.TradeFinalized += OnPM_TradeFinalized;
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
            PM.ExitReplaced -= OnPM_ExitReplaced;
            PM.PositionUpdated -= OnPM_PositionUpdated;
            PM.PositionClosing -= OnPM_PositionClosing;
            PM.PositionClosed -= OnPM_PositionClosed;
            PM.TradeFinalized -= OnPM_TradeFinalized;
            PM.ErrorOccurred -= OnPM_ErrorOccurred;
        }

        private Order SubmitEntryOrder(OrderSpec spec)
        {
            if (spec.Type == OrderType.Market)
            {
                return spec.Side == Side.Long
                    ? EnterLong(spec.Quantity, spec.ClientOrderId)
                    : EnterShort(spec.Quantity, spec.ClientOrderId);
            }
            else if (spec.Type == OrderType.Limit)
            {
                return spec.Side == Side.Long
                    ? EnterLongLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId)
                    : EnterShortLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId);
            }
            else if (spec.Type == OrderType.Stop)
            {
                return spec.Side == Side.Long
                    ? EnterLongStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId)
                    : EnterShortStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId);
            }
            return null;
        }

        private Order SubmitExitOrder(OrderSpec spec)
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

            if (spec.Type == OrderType.Market)
            {
                return spec.Side == Side.Long
                    ? ExitLong(0, spec.Quantity, spec.ClientOrderId, dummyEntry?.Name ?? "")
                    : ExitShort(0, spec.Quantity, spec.ClientOrderId, dummyEntry?.Name ?? "");
            }
            else if (spec.Type == OrderType.Limit)
            {
                return spec.Side == Side.Long
                    ? ExitLongLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "")
                    : ExitShortLimit(0, true, spec.Quantity, (double)spec.LimitPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "");
            }
            else if (spec.Type == OrderType.Stop)
            {
                return spec.Side == Side.Long
                    ? ExitLongStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "")
                    : ExitShortStopMarket(0, true, spec.Quantity, (double)spec.StopPrice.Value, spec.ClientOrderId, dummyEntry?.Name ?? "");
            }
            return null;
        }

        private OrderStatus MapNTOrderStateToTradeLogicStatus(OrderState ntState)
        {
            switch (ntState)
            {
                case OrderState.Accepted: return OrderStatus.Accepted;
                case OrderState.Working: return OrderStatus.Working;
                case OrderState.Filled: return OrderStatus.Filled;
                case OrderState.PartFilled: return OrderStatus.PartiallyFilled;
                case OrderState.Cancelled: return OrderStatus.Canceled;
                case OrderState.Rejected: return OrderStatus.Rejected;
                default: return OrderStatus.New;
            }
        }

        private int GetPreviousFilledQty(Order order)
        {
            return _previousFilledQty.TryGetValue(order, out int qty) ? qty : 0;
        }

        #endregion
    }
}

