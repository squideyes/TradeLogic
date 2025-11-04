using NinjaTrader.NinjaScript.Strategies;
using TLLogging = TradeLogic.Logging;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// ILogger implementation that forwards log entries to NinjaTrader's Print() method.
    /// </summary>
    public class NinjaTraderLogger : TLLogging.ILogger
    {
        private readonly Strategy _strategy;

        public NinjaTraderLogger(Strategy strategy)
        {
            _strategy = strategy;
        }

        public void Log(TLLogging.LogEntryBase entry)
        {
            if (entry is TLLogging.TextLogEntry textEntry)
            {
                _strategy.Print($"[TEXT] {textEntry.Message}");
            }
            else if (entry is TLLogging.ErrorLogEntry errorEntry)
            {
                _strategy.Print($"[ERROR] {errorEntry.ErrorCode}: {errorEntry.ErrorMessage}");
            }
            else if (entry is TLLogging.StateTransitionLogEntry stateEntry)
            {
                _strategy.Print($"[STATE] {stateEntry.FromState} -> {stateEntry.ToState}: {stateEntry.Message}");
            }
            else if (entry is TLLogging.OrderLogEntry orderEntry)
            {
                _strategy.Print($"[ORDER] {orderEntry.Status} - {orderEntry.ClientOrderId}");
            }
            else if (entry is TLLogging.FillLogEntry fillEntry)
            {
                _strategy.Print($"[FILL] {fillEntry.ClientOrderId} - {fillEntry.Quantity} @ {fillEntry.Price}");
            }
            else if (entry is TLLogging.TradeLogEntry tradeEntry)
            {
                _strategy.Print($"[TRADE] {tradeEntry.Side} {tradeEntry.NetQuantity} - P&L: {tradeEntry.RealizedPnL}");
            }
        }
    }
}

