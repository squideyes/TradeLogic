namespace WickScalper.Common
{
    /// <summary>
    /// Extension methods for string property validation.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates that a string property is not null, empty, or whitespace.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <param name="validator">The property validator</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, string> NotEmpty<T>(this PropertyValidator<T, string> validator)
            where T : class
        {
            validator.AddRule(value => !string.IsNullOrWhiteSpace(value),
                $"{validator.PropertyName} must not be empty");

            return validator;
        }
    }
}

