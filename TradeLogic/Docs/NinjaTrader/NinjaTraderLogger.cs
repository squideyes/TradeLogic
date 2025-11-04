using NinjaTrader.NinjaScript.Strategies;
using TradeLogic;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class NinjaTraderLogger : ILogger
    {
        private readonly Strategy strategy;

        public NinjaTraderLogger(Strategy strategy)
        {
            this.strategy = strategy;
        }

        public void Log(LogEntryBase entry) =>
            strategy.Print(entry.Serialize());
    }
}

