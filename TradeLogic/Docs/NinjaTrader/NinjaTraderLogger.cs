using NinjaTrader.NinjaScript.Strategies;
using TL = TradeLogic;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class NinjaTraderLogger : TL.ILogger
    {
        private readonly Strategy _strategy;

        public NinjaTraderLogger(Strategy strategy)
        {
            _strategy = strategy;
        }

        public void Log(TL.LogEntryBase entry)
        {
            if (entry is TL.TextLogEntry textEntry)
            {
                _strategy.Print($"[TEXT] {textEntry.Message}");
            }
            else if (entry is TL.ErrorLogEntry errorEntry)
            {
                _strategy.Print($"[ERROR] {errorEntry.ErrorCode}: {errorEntry.ErrorMessage}");
            }
            else if (entry is TL.StateTransitionLogEntry stateEntry)
            {
                _strategy.Print($"[STATE] {stateEntry.FromState} -> {stateEntry.ToState}: {stateEntry.Message}");
            }
            else if (entry is TL.OrderLogEntry orderEntry)
            {
                _strategy.Print($"[ORDER] {orderEntry.Status} - {orderEntry.ClientOrderId}");
            }
            else if (entry is TL.FillLogEntry fillEntry)
            {
                _strategy.Print($"[FILL] {fillEntry.ClientOrderId} - {fillEntry.Quantity} @ {fillEntry.Price}");
            }
            else if (entry is TL.TradeLogEntry tradeEntry)
            {
                _strategy.Print($"[TRADE] {tradeEntry.Side} {tradeEntry.NetQuantity} - P&L: {tradeEntry.RealizedPnL}");
            }
        }
    }
}

