using System;
using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public sealed class SlippageWarningLogEntry : LogEntryBase
    {
        public Guid PositionId { get; set; }
        public decimal IntendedPrice { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal SlippageTicks { get; set; }
        public decimal ToleranceTicks { get; set; }
        public string Message { get; set; }

        public SlippageWarningLogEntry(
            Guid positionId,
            decimal intendedPrice,
            decimal actualPrice,
            decimal slippageTicks,
            decimal toleranceTicks,
            string message,
            LogLevel level = LogLevel.Warn) : base(level)
        {
            PositionId = positionId;
            IntendedPrice = intendedPrice;
            ActualPrice = actualPrice;
            SlippageTicks = slippageTicks;
            ToleranceTicks = toleranceTicks;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "positionId", PositionId },
                { "intendedPrice", IntendedPrice },
                { "actualPrice", ActualPrice },
                { "slippageTicks", SlippageTicks },
                { "toleranceTicks", ToleranceTicks },
                { "message", Message }
            };
        }
    }
}

