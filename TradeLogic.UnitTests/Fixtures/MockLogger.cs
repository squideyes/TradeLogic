using System.Collections.Generic;

namespace TradeLogic.UnitTests.Fixtures
{
    public class MockLogger : ILogger
    {
        public List<string> InfoLogs { get; } = new List<string>();
        public List<string> WarnLogs { get; } = new List<string>();
        public List<string> ErrorLogs { get; } = new List<string>();

        public void Info(string message)
        {
            InfoLogs.Add(message);
        }

        public void Warn(string message)
        {
            WarnLogs.Add(message);
        }

        public void Error(string message)
        {
            ErrorLogs.Add(message);
        }

        public void Clear()
        {
            InfoLogs.Clear();
            WarnLogs.Clear();
            ErrorLogs.Clear();
        }
    }
}

