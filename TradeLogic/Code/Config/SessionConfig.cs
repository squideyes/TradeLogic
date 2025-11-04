using System;

namespace TradeLogic
{
    public sealed class SessionConfig
    {
        public string TimeZoneId { get; set; }
        public TimeSpan SessionStartLocal { get; set; }
        public TimeSpan SessionEndLocal { get; set; }

        public SessionConfig()
        {
            TimeZoneId = "Eastern Standard Time";
            SessionStartLocal = new TimeSpan(9, 30, 0);
            SessionEndLocal = new TimeSpan(16, 0, 0);
        }

        public DateTime GetSessionStartET(DateTime etNow)
        {
            var etStart = etNow.Date + SessionStartLocal;
            return etStart;
        }

        public DateTime GetSessionEndET(DateTime etNow)
        {
            var etEnd = etNow.Date + SessionEndLocal;
            return etEnd;
        }
    }
}
