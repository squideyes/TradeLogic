using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class TradeLogEntry : LogEntryBase
    {
        public Guid TradeId { get; set; }
        public Guid PositionId { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public DateTime OpenedET { get; set; }
        public DateTime ClosedET { get; set; }
        public string ExitReason { get; set; }
        public int NetQuantity { get; set; }
        public decimal AvgEntryPrice { get; set; }
        public decimal AvgExitPrice { get; set; }
        public decimal RealizedPnL { get; set; }
        public decimal TotalFees { get; set; }
        public decimal Slippage { get; set; }
        public string Message { get; set; }

        public TradeLogEntry(
            Guid tradeId,
            Guid positionId,
            Symbol symbol,
            string side,
            DateTime openedET,
            DateTime closedET,
            string exitReason,
            int netQuantity,
            decimal avgEntryPrice,
            decimal avgExitPrice,
            decimal realizedPnL,
            decimal totalFees,
            decimal slippage,
            string message,
            LogLevel level = LogLevel.Info) : base(level)
        {
            TradeId = tradeId;
            PositionId = positionId;
            Symbol = symbol.ToString();
            Side = side;
            OpenedET = openedET;
            ClosedET = closedET;
            ExitReason = exitReason;
            NetQuantity = netQuantity;
            AvgEntryPrice = avgEntryPrice;
            AvgExitPrice = avgExitPrice;
            RealizedPnL = realizedPnL;
            TotalFees = totalFees;
            Slippage = slippage;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "tradeId", TradeId },
                { "positionId", PositionId },
                { "symbol", Symbol },
                { "side", Side },
                { "openedET", OpenedET },
                { "closedET", ClosedET },
                { "exitReason", ExitReason },
                { "netQuantity", NetQuantity },
                { "avgEntryPrice", AvgEntryPrice },
                { "avgExitPrice", AvgExitPrice },
                { "realizedPnL", RealizedPnL },
                { "totalFees", TotalFees },
                { "slippage", Slippage },
                { "message", Message }
            };
        }
    }
}

