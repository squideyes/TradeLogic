namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, string> MinLength<T>(
            this PropertyValidator<T, string> validator, int minLength)
                where T : class
        {
            validator.AddRule(
                value => value != null && value.Length >= minLength,
                $"{validator.PropertyName} must be at least {minLength} characters long");

            return validator;
        }
    }
}

