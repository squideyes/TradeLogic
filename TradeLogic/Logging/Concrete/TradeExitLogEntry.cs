using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class TradeExitLogEntry : LogEntryBase
    {
        public string ExitReason { get; set; }
        public double EntryPrice { get; set; }
        public double ExitPrice { get; set; }
        public string ExitTime { get; set; }
        public int BarsHeld { get; set; }
        public string PnL { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "exitReason", ExitReason },
                { "entryPrice", EntryPrice },
                { "exitPrice", ExitPrice },
                { "exitTime", ExitTime },
                { "barsHeld", BarsHeld },
                { "pnl", PnL }
            };
        }
    }
}

