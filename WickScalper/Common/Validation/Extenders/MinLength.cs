namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for MinLength validation rule.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates that the string property has a minimum length.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="minLength">The minimum required length</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, string> MinLength<T>(this PropertyValidator<T, string> validator, int minLength)
            where T : class
        {
            validator.AddRule(
                value => value != null && value.Length >= minLength,
                $"{validator.PropertyName} must be at least {minLength} characters long");
            return validator;
        }
    }
}

