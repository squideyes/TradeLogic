using System;

namespace WickScalper.Common
{
    public sealed class Bar
    {
        public DateTime OpenET { get; }
        public decimal Open { get; }

        public decimal High { get; private set; }
        public decimal Low { get; private set; }
        public decimal Close { get; private set; }
        public long Volume { get; private set; }

        internal Bar(Tick tick, DateTime openET)
        {
            OpenET = openET;
            Open = tick.Last;
            High = tick.Last;
            Low = tick.Last;
            Close = tick.Last;
            Volume = tick.Volume;
        }

        internal void Adjust(Tick tick)
        {
            High = Math.Max(High, tick.Last);
            Low = Math.Min(Low, tick.Last);
            Close = tick.Last;
            Volume += tick.Volume;
        }

        public decimal GetTrueRange(decimal? prevClose = null)
        {
            var highLow = High - Low;

            if (prevClose == null)
                return highLow;

            var highPrevClose = Math.Abs(High - prevClose.Value);

            var lowPrevClose = Math.Abs(Low - prevClose.Value);

            return Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));
        }
    }
}

