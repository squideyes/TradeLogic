using System.IO;

namespace WickScalper.Common
{
    public class TickSetParser
    {
        public static TickSet ParseDTLBAV(
            Symbol symbol, Session session, string csv)
        {
            return ParseDTLBAV(
                KnownFutures.GetFuture(symbol), session, csv);
        }

        public static TickSet ParseDTLBAV(
            Future future, Session session, string csv)
        {
            var tickSet = new TickSet(future, session);

            var reader = new StringReader(csv);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                tickSet.Add(Tick.Parse(
                    tickSet.Future, tickSet.Session, line));
            };

            return tickSet;
        }
    }
}
