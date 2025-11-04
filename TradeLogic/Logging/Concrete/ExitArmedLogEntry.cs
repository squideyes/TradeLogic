using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class ExitArmedLogEntry : LogEntryBase
    {
        public Guid PositionId { get; set; }
        public decimal? StopLossPrice { get; set; }
        public decimal? TakeProfitPrice { get; set; }
        public string Message { get; set; }

        public ExitArmedLogEntry(
            Guid positionId,
            decimal? stopLossPrice,
            decimal? takeProfitPrice,
            string message,
            LogLevel level = LogLevel.Info) : base(level)
        {
            PositionId = positionId;
            StopLossPrice = stopLossPrice;
            TakeProfitPrice = takeProfitPrice;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            var data = new Dictionary<string, object>
            {
                { "positionId", PositionId },
                { "message", Message }
            };

            if (StopLossPrice.HasValue)
                data["stopLossPrice"] = StopLossPrice.Value;

            if (TakeProfitPrice.HasValue)
                data["takeProfitPrice"] = TakeProfitPrice.Value;

            return data;
        }
    }
}

