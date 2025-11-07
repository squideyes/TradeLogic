namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for NotEmpty validation rule.
    /// </summary>
    public static class NotEmptyExtension
    {
        /// <summary>
        /// Validates that the string property is not empty or whitespace.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The property validator</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, string> NotEmpty<T>(this PropertyValidator<T, string> validator)
            where T : class
        {
            validator.AddRule(value => !string.IsNullOrWhiteSpace(value), $"{validator.PropertyName} must not be empty");
            return validator;
        }
    }
}

