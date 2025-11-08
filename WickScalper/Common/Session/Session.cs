using System;

namespace WickScalper.Common
{
    public class Session
    {
        public static readonly DateOnly MinDate = new DateOnly(2024, 1, 2);
        public static readonly DateOnly MaxDate = new DateOnly(2028, 12, 22);

        public DateOnly Date { get; }
        public DateTime From { get; }
        public DateTime Until { get; }

        public Session(DateOnly date)
        {
            Date = date.Should().Satisfy(v => v.IsTradeDate());
            From = date.ToDateTime(new TimeOnly(6, 30, 0));
            Until = date.ToDateTime(new TimeOnly(16, 30, 0));
        }

        public bool IsInSession(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                && value >= From && value <= Until;
        }
    }
}