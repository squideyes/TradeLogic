using System;

namespace TradeLogic
{
    public sealed class PositionView
    {
        public PositionState State { get; private set; }
        public Side? Side { get; private set; }
        public int NetQuantity { get; private set; }
        public decimal AvgEntryPrice { get; private set; }
        public decimal RealizedPnl { get; private set; }
        public decimal UnrealizedPnl { get; private set; }
        public DateTime? OpenET { get; private set; }
        public DateTime? ClosedET { get; private set; }
        public string Symbol { get; private set; }
        public decimal? StopLossPrice { get; private set; }
        public decimal? TakeProfitPrice { get; private set; }

        public PositionView(
            PositionState state,
            Side? side,
            int netQty,
            decimal avgEntry,
            decimal realizedPnl,
            decimal unrealizedPnl,
            DateTime? openET,
            DateTime? closedET,
            string symbol,
            decimal? sl,
            decimal? tp)
        {
            State = state;
            Side = side;
            NetQuantity = netQty;
            AvgEntryPrice = avgEntry;
            RealizedPnl = realizedPnl;
            UnrealizedPnl = unrealizedPnl;
            OpenET = openET;
            ClosedET = closedET;
            Symbol = symbol;
            StopLossPrice = sl;
            TakeProfitPrice = tp;
        }
    }
}
