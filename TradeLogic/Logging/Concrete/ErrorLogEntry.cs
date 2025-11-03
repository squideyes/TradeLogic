using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public sealed class ErrorLogEntry : LogEntryBase
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ErrorLogEntry(string errorCode, string errorMessage, LogLevel level = LogLevel.Error) : base(level)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "errorCode", ErrorCode },
                { "errorMessage", ErrorMessage }
            };
        }
    }
}

