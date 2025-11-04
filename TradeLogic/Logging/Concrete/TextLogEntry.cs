using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class TextLogEntry : LogEntryBase
    {
        public string Message { get; set; }

        public TextLogEntry(string message, LogLevel level = LogLevel.Info) : base(level)
        {
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "message", Message }
            };
        }
    }
}

