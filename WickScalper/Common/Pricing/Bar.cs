using System;

namespace WickScalper.Common
{
    public sealed class Bar
    {
        public DateTime OpenOnET { get; private set; }
        public decimal Open { get; private set; }
        public decimal High { get; private set; }
        public decimal Low { get; private set; }
        public decimal Close { get; private set; }
        public long Volume { get; private set; }

        internal Bar(
            DateTime openOnET,
            decimal open,
            decimal high,
            decimal low,
            decimal close,
            long volume)
        {
            OpenOnET = openOnET;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }
}

