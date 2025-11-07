using System;
using static System.DayOfWeek;


namespace WickScalper.Common
{
    public class Session
    {
        private static readonly DateOnly MinDate = new DateOnly(2024, 1, 2);
        private static readonly DateOnly MaxDate = new DateOnly(2028, 12, 22);

        public DateOnly Date { get; }
        public DateTime From { get; }
        public DateTime Until { get; }

        public Session(DateOnly date)
        {
            if (date < MinDate || date > MaxDate)
            {
                throw new ArgumentException(
                    $"Date must be between {MinDate} and {MaxDate}", nameof(date));
            }

            if (!IsWeekday(date))
            {
                throw new ArgumentException(
                    "Date must be a weekday (Monday-Friday)", nameof(date));
            }

            Date = date;
            From = date.ToDateTime(new TimeOnly(6, 30, 0));
            Until = date.ToDateTime(new TimeOnly(16, 30, 0));
        }

        private static bool IsWeekday(DateOnly date)=>
            date.DayOfWeek >= Monday && date.DayOfWeek <= Friday;
    }
}