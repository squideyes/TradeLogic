using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class Trade
    {
        public Guid TradeId { get; private set; }
        public Guid PositionId { get; private set; }
        public string Symbol { get; private set; }
        public Side Side { get; private set; }
        public DateTime OpenedET { get; private set; }
        public DateTime ClosedET { get; private set; }
        public ExitReason ExitReason { get; private set; }
        public int NetQty { get; private set; }
        public decimal AvgEntryPrice { get; private set; }
        public decimal AvgExitPrice { get; private set; }
        public decimal RealizedPnl { get; private set; }
        public decimal TotalFees { get; private set; }
        public decimal Slippage { get; private set; }
        public IReadOnlyList<Fill> EntryFills { get; private set; }
        public IReadOnlyList<Fill> ExitFills { get; private set; }

        public Trade(
            Guid tradeId,
            Guid positionId,
            string symbol,
            Side side,
            DateTime openedET,
            DateTime closedET,
            ExitReason exitReason,
            int netQty,
            decimal avgEntryPrice,
            decimal avgExitPrice,
            decimal realizedPnl,
            decimal totalFees,
            decimal slippage,
            IReadOnlyList<Fill> entryFills,
            IReadOnlyList<Fill> exitFills)
        {
            TradeId = tradeId;
            PositionId = positionId;
            Symbol = symbol;
            Side = side;
            OpenedET = openedET;
            ClosedET = closedET;
            ExitReason = exitReason;
            NetQty = netQty;
            AvgEntryPrice = avgEntryPrice;
            AvgExitPrice = avgExitPrice;
            RealizedPnl = realizedPnl;
            TotalFees = totalFees;
            Slippage = slippage;
            EntryFills = entryFills;
            ExitFills = exitFills;
        }
    }
}
