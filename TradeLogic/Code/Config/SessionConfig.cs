using System;

namespace TradeLogic
{
    /// <summary>
    /// Standard trading session configuration: 6:30 AM - 4:30 PM Eastern Time.
    /// This is a constant and cannot be changed.
    /// </summary>
    public static class SessionConfig
    {
        /// <summary>
        /// Session start time: 6:30 AM Eastern Time
        /// </summary>
        public static readonly TimeSpan SessionStartLocal = new TimeSpan(6, 30, 0);

        /// <summary>
        /// Session end time: 4:30 PM Eastern Time
        /// </summary>
        public static readonly TimeSpan SessionEndLocal = new TimeSpan(16, 30, 0);

        /// <summary>
        /// Time zone: Eastern Standard Time
        /// </summary>
        public static readonly string TimeZoneId = "Eastern Standard Time";

        /// <summary>
        /// Get the session start time for a given Eastern Time date.
        /// </summary>
        /// <param name="etNow">Current Eastern Time</param>
        /// <returns>Session start time for the date</returns>
        public static DateTime GetSessionStartET(DateTime etNow)
        {
            return etNow.Date + SessionStartLocal;
        }

        /// <summary>
        /// Get the session end time for a given Eastern Time date.
        /// </summary>
        /// <param name="etNow">Current Eastern Time</param>
        /// <returns>Session end time for the date</returns>
        public static DateTime GetSessionEndET(DateTime etNow)
        {
            return etNow.Date + SessionEndLocal;
        }
    }
}
