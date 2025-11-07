using System;
using static System.Environment;
using static System.PlatformID;

namespace WickScalper.Common
{
    internal static class TimeZoneHelper
    {
        public static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;
        public static readonly TimeZoneInfo ET = GetEtTzi();

        private static TimeZoneInfo GetEtTzi()
        {
            bool isWindows = OSVersion.Platform == Win32NT
                || OSVersion.Platform == Win32Windows
                || OSVersion.Platform == Win32S
                || OSVersion.Platform == WinCE;

            return TimeZoneInfo.FindSystemTimeZoneById(
                isWindows ? "Eastern Standard Time" : "America/New_York");
        }

        public static DateTime ToET(this DateTime utc)
        {
            if (utc.Kind != DateTimeKind.Utc)
                throw new ArgumentException(nameof(utc));

            return TimeZoneInfo.ConvertTimeFromUtc(utc, ET);
        }
    }
}