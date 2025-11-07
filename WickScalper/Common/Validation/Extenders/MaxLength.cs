namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, string> MaxLength<T>(
            this PropertyValidator<T, string> validator, int maxLength)
                where T : class
        {
            validator.AddRule(
                value => string.IsNullOrEmpty(value) || value.Length <= maxLength,
                $"{validator.PropertyName} must not exceed {maxLength} characters");

            return validator;
        }
    }
}

