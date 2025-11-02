using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class StartupLogEntry : LogEntryBase
    {
        public string SessionTime { get; set; }
        public double DefaultStopLoss { get; set; }
        public double DefaultTakeProfit { get; set; }
        public int PositionSize { get; set; }
        public double MaxDailyLoss { get; set; }
        public int MaxTradesPerDay { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "sessionTime", SessionTime },
                { "defaultStopLoss", DefaultStopLoss },
                { "defaultTakeProfit", DefaultTakeProfit },
                { "positionSize", PositionSize },
                { "maxDailyLoss", MaxDailyLoss },
                { "maxTradesPerDay", MaxTradesPerDay }
            };
        }
    }
}

