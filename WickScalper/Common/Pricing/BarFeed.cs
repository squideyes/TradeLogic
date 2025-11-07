using System;
namespace WickScalper.Common
{
    public sealed class BarFeed
    {
        private readonly int barSeconds;
        private readonly Action<Bar> onBarClosed;

        private Bar current = null;

        public BarFeed(
            Symbol symbol,
            Session session,
            int barSeconds,
            Action<Bar> onBarClosed)
        {
            Symbol = symbol;
            Session = session;
            this.barSeconds = barSeconds;
            this.onBarClosed = onBarClosed;
        }

        public Symbol Symbol { get; }
        public Session Session { get; }

        public void ProcessTick(Tick tick)
        {
            var openOn = GetOpenOnET(tick.OnET);

            if (current is null)
            {
            }
            else if (current.OpenOnET != openOn)
            {
                onBarClosed(current);
                current = null;
            }
            else
            {
            }
        }

        private DateTime GetOpenOnET(DateTime value)
        {
            var delta = value - Session.From;

            var totalSeconds = (long)Math.Floor(delta.TotalSeconds);

            var index = totalSeconds / barSeconds;

            return Session.From.AddSeconds(index * barSeconds);
        }
    }
}

