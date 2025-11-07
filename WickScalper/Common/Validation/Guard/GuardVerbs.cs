namespace WickScalper.Common
{
    public static class GuardVerbs
    {
        public static MayNotValidators<T> MayNot<T>(
            this T value, string parameterName = null)
        {
            return new MayNotValidators<T>(
                new GuardContext<T>(value, parameterName));
        }

        public static ShouldValidators<T> Should<T>(
            this T value, string parameterName = null)
        {
            return new ShouldValidators<T>(
                new GuardContext<T>(value, parameterName));
        }
    }
}

