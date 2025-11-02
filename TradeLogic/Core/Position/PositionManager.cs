using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeLogic
{
    public sealed class PositionManager
    {
        private readonly object _sync = new object();

        private readonly PositionConfig _config;
        private readonly IClock _clock;
        private readonly IFeeModel _feeModel;
        private readonly IIdGenerator _idGen;
        private readonly ILogger _log;

        private readonly Guid _positionId;

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

        private readonly List<Fill> _entryFills = new List<Fill>();
        private readonly List<Fill> _exitFills = new List<Fill>();

        private decimal _intendedEntryPriceForSlippage;
        private bool _haveIntendedEntryPrice;

        private ExitAtSessionEndMode _eosMode;

        public PositionManager(PositionConfig config, IClock clock, 
            IFeeModel feeModel, IIdGenerator idGen, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? new SystemClock();
            _feeModel = feeModel ?? new FlatFeeModel(0m);
            _idGen = idGen ?? new GuidIdGenerator();
            _log = logger ?? new NoopLogger();

            _positionId = Guid.NewGuid();
            _state = PositionState.Flat;
            _eosMode = _config.ExitAtSessionEndMode;
        }

        // Order events: (positionId, orderSnapshot)
        public event Action<Guid, OrderSnapshot> OrderSubmitted;
        public event Action<Guid, OrderSnapshot> OrderAccepted;
        public event Action<Guid, OrderSnapshot> OrderRejected;
        public event Action<Guid, OrderSnapshot> OrderCanceled;
        public event Action<Guid, OrderSnapshot> OrderExpired;
        public event Action<Guid, OrderSnapshot> OrderWorking;

        // Order fill events: (positionId, orderSnapshot, fill)
        public event Action<Guid, OrderSnapshot, Fill> OrderPartiallyFilled;
        public event Action<Guid, OrderSnapshot, Fill> OrderFilled;

        // Position events: (positionId, positionView, exitReason)
        public event Action<Guid, PositionView, ExitReason?> PositionOpened;
        public event Action<Guid, PositionView, ExitReason?> ExitArmed;
        public event Action<Guid, PositionView, ExitReason?> ExitReplaced;
        public event Action<Guid, PositionView, ExitReason?> PositionUpdated;
        public event Action<Guid, PositionView, ExitReason?> PositionClosing;
        public event Action<Guid, PositionView, ExitReason?> PositionClosed;

        // Trade event: (positionId, trade)
        public event Action<Guid, Trade> TradeFinalized;

        // Error event: (code, message, context)
        public event Action<string, string, object> ErrorOccurred;

        public Guid PositionId => _positionId;

        public PositionView GetView()
        {
            lock (_sync) { return BuildView(); }
        }

        public void ConfigureEndOfSession(ExitAtSessionEndMode mode)
        {
            lock (_sync) { _eosMode = mode; }
        }

        public string SubmitEntry(OrderType type, Side side, int quantity, 
            decimal? limitPrice = null, decimal? stopPrice = null)
        {
            lock (_sync)
            {
                GuardState(PositionState.Flat, 
                    "SubmitEntry only allowed from Flat.");

                GuardQty(quantity);
                
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
        }

        public void ArmExits(
            decimal? stopLossPrice, decimal? takeProfitPrice)
        {
            lock (_sync)
            {
                _armedSL = stopLossPrice;
                _armedTP = takeProfitPrice;

                if (_state == PositionState.Open 
                    && _slOrder == null && _tpOrder == null)
                {
                    SubmitOcoExits();
                }

                ExitArmed?.Invoke(_positionId, BuildView(), null);
            }
        }

        public void ReplaceExits(
            decimal? newStopLossPrice, decimal? newTakeProfitPrice)
        {
            lock (_sync)
            {
                if (_state != PositionState.Open
                    && _state != PositionState.PendingExit
                    && _state != PositionState.Closing)
                {
                    throw new InvalidOperationException("ReplaceExits allowed only while position is open or closing.");
                }

                _armedSL = newStopLossPrice;
                _armedTP = newTakeProfitPrice;

                CancelExitIfWorking(_slOrder);

                CancelExitIfWorking(_tpOrder);

                if (_state == PositionState.Open && _openQty != 0)
                {
                    SubmitOcoExits();
                }

                ExitReplaced?.Invoke(_positionId, BuildView(), null);
            }
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

                if (_openQty == 0) 
                    return;

                CancelExitIfWorking(_slOrder);
                
                CancelExitIfWorking(_tpOrder);

                SubmitImmediateExit(ExitReason.ManualGoFlat);

                _state = PositionState.Closing;

                PositionClosing?.Invoke(
                    _positionId, BuildView(), ExitReason.ManualGoFlat);
            }
        }

        public void OnClock(DateTime utcNow)
        {
            lock (_sync)
            {
                if (_state == PositionState.Open || _state == PositionState.PendingExit)
                {
                    var sessionEnd = _config.Session.GetSessionEndUtc(utcNow);
                    
                    if (utcNow >= sessionEnd)
                        HandleEndOfSessionExit();
                }
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                if (_state != PositionState.Closed
                    && _state != PositionState.Flat)
                {
                    throw new InvalidOperationException(
                        "Reset allowed only when Closed or Flat.");
                }

                _state = PositionState.Flat;
                _side = null;
                _openQty = 0;
                _avgEntryPrice = 0m;
                _realizedPnl = 0m;
                _openedUtc = null;
                _closedUtc = null;

                _entryOrder = null;
                _slOrder = null;
                _tpOrder = null;

                _exitOcoGroupId = null;
                _armedSL = null;
                _armedTP = null;

                _entryFills.Clear();
                _exitFills.Clear();
                _intendedEntryPriceForSlippage = 0m;
                _haveIntendedEntryPrice = false;
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
                    PositionUpdated?.Invoke(_positionId, BuildView(), null);
                }
                else if (os.Spec.IsExit)
                {
                    _log.Warn("EXIT_REJECTED: Exit order rejected");
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
                    _log.Warn("FOK_PARTIAL: Entry received partial fill under FOK; request cancel and revert.");
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

        public void OnOrderFilled(string clientOrderId, string fillId, decimal price, int quantity, DateTime fillUtc)
        {
            lock (_sync)
            {
                var os = FindOrder(clientOrderId);
                if (os == null) return;

                var fill = MakeFillAndFee(clientOrderId, fillId, price, quantity, fillUtc);
                var newFilledQty = os.FilledQuantity + quantity;
                var avgFill = ComputeNewAverage(os.AvgFillPrice, os.FilledQuantity, price, quantity);
                os = os.With(OrderStatus.Filled, filledQty: newFilledQty, avgFillPrice: avgFill);
                ReplaceOrderSnapshot(os);

                if (os.Spec.IsEntry)
                {
                    _entryFills.Add(fill);
                    if (_openedUtc == null) _openedUtc = fillUtc;

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
                        _closedUtc = fillUtc;

                        CancelExitIfWorking(_slOrder);
                        CancelExitIfWorking(_tpOrder);

                        var trade = BuildTrade(DetectExitReasonFromLastFilledExit(os));
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

        private void GuardQty(int qty)
        {
            if (qty < _config.MinQty) throw new ArgumentOutOfRangeException(nameof(qty), "Quantity below MinQty.");
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
            var gtt = _config.Session.GetSessionEndUtc(_clock.UtcNow);

            if (_armedSL.HasValue)
            {
                _slOrder = MakeExitOrder(
                    "EXIT-SL",
                    sideToClose,
                    _config.UseStopLimitForSL ? OrderType.StopLimit : OrderType.Stop,
                    qty,
                    limitPrice: _config.UseStopLimitForSL ? MakeMarketableLimitPrice(sideToClose) : (decimal?)null,
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
            var gtt = _config.Session.GetSessionEndUtc(_clock.UtcNow);

            OrderSnapshot exit;
            if (_config.UseMarketForManualFlat || (reason == ExitReason.EndOfSession && _eosMode == ExitAtSessionEndMode.CancelAndMarket))
            {
                var coid = _idGen.NewId(_config.IdPrefix + "-EXIT-MKT");
                var spec = new OrderSpec(coid, sideToClose, OrderType.Market, qty, TimeInForce.GTD, null, null, gtt, false, true, null);
                exit = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            }
            else
            {
                var px = MakeMarketableLimitPrice(sideToClose);
                var coid = _idGen.NewId(_config.IdPrefix + "-EXIT-MLMT");
                var spec = new OrderSpec(coid, sideToClose, OrderType.Limit, qty, TimeInForce.GTD, px, null, gtt, false, true, null);
                exit = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            }

            OrderSubmitted?.Invoke(_positionId, exit);
            if (_slOrder == null) _slOrder = exit; else _tpOrder = exit;
        }

        private decimal MakeMarketableLimitPrice(Side sideToClose)
        {
            var ticks = Math.Max(1, _config.MarketableLimitOffsetTicks);
            var offset = ticks * _config.TickSize;

            if (sideToClose == Side.Short)
            {
                // Selling to close: price slightly below/at current to be marketable
                return Math.Max(0.01m, _avgEntryPrice - offset);
            }
            else
            {
                // Buying to close: price slightly above/at current to be marketable
                return _avgEntryPrice + offset;
            }
        }

        private void CancelExitIfWorking(OrderSnapshot os)
        {
            if (os == null) return;
            if (os.Status == OrderStatus.Filled || os.Status == OrderStatus.Canceled || os.Status == OrderStatus.Rejected || os.Status == OrderStatus.Expired)
                return;

            _log.Warn("CANCEL_REQUEST: Please cancel working exit order");
            ErrorOccurred?.Invoke("CANCEL_REQUEST", "Please cancel working exit order", os);
        }

        private void HandleEndOfSessionExit()
        {
            if (_openQty == 0) return;

            switch (_eosMode)
            {
                case ExitAtSessionEndMode.GTDMarket:
                    if (_slOrder == null && _tpOrder == null) SubmitImmediateExit(ExitReason.EndOfSession);
                    _state = PositionState.Closing;
                    PositionClosing?.Invoke(_positionId, BuildView(), ExitReason.EndOfSession);
                    break;

                case ExitAtSessionEndMode.GTDMarketableLimit:
                    if (_slOrder == null && _tpOrder == null)
                    {
                        _config.UseMarketForManualFlat = false;
                        SubmitImmediateExit(ExitReason.EndOfSession);
                    }
                    _state = PositionState.Closing;
                    PositionClosing?.Invoke(_positionId, BuildView(), ExitReason.EndOfSession);
                    break;

                case ExitAtSessionEndMode.CancelAndMarket:
                    CancelExitIfWorking(_slOrder);
                    CancelExitIfWorking(_tpOrder);
                    SubmitImmediateExit(ExitReason.EndOfSession);
                    _state = PositionState.Closing;
                    PositionClosing?.Invoke(_positionId, BuildView(), ExitReason.EndOfSession);
                    break;
            }
        }

        private Fill MakeFillAndFee(string clientOrderId, string fillId, decimal price, int quantity, DateTime fillUtc)
        {
            var f = new Fill(clientOrderId, fillId, RoundToTick(price), quantity, 0m, fillUtc);
            var fee = _feeModel.ComputeCommissionPerFill(f);
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
                var utc = _closedUtc.Value;
                var eos = _config.Session.GetSessionEndUtc(utc);
                if (utc >= eos.AddMinutes(-1))
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
                    ErrorOccurred?.Invoke("SLIPPAGE_WARN", "Entry slippage exceeded tolerance.", new { ticks = ticks });
            }

            return new Trade(
                Guid.NewGuid(),
                _positionId,
                _config.Symbol,
                _side ?? Side.Long,
                _openedUtc ?? _clock.UtcNow,
                _closedUtc ?? _clock.UtcNow,
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
