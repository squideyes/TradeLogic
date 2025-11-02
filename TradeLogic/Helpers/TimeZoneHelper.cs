using System;

internal static class TimeZoneHelper
{
    public static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;
    public static readonly TimeZoneInfo ET = CreateET();

    private static TimeZoneInfo CreateET()
    {
        bool isWindows =
            Environment.OSVersion.Platform == PlatformID.Win32NT ||
            Environment.OSVersion.Platform == PlatformID.Win32Windows ||
            Environment.OSVersion.Platform == PlatformID.Win32S ||
            Environment.OSVersion.Platform == PlatformID.WinCE;

        var id = isWindows ? "Eastern Standard Time" : "America/New_York";

        return TimeZoneInfo.FindSystemTimeZoneById(id);
    }

    public static DateTime ToET(this DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Input must be UTC", nameof(utc));

        return TimeZoneInfo.ConvertTimeFromUtc(utc, ET);
    }
}
