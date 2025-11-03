namespace TradeLogic.Logging
{
    public interface ILogWriter
    {
        void Write(LogEntryBase entry);
    }
}

