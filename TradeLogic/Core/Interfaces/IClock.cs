using System;

namespace TradeLogic
{
    public interface IClock
    {
        DateTime ETNow { get; }
    }
}
