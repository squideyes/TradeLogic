using NinjaTrader.NinjaScript.Strategies;
using TradeLogic.Logging;

namespace TradeLogic.NinjaTrader
{
    /// <summary>
    /// ILogger implementation that forwards log entries to NinjaTrader's Print() method.
    /// </summary>
    public class NinjaTraderLogger : ILogger
    {
        private readonly Strategy _strategy;

        public NinjaTraderLogger(Strategy strategy)
        {
            _strategy = strategy;
        }

        public void Log(LogEntryBase entry)
        {
            if (entry is TextLogEntry textEntry)
            {
                _strategy.Print($"[{textEntry.Category}] {textEntry.Message}");
            }
            else if (entry is ErrorLogEntry errorEntry)
            {
                _strategy.Print($"[ERROR] {errorEntry.Code}: {errorEntry.Message}");
            }
            else if (entry is StateTransitionLogEntry stateEntry)
            {
                _strategy.Print($"[STATE] {stateEntry.FromState} -> {stateEntry.ToState}: {stateEntry.Reason}");
            }
            else if (entry is OrderLogEntry orderEntry)
            {
                _strategy.Print($"[ORDER] {orderEntry.Action} - {orderEntry.ClientOrderId}");
            }
            else if (entry is FillLogEntry fillEntry)
            {
                _strategy.Print($"[FILL] {fillEntry.ClientOrderId} - {fillEntry.Quantity} @ {fillEntry.Price}");
            }
            else if (entry is TradeLogEntry tradeEntry)
            {
                _strategy.Print($"[TRADE] {tradeEntry.Side} {tradeEntry.Quantity} - P&L: {tradeEntry.RealizedPnL}");
            }
        }
    }
}

