using System;

namespace WickScalper.Common
{
    public sealed class Tick
    {
        public DateTime OnET { get; }
        public decimal Last { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public int Volume { get; }

        internal Tick(
            DateTime onET,
            decimal last, 
            decimal bid,
            decimal ask,
            int volume)
        {
            OnET = onET;
            Last = last;
            Bid = bid;
            Ask = ask;
            Volume = volume;
        }
    }
}

