using System;

namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for Must validation rule (custom predicate).
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates that the property value satisfies a custom predicate.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <typeparam name="P">The property type</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="predicate">The custom validation predicate</param>
        /// <param name="errorMessage">Optional custom error message</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> Must<T, P>(this PropertyValidator<T, P> validator, Func<P, bool> predicate, string errorMessage = null)
            where T : class
        {
            validator.AddRule(
                predicate,
                errorMessage ?? $"{validator.PropertyName} is invalid");
            return validator;
        }
    }
}

