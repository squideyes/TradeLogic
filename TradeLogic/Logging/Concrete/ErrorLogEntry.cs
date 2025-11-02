using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class ErrorLogEntry : LogEntryBase
    {
        public string ErrorMessage { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "errorMessage", ErrorMessage }
            };
        }
    }
}

