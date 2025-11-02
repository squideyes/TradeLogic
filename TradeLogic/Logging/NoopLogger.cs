namespace TradeLogic
{
    public sealed class NoopLogger : ILogger
    {
        public void Error(string message) { }
        public void Info(string message) { }
        public void Warn(string message) { }
    }
}
