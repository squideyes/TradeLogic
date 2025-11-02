using System;

namespace TradeLogic
{
    internal sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
