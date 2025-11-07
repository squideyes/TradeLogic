using System;

namespace WickScalper.Common
{
    /// <summary>
    /// Extension methods for greater-than-or-equal comparison validation.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates that a property value is greater than or equal to a threshold.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated</typeparam>
        /// <typeparam name="P">The type of the property being validated (must implement IComparable&lt;P&gt;)</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="threshold">The threshold value</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> GreaterThanOrEqual<T, P>(
            this PropertyValidator<T, P> validator, P threshold)
            where T : class
            where P : IComparable<P>
        {
            validator.AddRule(
                value => value != null && value.CompareTo(threshold) >= 0,
                $"{validator.PropertyName} must be greater than or equal to {threshold}");

            return validator;
        }
    }
}

