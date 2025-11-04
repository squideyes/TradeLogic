using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeLogic
{
    public sealed class PositionManager
    {
        private readonly object _sync = new object();

        private readonly PositionConfig _config;
        private readonly IIdGenerator _idGen;
        private readonly ILogger _log;

        private readonly Guid _positionId;

        private DateTime _currentET = DateTime.MinValue;

        private PositionState _state;
        private Side? _side;
        private int _openQty;
        private decimal _avgEntryPrice;
        private decimal _realizedPnl;
        private DateTime? _openedUtc;
        private DateTime? _closedUtc;

        private OrderSnapshot _entryOrder;
        private OrderSnapshot _slOrder;
        private OrderSnapshot _tpOrder;

        private string _exitOcoGroupId;
        private decimal? _armedSL;
        private decimal? _armedTP;
        private decimal? _armedStopLimitPrice;

        private readonly List<Fill> _entryFills = new List<Fill>();
        private readonly List<Fill> _exitFills = new List<Fill>();

        private decimal _intendedEntryPriceForSlippage;
        private bool _haveIntendedEntryPrice;

        public PositionManager(PositionConfig config, IIdGenerator idGen, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _idGen = idGen ?? new GuidIdGenerator();
            _log = logger ?? throw new ArgumentNullException(nameof(logger));

            _positionId = Guid.NewGuid();
            _state = PositionState.Flat;
        }

        // Public events - for strategy implementers
        // Position lifecycle events: (positionId, positionView, exitReason)
        public event Action<Guid, PositionView, ExitReason?> PositionOpened;
        public event Action<Guid, PositionView, ExitReason?> PositionClosed;

        // Trade event: (positionId, trade)
        public event Action<Guid, Trade> TradeFinalized;

        // Error event: (code, message, context)
        public event Action<string, string, object> ErrorOccurred;

        // Internal events - used by TradeLogicStrategyBase for plumbing
        internal event Action<Guid, OrderSnapshot> OrderSubmitted;
        internal event Action<Guid, OrderSnapshot> OrderAccepted;
        internal event Action<Guid, OrderSnapshot> OrderRejected;
        internal event Action<Guid, OrderSnapshot> OrderCanceled;
        internal event Action<Guid, OrderSnapshot> OrderExpired;
        internal event Action<Guid, OrderSnapshot> OrderWorking;
        internal event Action<Guid, OrderSnapshot, Fill> OrderPartiallyFilled;
        internal event Action<Guid, OrderSnapshot, Fill> OrderFilled;
        internal event Action<Guid, PositionView, ExitReason?> ExitArmed;
        internal event Action<Guid, PositionView, ExitReason?> PositionUpdated;
        internal event Action<Guid, PositionView, ExitReason?> PositionClosing;

        public Guid PositionId => _positionId;

        public PositionView GetPosition()
        {
            lock (_sync) { return BuildView(); }
        }

        public string SubmitEntry(OrderType type, Side side, int quantity,
            decimal stopLossPrice, decimal takeProfitPrice,
            decimal? limitPrice = null, decimal? stopPrice = null, decimal? stopLimitPrice = null)
        {
            lock (_sync)
            {
                // Submit entry with specified quantity
                var entryOrderId = SubmitEntryUnlocked(type, side, quantity, limitPrice, stopPrice);

                // Arm exits
                _armedSL = stopLossPrice;
                _armedTP = takeProfitPrice;
                _armedStopLimitPrice = stopLimitPrice;

                _log.Log(new ExitArmedLogEntry(_positionId, stopLossPrice, takeProfitPrice, "Exits armed with entry"));

                if (_state == PositionState.Open
                    && _slOrder == null && _tpOrder == null)
                {
                    SubmitOcoExits();
                }

                ExitArmed?.Invoke(_positionId, BuildView(), null);

                return entryOrderId;
            }
        }

        private string SubmitEntryUnlocked(OrderType type, Side side, int quantity,
            decimal? limitPrice = null, decimal? stopPrice = null)
        {
            GuardState(PositionState.Flat,
                "SubmitEntry only allowed from Flat.");

            ValidateEntryPrices(type, side, limitPrice, stopPrice);

            var clientOrderId = _idGen.NewId(_config.IdPrefix + "-ENTRY");

            var spec = new OrderSpec(
                clientOrderId,
                side,
                type,
                quantity,
                TimeInForce.FOK,
                limitPrice,
                stopPrice,
                null,
                true,
                false,
                null);

            _entryOrder = new OrderSnapshot(
                spec, OrderStatus.New, 0, null, null);

            _state = PositionState.PendingEntry;
            _side = side;
            _intendedEntryPriceForSlippage = 0m;
            _haveIntendedEntryPrice = false;

            _log.Log(new StateTransitionLogEntry(_positionId, PositionState.Flat.ToString(), PositionState.PendingEntry.ToString(), "SubmitEntry", $"Entry order submitted: {type} {side} {quantity} @ {limitPrice?.ToString() ?? stopPrice?.ToString() ?? "MKT"}"));

            if (type == OrderType.Limit || type == OrderType.StopLimit)
            {
                if (limitPrice.HasValue)
                {
                    _intendedEntryPriceForSlippage = limitPrice.Value;
                    _haveIntendedEntryPrice = true;
                }
            }
            else if (type == OrderType.Stop)
            {
                if (stopPrice.HasValue)
                {
                    _intendedEntryPriceForSlippage = stopPrice.Value;
                    _haveIntendedEntryPrice = true;
                }
            }

            OrderSubmitted?.Invoke(_positionId, _entryOrder);

            return clientOrderId;
        }



        public void GoFlat()
        {
            lock (_sync)
            {
                if (_state == PositionState.Flat
                    || _state == PositionState.Closed)
                {
                    return;
                }

                // Handle pending entry orders (Limit, Stop, StopLimit) that haven't filled yet
                if (_state == PositionState.PendingEntry)
                {
                    CancelEntryIfWorking(_entryOrder);
                    return;
                }

                if (_openQty == 0)
                    return;

                CancelExitIfWorking(_slOrder);

                CancelExitIfWorking(_tpOrder);

                SubmitImmediateExit(ExitReason.ManualGoFlat);

                _state = PositionState.Closing;

                _log.Log(new StateTransitionLogEntry(_positionId, PositionState.Open.ToString(), PositionState.Closing.ToString(), "GoFlat", "Manual flatten initiated"));

                PositionClosing?.Invoke(
                    _positionId, BuildView(), ExitReason.ManualGoFlat);
            }
        }

        public void OnTick(Tick tick)
        {
            lock (_sync)
            {
                _currentET = tick.OnET;

                var sessionEnd = SessionConfig.GetSessionEndET(tick.OnET);

                // Cancel pending entry orders at end of session
                if (_state == PositionState.PendingEntry && tick.OnET >= sessionEnd)
                {
                    CancelEntryIfWorking(_entryOrder);
                    return;
                }

                if (_state == PositionState.Open || _state == PositionState.PendingExit)
                {
                    if (tick.OnET >= sessionEnd)
                        HandleEndOfSessionExit();
                }
            }
        }

        public void OnOrderAccepted(OrderUpdate u)
        {
            lock (_sync)
            {
                var os = FindOrder(u.ClientOrderId);

                if (os == null) 
                    return;

                var spec = os.Spec.WithVenueOrderId(string.IsNullOrEmpty(
                    u.VenueOrderId) ? os.Spec.VenueOrderId : u.VenueOrderId);

                os = new OrderSnapshot(spec, OrderStatus.Accepted, 
                    os.FilledQuantity, os.AvgFillPrice, 
                        os.RejectOrCancelReason);
                
                ReplaceOrderSnapshot(os);

                OrderAccepted?.Invoke(_positionId, os);
            }
        }

        public void OnOrderRejected(OrderUpdate u)
        {
            lock (_sync)
            {
                var os = FindOrder(u.ClientOrderId);
                if (os == null) return;

                os = os.With(OrderStatus.Rejected, reason: u.Reason);
                ReplaceOrderSnapshot(os);
                OrderRejected?.Invoke(_positionId, os);

                if (os.Spec.IsEntry && _state == PositionState.PendingEntry)
                {
                    _state = PositionState.Flat;
                    _side = null;
                    _entryOrder = null;
                    _log.Log(new StateTransitionLogEntry(_positionId, PositionState.PendingEntry.ToString(), PositionState.Flat.ToString(), "OnOrderRejected", $"Entry order rejected: {u.Reason}"));
                    PositionUpdated?.Invoke(_positionId, BuildView(), null);
                }
                else if (os.Spec.IsExit)
                {
                    _log.Log(new ErrorLogEntry("EXIT_REJECTED", "Exit order rejected", LogLevel.Warn));
                    ErrorOccurred?.Invoke("EXIT_REJECTED", "Exit order rejected", os);
                }
            }
        }

        public void OnOrderCanceled(OrderUpdate u)
        {
            lock (_sync)
            {
                var os = FindOrder(u.ClientOrderId);
                if (os == null) return;

                os = os.With(OrderStatus.Canceled, reason: u.Reason);
                ReplaceOrderSnapshot(os);
                OrderCanceled?.Invoke(_positionId, os);

                if (os.Spec.IsEntry && _state == PositionState.PendingEntry)
                {
                    _state = PositionState.Flat;
                    _side = null;
                    _entryOrder = null;
                    _log.Log(new StateTransitionLogEntry(_positionId, PositionState.PendingEntry.ToString(), PositionState.Flat.ToString(), "OnOrderCanceled", $"Entry order canceled: {u.Reason}"));
                    PositionUpdated?.Invoke(_positionId, BuildView(), null);
                }
            }
        }

        public void OnOrderExpired(OrderUpdate u)
        {
            lock (_sync)
            {
                var os = FindOrder(u.ClientOrderId);
                if (os == null) return;

                os = os.With(OrderStatus.Expired, reason: u.Reason);
                ReplaceOrderSnapshot(os);
                OrderExpired?.Invoke(_positionId, os);
            }
        }

        public void OnOrderWorking(OrderUpdate u)
        {
            lock (_sync)
            {
                var os = FindOrder(u.ClientOrderId);
                if (os == null) return;

                os = os.With(OrderStatus.Working);
                ReplaceOrderSnapshot(os);
                OrderWorking?.Invoke(_positionId, os);
            }
        }

        public void OnOrderPartiallyFilled(string clientOrderId, string fillId, decimal price, int quantity, DateTime fillUtc)
        {
            lock (_sync)
            {
                var os = FindOrder(clientOrderId);
                if (os == null) return;

                var fill = MakeFillAndFee(clientOrderId, fillId, price, quantity, fillUtc);

                if (os.Spec.IsEntry)
                {
                    var newFilledQty = os.FilledQuantity + quantity;
                    var avgFill = ComputeNewAverage(os.AvgFillPrice, os.FilledQuantity, price, quantity);
                    os = os.With(OrderStatus.PartiallyFilled, filledQty: newFilledQty, avgFillPrice: avgFill);

                    ReplaceOrderSnapshot(os);
                    OrderPartiallyFilled?.Invoke(_positionId, os, fill);
                    _log.Log(new ErrorLogEntry("FOK_PARTIAL", "Entry received partial fill under FOK; request cancel and revert.", LogLevel.Warn));
                    ErrorOccurred?.Invoke("FOK_PARTIAL", "Entry received partial fill under FOK; request cancel and revert.", os);
                    return;
                }

                {
                    var newFilledQty = os.FilledQuantity + quantity;
                    var avgFill = ComputeNewAverage(os.AvgFillPrice, os.FilledQuantity, price, quantity);
                    os = os.With(OrderStatus.PartiallyFilled, filledQty: newFilledQty, avgFillPrice: avgFill);
                    ReplaceOrderSnapshot(os);

                    _exitFills.Add(fill);
                    UpdateRealizedAndNetOnExitFill(fill, os.Spec.Side);

                    OrderPartiallyFilled?.Invoke(_positionId, os, fill);
                    PositionUpdated?.Invoke(_positionId, BuildView(), null);
                }
            }
        }

        public void OnOrderFilled(string clientOrderId, string fillId, decimal price, int quantity, DateTime fillET)
        {
            lock (_sync)
            {
                var os = FindOrder(clientOrderId);
                if (os == null) return;

                var fill = MakeFillAndFee(clientOrderId, fillId, price, quantity, fillET);
                var newFilledQty = os.FilledQuantity + quantity;
                var avgFill = ComputeNewAverage(os.AvgFillPrice, os.FilledQuantity, price, quantity);
                os = os.With(OrderStatus.Filled, filledQty: newFilledQty, avgFillPrice: avgFill);
                ReplaceOrderSnapshot(os);

                if (os.Spec.IsEntry)
                {
                    _entryFills.Add(fill);
                    if (_openedUtc == null) _openedUtc = fillET;

                    _openQty += (os.Spec.Side == Side.Long ? quantity : -quantity);
                    _avgEntryPrice = ComputeNewAverage(_avgEntryPrice, Math.Abs(_openQty) - quantity, price, quantity);

                    if (!_haveIntendedEntryPrice)
                    {
                        _intendedEntryPriceForSlippage = price;
                        _haveIntendedEntryPrice = true;
                    }

                    OrderFilled?.Invoke(_positionId, os, fill);

                    if (_state != PositionState.Open)
                    {
                        _state = PositionState.Open;
                        _log.Log(new StateTransitionLogEntry(_positionId, PositionState.PendingEntry.ToString(), PositionState.Open.ToString(), "OnOrderFilled", $"Position opened: {_side} {Math.Abs(_openQty)} @ {price}"));
                        PositionOpened?.Invoke(_positionId, BuildView(), null);
                        if ((_armedSL.HasValue || _armedTP.HasValue) && _slOrder == null && _tpOrder == null)
                            SubmitOcoExits();
                    }
                }
                else if (os.Spec.IsExit)
                {
                    _exitFills.Add(fill);
                    UpdateRealizedAndNetOnExitFill(fill, os.Spec.Side);

                    OrderFilled?.Invoke(_positionId, os, fill);
                    PositionUpdated?.Invoke(_positionId, BuildView(), null);

                    if (_openQty == 0)
                    {
                        _state = PositionState.Closed;
                        _closedUtc = fillET;

                        CancelExitIfWorking(_slOrder);
                        CancelExitIfWorking(_tpOrder);

                        var trade = BuildTrade(DetectExitReasonFromLastFilledExit(os));
                        _log.Log(new TradeLogEntry(trade.TradeId, trade.PositionId, trade.Symbol, trade.Side.ToString(), trade.OpenedET, trade.ClosedET, trade.ExitReason.ToString(), trade.NetQty, trade.AvgEntryPrice, trade.AvgExitPrice, trade.RealizedPnl, trade.TotalFees, trade.Slippage, $"Trade closed: {trade.ExitReason}"));
                        PositionClosed?.Invoke(_positionId, BuildView(), trade.ExitReason);
                        TradeFinalized?.Invoke(_positionId, trade);
                    }
                    else
                    {
                        _state = PositionState.PendingExit;
                    }
                }
            }
        }

        private PositionView BuildView()
        {
            decimal unreal = 0m;
            return new PositionView(
                _state, _side, _openQty, _avgEntryPrice, _realizedPnl, unreal,
                _openedUtc, _closedUtc, _config.Symbol, _armedSL, _armedTP);
        }

        private void GuardState(PositionState expected, string messageIfNot)
        {
            if (_state != expected) throw new InvalidOperationException(messageIfNot);
        }

        private void ValidateEntryPrices(OrderType type, Side side, decimal? limitPrice, decimal? stopPrice)
        {
            if (type == OrderType.Limit && !limitPrice.HasValue)
                throw new ArgumentException("Limit price required for Limit entry.");
            if (type == OrderType.Stop && !stopPrice.HasValue)
                throw new ArgumentException("Stop price required for Stop entry.");
            if (type == OrderType.StopLimit && (!limitPrice.HasValue || !stopPrice.HasValue))
                throw new ArgumentException("Both Stop and Limit prices required for StopLimit entry.");
        }

        private OrderSnapshot FindOrder(string clientOrderId)
        {
            if (_entryOrder != null && _entryOrder.Spec.ClientOrderId == clientOrderId) return _entryOrder;
            if (_slOrder != null && _slOrder.Spec.ClientOrderId == clientOrderId) return _slOrder;
            if (_tpOrder != null && _tpOrder.Spec.ClientOrderId == clientOrderId) return _tpOrder;
            return null;
        }

        private void ReplaceOrderSnapshot(OrderSnapshot os)
        {
            if (_entryOrder != null && _entryOrder.Spec.ClientOrderId == os.Spec.ClientOrderId) { _entryOrder = os; return; }
            if (_slOrder != null && _slOrder.Spec.ClientOrderId == os.Spec.ClientOrderId) { _slOrder = os; return; }
            if (_tpOrder != null && _tpOrder.Spec.ClientOrderId == os.Spec.ClientOrderId) { _tpOrder = os; return; }
        }

        private void SubmitOcoExits()
        {
            if (_openQty == 0 || !_side.HasValue) return;

            _exitOcoGroupId = _idGen.NewId(_config.IdPrefix + "-OCO");

            var sideToClose = _side.Value == Side.Long ? Side.Short : Side.Long;
            var qty = Math.Abs(_openQty);
            var gtt = SessionConfig.GetSessionEndET(_currentET);

            if (_armedSL.HasValue)
            {
                var orderType = OrderType.Stop;
                decimal? limitPrice = null;

                if (_armedStopLimitPrice.HasValue)
                {
                    orderType = OrderType.StopLimit;
                    limitPrice = _armedStopLimitPrice;
                }

                _slOrder = MakeExitOrder(
                    "EXIT-SL",
                    sideToClose,
                    orderType,
                    qty,
                    limitPrice: limitPrice,
                    stopPrice: _armedSL,
                    gttUtc: gtt);
                OrderSubmitted?.Invoke(_positionId, _slOrder);
            }

            if (_armedTP.HasValue)
            {
                _tpOrder = MakeExitOrder(
                    "EXIT-TP",
                    sideToClose,
                    OrderType.Limit,
                    qty,
                    limitPrice: _armedTP,
                    stopPrice: null,
                    gttUtc: gtt);
                OrderSubmitted?.Invoke(_positionId, _tpOrder);
            }
        }

        private OrderSnapshot MakeExitOrder(
            string tag,
            Side side,
            OrderType type,
            int qty,
            decimal? limitPrice,
            decimal? stopPrice,
            DateTime gttUtc)
        {
            var coid = _idGen.NewId(_config.IdPrefix + "-" + tag);
            var spec = new OrderSpec(
                coid, side, type, qty, TimeInForce.GTD, limitPrice, stopPrice, gttUtc, false, true, _exitOcoGroupId);
            return new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
        }

        private void SubmitImmediateExit(ExitReason reason)
        {
            if (_openQty == 0 || !_side.HasValue) return;

            var sideToClose = _side.Value == Side.Long ? Side.Short : Side.Long;
            var qty = Math.Abs(_openQty);
            var gtt = SessionConfig.GetSessionEndET(_currentET);

            var coid = _idGen.NewId(_config.IdPrefix + "-EXIT-MKT");
            var spec = new OrderSpec(coid, sideToClose, OrderType.Market, qty, TimeInForce.GTD, null, null, gtt, false, true, null);
            var exit = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);

            OrderSubmitted?.Invoke(_positionId, exit);
            if (_slOrder == null) _slOrder = exit; else _tpOrder = exit;
        }



        private void CancelEntryIfWorking(OrderSnapshot os)
        {
            if (os == null) return;
            if (os.Status == OrderStatus.Filled || os.Status == OrderStatus.Canceled || os.Status == OrderStatus.Rejected || os.Status == OrderStatus.Expired)
                return;

            _log.Log(new ErrorLogEntry("CANCEL_REQUEST", "Please cancel working entry order", LogLevel.Warn));
            ErrorOccurred?.Invoke("CANCEL_REQUEST", "Please cancel working entry order", os);
        }

        private void CancelExitIfWorking(OrderSnapshot os)
        {
            if (os == null) return;
            if (os.Status == OrderStatus.Filled || os.Status == OrderStatus.Canceled || os.Status == OrderStatus.Rejected || os.Status == OrderStatus.Expired)
                return;

            _log.Log(new ErrorLogEntry("CANCEL_REQUEST", "Please cancel working exit order", LogLevel.Warn));
            ErrorOccurred?.Invoke("CANCEL_REQUEST", "Please cancel working exit order", os);
        }

        private void HandleEndOfSessionExit()
        {
            if (_openQty == 0) return;

            // GTDMarket: Let GTD exits work, but submit immediate market exit if none exist
            if (_slOrder == null && _tpOrder == null)
                SubmitImmediateExit(ExitReason.EndOfSession);

            _state = PositionState.Closing;
            _log.Log(new StateTransitionLogEntry(_positionId, PositionState.Open.ToString(), PositionState.Closing.ToString(), "EndOfSession", "End of session exit initiated"));
            PositionClosing?.Invoke(_positionId, BuildView(), ExitReason.EndOfSession);
        }

        private Fill MakeFillAndFee(string clientOrderId, string fillId, decimal price, int quantity, DateTime fillET)
        {
            var f = new Fill(clientOrderId, fillId, RoundToTick(price), quantity, 0m, fillET);
            var fee = TradovateFees.ComputeCommission(f, _config.Symbol);
            return f.WithCommission(fee);
        }

        private decimal RoundToTick(decimal price)
        {
            var ts = _config.TickSize;
            if (ts <= 0m) return price;
            var ticks = Math.Round(price / ts, 0, MidpointRounding.AwayFromZero);
            return ticks * ts;
        }

        private decimal ComputeNewAverage(decimal? prevAvg, int prevQty, decimal px, int qty)
        {
            var totalQty = prevQty + qty;
            if (totalQty <= 0) return px;
            var sum = (prevAvg.GetValueOrDefault() * prevQty) + (px * qty);
            return sum / totalQty;
        }

        private void UpdateRealizedAndNetOnExitFill(Fill fill, Side exitSide)
        {
            var signedQty = exitSide == Side.Short ? -fill.Quantity : fill.Quantity;
            var closeQty = Math.Min(Math.Abs(_openQty), Math.Abs(signedQty));
            if (closeQty <= 0) return;

            var pnlPerUnit = (_side == Side.Long)
                ? (fill.Price - _avgEntryPrice) * _config.PointValue
                : (_avgEntryPrice - fill.Price) * _config.PointValue;

            var realized = pnlPerUnit * closeQty - fill.Commission;
            _realizedPnl += realized;

            _openQty += signedQty;

            if (_openQty == 0)
                _avgEntryPrice = 0m;
        }

        private ExitReason DetectExitReasonFromLastFilledExit(OrderSnapshot filledExit)
        {
            if (filledExit == null) return ExitReason.ManualGoFlat;
            if (_slOrder != null && ReferenceEquals(filledExit, _slOrder)) return ExitReason.StopLoss;
            if (_tpOrder != null && ReferenceEquals(filledExit, _tpOrder)) return ExitReason.TakeProfit;

            if (_state == PositionState.Closed && _closedUtc.HasValue)
            {
                var et = _closedUtc.Value;
                var eos = SessionConfig.GetSessionEndET(et);
                if (et >= eos.AddMinutes(-1))
                    return ExitReason.EndOfSession;
            }

            return ExitReason.ManualGoFlat;
        }

        private Trade BuildTrade(ExitReason reason)
        {
            var totalEntryQty = _entryFills.Sum(f => f.Quantity);
            var totalExitQty = _exitFills.Sum(f => f.Quantity);
            var avgEntry = _entryFills.Any() ? _entryFills.Sum(f => f.Price * f.Quantity) / Math.Max(1, totalEntryQty) : 0m;
            var avgExit = _exitFills.Any() ? _exitFills.Sum(f => f.Price * f.Quantity) / Math.Max(1, totalExitQty) : 0m;

            var totalFees = _entryFills.Sum(f => f.Commission) + _exitFills.Sum(f => f.Commission);

            decimal slippage = 0m;
            if (_haveIntendedEntryPrice && totalEntryQty > 0)
            {
                var diff = Math.Abs(avgEntry - _intendedEntryPriceForSlippage);
                slippage = diff * totalEntryQty * _config.PointValue;
                var ticks = diff / _config.TickSize;
                if (ticks > _config.SlippageToleranceTicks)
                {
                    _log.Log(new SlippageWarningLogEntry(_positionId, _intendedEntryPriceForSlippage, avgEntry, ticks, _config.SlippageToleranceTicks, "Entry slippage exceeded tolerance"));
                    ErrorOccurred?.Invoke("SLIPPAGE_WARN", "Entry slippage exceeded tolerance.", new { ticks = ticks });
                }
            }

            return new Trade(
                Guid.NewGuid(),
                _positionId,
                _config.Symbol,
                _side ?? Side.Long,
                _openedUtc ?? _currentET,
                _closedUtc ?? _currentET,
                reason,
                Math.Abs(totalExitQty),
                avgEntry,
                avgExit,
                _realizedPnl,
                totalFees,
                slippage,
                _entryFills.ToArray(),
                _exitFills.ToArray());
        }
    }
}
