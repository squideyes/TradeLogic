using System;

namespace TradeLogic
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
