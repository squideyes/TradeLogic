using System;

namespace WickScalper.Common
{
    /// <summary>
    /// Extension methods for custom validation predicates.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates a property using a custom predicate function.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <typeparam name="P">The type of the property being validated</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="predicate">Function that returns true if the value is valid</param>
        /// <param name="errorMessage">Optional custom error message</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> Must<T, P>(
            this PropertyValidator<T, P> validator, Func<P, bool> predicate, string errorMessage = null)
                where T : class
        {
            validator.AddRule(
                predicate,
                errorMessage ?? $"{validator.PropertyName} is invalid");

            return validator;
        }
    }
}

