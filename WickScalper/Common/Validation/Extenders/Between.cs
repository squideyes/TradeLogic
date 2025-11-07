using System;

namespace WickScalper.Common
{
    /// <summary>
    /// Extension methods for range validation.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates that a property value is between two thresholds (inclusive).
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <typeparam name="P">The type of the property being validated (must implement IComparable&lt;P&gt;)</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="min">The minimum value (inclusive)</param>
        /// <param name="max">The maximum value (inclusive)</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> Between<T, P>(
            this PropertyValidator<T, P> validator, P min, P max)
            where T : class
            where P : IComparable<P>
        {
            validator.AddRule(
                value => value != null && value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
                $"{validator.PropertyName} must be between {min} and {max}");

            return validator;
        }
    }
}

