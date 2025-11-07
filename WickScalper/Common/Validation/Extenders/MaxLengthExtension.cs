namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for MaxLength validation rule.
    /// </summary>
    public static class MaxLengthExtension
    {
        /// <summary>
        /// Validates that the string property does not exceed a maximum length.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="maxLength">The maximum allowed length</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, string> MaxLength<T>(this PropertyValidator<T, string> validator, int maxLength)
            where T : class
        {
            validator.AddRule(
                value => value == null || value.Length <= maxLength,
                $"{validator.PropertyName} must not exceed {maxLength} characters");
            return validator;
        }
    }
}

