using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class EntrySubmissionLogEntry : LogEntryBase
    {
        public string TradeId { get; set; }
        public string Direction { get; set; }
        public double EntryPrice { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public int PositionSize { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "tradeId", TradeId },
                { "direction", Direction },
                { "entryPrice", EntryPrice },
                { "stopLoss", StopLoss },
                { "takeProfit", TakeProfit },
                { "positionSize", PositionSize }
            };
        }
    }
}

