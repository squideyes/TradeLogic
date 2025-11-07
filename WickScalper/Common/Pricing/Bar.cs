using System;

namespace WickScalper.Common
{
    public sealed class Bar
    {
        public DateTime OpenOnET { get; internal set; }
        public decimal Open { get; internal set; }
        public decimal High { get; internal set; }
        public decimal Low { get; internal set; }
        public decimal Close { get; internal set; }
        public long Volume { get; internal set; }
    }
}

