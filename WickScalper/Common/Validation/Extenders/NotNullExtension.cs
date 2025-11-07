namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for NotNull validation rule.
    /// </summary>
    public static class NotNullExtension
    {
        /// <summary>
        /// Validates that the property value is not null.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <typeparam name="P">The property type</typeparam>
        /// <param name="validator">The property validator</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> NotNull<T, P>(this PropertyValidator<T, P> validator)
            where T : class
        {
            validator.AddRule(value => value != null, $"{validator.PropertyName} must not be null");
            return validator;
        }
    }
}

