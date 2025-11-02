using System;

namespace TradeLogic
{
    #region Default Implementations

    #endregion


    internal sealed class SessionConfig
    {
        public string TimeZoneId { get; set; } // e.g., "America/New_York" or "Eastern Standard Time"
        public TimeSpan SessionStartLocal { get; set; } // e.g., 06:30
        public TimeSpan SessionEndLocal { get; set; } // e.g., 16:30

        public SessionConfig()
        {
            TimeZoneId = "UTC";
            SessionStartLocal = TimeSpan.Zero;
            SessionEndLocal = new TimeSpan(23, 59, 59);
        }

        public DateTime GetSessionStartUtc(DateTime utcNow)
        {
            var tz = SafeFindTimeZone(TimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            var localStart = localNow.Date + SessionStartLocal;
            return TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
        }

        public DateTime GetSessionEndUtc(DateTime utcNow)
        {
            var tz = SafeFindTimeZone(TimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
            var localEnd = localNow.Date + SessionEndLocal;
            return TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);
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
