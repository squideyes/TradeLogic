namespace WickScalper.Common
{
    public static class GuardClauseExtensions
    {
        public static MayNotResult<T> MayNot<T>(this T value, string parameterName = null)
        {
            var context = new GuardClauseContext<T>(value, parameterName);

            return new MayNotResult<T>(context);
        }

        public static ShouldResult<T> Should<T>(this T value, string parameterName = null)
        {
            var context = new GuardClauseContext<T>(value, parameterName);

            return new ShouldResult<T>(context);
        }
    }
}

