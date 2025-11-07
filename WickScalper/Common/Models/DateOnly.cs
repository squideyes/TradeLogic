using System;
using System.Globalization;

namespace WickScalper.Common
{
    [Serializable]
    public readonly struct DateOnly
        : IComparable<DateOnly>, IEquatable<DateOnly>
    {
        private readonly DateTime date;

        public int Year => date.Year;
        public int Month => date.Month;
        public int Day => date.Day;
        public DayOfWeek DayOfWeek => date.DayOfWeek;

        public DateOnly(int year, int month, int day)
        {
            date = new DateTime(
                year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        private DateOnly(DateTime dateTime) => date = dateTime.Date;

        public static DateOnly FromDateTime(DateTime dateTime) =>
            new DateOnly(dateTime.Date);

        public DateTime ToDateTime(TimeOnly time) => new DateTime(Year, Month,
            Day, time.Hour, time.Minute, time.Second, time.Millisecond);

        public DateTime ToDateTime() => new DateTime(Year, Month, Day);

        public int CompareTo(DateOnly other) => date.CompareTo(other.date);

        public bool Equals(DateOnly other) => date.Equals(other.date);

        public override bool Equals(object other) =>
            other is DateOnly d && Equals(d);

        public override int GetHashCode() => date.GetHashCode();

        public DateOnly AddDays(int days) => new DateOnly(date.AddDays(days));

        public static bool operator ==(DateOnly left, DateOnly right) =>
            left.Equals(right);

        public static bool operator !=(DateOnly left, DateOnly right) =>
            !left.Equals(right);

        public static bool operator <(DateOnly left, DateOnly right) =>
            left.CompareTo(right) < 0;

        public static bool operator <=(DateOnly left, DateOnly right) =>
            left.CompareTo(right) <= 0;

        public static bool operator >(DateOnly left, DateOnly right) =>
            left.CompareTo(right) > 0;

        public static bool operator >=(DateOnly left, DateOnly right) =>
            left.CompareTo(right) >= 0;

        public override string ToString() =>
            date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public string ToString(string format) =>
            date.ToString(format, CultureInfo.InvariantCulture);

        public static DateOnly MinValue => new DateOnly(DateTime.MinValue);
        public static DateOnly MaxValue => new DateOnly(DateTime.MaxValue);
    }
}
