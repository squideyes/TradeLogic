using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class TextLogEntry : LogEntryBase
    {
        public string Message { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "message", Message }
            };
        }
    }
}

