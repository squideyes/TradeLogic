using System;

namespace WickScalper.Common
{
    public sealed class BarFeed
    {
        private readonly int barSeconds;
        private readonly Action<Bar> onBarClosed;

        private Bar current = null;

        public BarFeed(Symbol symbol, Session session,
            int barSeconds, Action<Bar> onBarClosed)
        {
            Symbol = symbol.Should().BeDefined();
            
            Session = session.MayNot().BeNull();

            this.barSeconds = barSeconds.Should()
                .BeBetween(5, 300).Satisfy(v => v % 5 == 0);
            
            this.onBarClosed = onBarClosed.MayNot().BeNull();
        }

        public Symbol Symbol { get; }
        public Session Session { get; }

        public void HandleTick(Tick tick)
        {
            tick.MayNot().BeNull();

            var openET = GetOpenET(tick.OnET);

            if (current is null)
            {
                current = new Bar(tick, openET);
            }
            else if (current.OpenET != openET)
            {
                onBarClosed(current);

                current = new Bar(tick, openET);
            }
            else
            {
                current.Adjust(tick);
            }
        }

        private DateTime GetOpenET(DateTime value)
        {
            var delta = value - Session.From;

            var totalSeconds = (int)Math.Floor(delta.TotalSeconds);

            var index = totalSeconds / barSeconds;

            return Session.From.AddSeconds(index * barSeconds);
        }
    }
}

