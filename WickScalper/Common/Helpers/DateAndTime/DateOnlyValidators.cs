using static System.DayOfWeek;

namespace WickScalper.Common
{
    public static partial class DateOnlyValidators
    {
        public static bool IsWeekday(this DateOnly value) =>
            value.DayOfWeek >= Monday && value.DayOfWeek <= Friday;
    }
}
