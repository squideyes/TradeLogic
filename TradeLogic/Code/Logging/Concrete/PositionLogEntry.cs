using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class PositionLogEntry : LogEntryBase
    {
        public Guid PositionId { get; set; }
        public string State { get; set; }
        public string Side { get; set; }
        public int NetQuantity { get; set; }
        public decimal AvgEntryPrice { get; set; }
        public decimal RealizedPnL { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }

        public PositionLogEntry(
            Guid positionId,
            string state,
            string side,
            int netQuantity,
            decimal avgEntryPrice,
            decimal realizedPnL,
            string @event,
            string message,
            LogLevel level = LogLevel.Info) : base(level)
        {
            PositionId = positionId;
            State = state;
            Side = side;
            NetQuantity = netQuantity;
            AvgEntryPrice = avgEntryPrice;
            RealizedPnL = realizedPnL;
            Event = @event;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "positionId", PositionId },
                { "state", State },
                { "side", Side },
                { "netQuantity", NetQuantity },
                { "avgEntryPrice", AvgEntryPrice },
                { "realizedPnL", RealizedPnL },
                { "event", Event },
                { "message", Message }
            };
        }
    }
}

