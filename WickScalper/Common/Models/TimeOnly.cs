using System;

namespace WickScalper.Common
{
    [Serializable]
    public readonly struct TimeOnly
    {
        public int Hour { get; }
        public int Minute { get; }
        public int Second { get; }
        public int Millisecond { get; }

        public TimeOnly(int hour, int minute, int second = 0, int millisecond = 0)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
            Millisecond = millisecond;
        }

        public static TimeOnly FromDateTime(DateTime dateTime)
        {
            return new TimeOnly(dateTime.Hour,
                dateTime.Minute, dateTime.Second, dateTime.Millisecond);
        }
    }
}
