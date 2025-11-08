using System;

namespace WickScalper.Common
{
    public sealed class Tick
    {
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

        public DateTime OnET { get; }
        public decimal Last { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public int Volume { get; }

        internal static Tick Parse(Future future, Session session, string line)
        {
            var fields = line.Split(',');

            var onET = DateTime.ParseExact(
                fields[0] + " " + fields[1], "yyyyMMdd HHmmssfff", null)
                    .Should().Satisfy(v => session.IsInSession(v));

            var bid = decimal.Parse(fields[3])
                .Should().BeGreaterThan(0).Satisfy(v => future.IsPrice(v));

            var ask = decimal.Parse(fields[4]).Should()
                .BeGreaterThanOrEqual(bid).Satisfy(v => future.IsPrice(v));

            var last = decimal.Parse(fields[2]).Should()
                .BeBetween(bid, ask).Satisfy(v => future.IsPrice(v));

            var volume = int.Parse(fields[5]).Should().BeGreaterThan(0);

            return new Tick(onET, last, bid, ask, volume);
        }
    }
}

