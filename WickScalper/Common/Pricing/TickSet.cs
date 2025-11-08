using System.Collections;
using System.Collections.Generic;

namespace WickScalper.Common
{
    public class TickSet : IEnumerable<Tick>
    {
        private readonly List<Tick> ticks = new List<Tick>();

        public TickSet(Symbol symbol, Session session)
            : this(KnownFutures.GetFuture(symbol), session)
        {
        }

        public TickSet(Future future, Session session)
        {
            Future = future.MayNot().BeNull();
            Session = session.MayNot().BeNull();
        }

        public Future Future { get; }
        public Session Session { get; }

        public int Count => ticks.Count;

        public void Add(Tick tick)
        {
            tick.MayNot().BeNull();
         
            if (ticks.Count > 0)
            {
                tick.OnET.Should().BeGreaterThanOrEqual(
                    ticks[ticks.Count - 1].OnET);
            }
            
            ticks.Add(tick);
        }

        public void AddRange(IEnumerable<Tick> ticks)
        {
            ticks.MayNot().BeNull();

            foreach (var tick in ticks)
                Add(tick);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<Tick> GetEnumerator() => ticks.GetEnumerator();
    }
}
