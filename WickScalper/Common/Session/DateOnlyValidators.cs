namespace WickScalper.Common
{
    public static partial class DateOnlyValidators
    {
        public static bool IsTradeDate(this DateOnly date)
        {
            return date >= Session.MinDate
                && date <= Session.MaxDate
                && date.IsWeekday()
                && !date.IsHoliday()
                && !date.IsEarlyCloseDay()
                && !date.IsReducedLiquidityDay();
        }

        private static bool IsHoliday(this DateOnly date)
        {
            return date.IsNewYearsDay()
                || date.IsChristmas()
                || date.IsGoodFriday()
                || date.IsIndependenceDay()
                || date.IsThanksgivingDay();
        }

        private static bool IsEarlyCloseDay(this DateOnly date)
        {
            return date.IsMartinLutherKingDay()
                || date.IsPresidentsDay()
                || date.IsMemorialDay()
                || date.IsJuneteenth()
                || date.IsLaborDay()
                || date.IsBlackFriday()
                || date.IsChristmasEve()
                || date.IsNewYearsEve();
        }

        private static bool IsReducedLiquidityDay(this DateOnly date) =>
            date.IsEasterMonday() || date.IsBoxingDay();
    }
}