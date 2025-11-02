using System;

namespace TradeLogic
{
    #region Default Implementations

    #endregion


    public sealed class SessionConfig
    {
        public string TimeZoneId { get; set; } // e.g., "America/New_York" or "Eastern Standard Time"
        public TimeSpan SessionStartLocal { get; set; } // e.g., 06:30
        public TimeSpan SessionEndLocal { get; set; } // e.g., 16:30

        public SessionConfig()
        {
            TimeZoneId = "Eastern Standard Time";
            SessionStartLocal = new TimeSpan(9, 30, 0);   // 9:30 AM ET
            SessionEndLocal = new TimeSpan(16, 0, 0);     // 4:00 PM ET
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

        private static TimeZoneInfo SafeFindTimeZone(string tzId)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(tzId); }
            catch
            {
                if (string.Equals(tzId, "America/New_York", StringComparison.OrdinalIgnoreCase))
                {
                    try { return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); } catch { }
                }
                if (string.Equals(tzId, "UTC", StringComparison.OrdinalIgnoreCase))
                    return TimeZoneInfo.Utc;
                return TimeZoneInfo.Utc;
            }
        }
    }
}
