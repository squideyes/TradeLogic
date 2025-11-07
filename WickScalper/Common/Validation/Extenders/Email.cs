using System.Text.RegularExpressions;

namespace WickScalper.Common
{
    /// <summary>
    /// Extension methods for email validation.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        // Simple email regex pattern - matches most common email formats
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Validates that a string property is a valid email address.
        /// Uses a simple pattern that matches most common email formats.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <param name="validator">The property validator</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, string> Email<T>(
            this PropertyValidator<T, string> validator)
            where T : class
        {
            validator.AddRule(
                value => string.IsNullOrEmpty(value) || EmailRegex.IsMatch(value),
                $"{validator.PropertyName} must be a valid email address");

            return validator;
        }
    }
}

