using System.Collections.Generic;

namespace WickScalper.Common
{
    public sealed class ErrorLogEntry : LogEntryBase
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ErrorLogEntry(
            string errorCode,
            string errorMessage) 
                : base(LogLevel.Error)
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

