using System;

namespace TradeLogic
{
    public sealed class Bar
    {
        public DateTime OpenET { get; private set; }
        public decimal Open { get; private set; }
        public decimal High { get; private set; }
        public decimal Low { get; private set; }
        public decimal Close { get; private set; }
        public long Volume { get; private set; }

        public Bar(DateTime openET, decimal open, decimal high, decimal low, decimal close, long volume)
        {
            if (openET.Kind != DateTimeKind.Unspecified && openET.Kind != DateTimeKind.Local)
                throw new ArgumentException("Bar open time should be in ET (Eastern Time)", nameof(openET));

            OpenET = openET;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }
}

