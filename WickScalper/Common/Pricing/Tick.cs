using System;

namespace WickScalper.Common
{
    public sealed class Tick
    {
        public DateTime OnET { get; private set; }
        public decimal Last { get; private set; }
        public decimal Bid { get; private set; }
        public decimal Ask { get; private set; }
        public int Volume { get; private set; }

        public Tick(
            DateTime onET,
            decimal last, 
            decimal bid,
            decimal ask,
            int volume)
        {
            if (onET.Kind != DateTimeKind.Unspecified && onET.Kind != DateTimeKind.Local)
                throw new ArgumentException("Tick time should be in ET (Eastern Time)", nameof(onET));

            OnET = onET;
            Last = last;
            Bid = bid;
            Ask = ask;
            Volume = volume;
        }
    }
}

