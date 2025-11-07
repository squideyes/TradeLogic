namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, P> NotNull<T, P>(this PropertyValidator<T, P> validator)
            where T : class
        {
            validator.AddRule(value => value != null, $"{validator.PropertyName} must not be null");

            return validator;
        }
    }
}

