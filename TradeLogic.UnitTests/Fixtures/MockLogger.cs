using System.Collections.Generic;
using TradeLogic.Logging;

namespace TradeLogic.UnitTests.Fixtures
{
    public class MockLogger : ILogger
    {
        public List<LogEntryBase> Logs { get; } = new List<LogEntryBase>();

        public void Log(LogEntryBase entry)
        {
            Logs.Add(entry);
        }

        public void Clear()
        {
            Logs.Clear();
        }
    }
}

