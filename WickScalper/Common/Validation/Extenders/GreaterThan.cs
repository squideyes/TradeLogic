using System;

namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for GreaterThan validation rule.
    /// </summary>
    public static partial class PropertyValidatorExtenders
    {
        /// <summary>
        /// Validates that the property value is greater than a threshold.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <typeparam name="P">The property type (must implement IComparable)</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="threshold">The threshold value to compare against</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> GreaterThan<T, P>(this PropertyValidator<T, P> validator, IComparable threshold)
            where T : class
        {
            validator.AddRule(
                value =>
                {
                    if (value is IComparable comparable)
                        return comparable.CompareTo(threshold) > 0;
                    return false;
                },
                $"{validator.PropertyName} must be greater than {threshold}");
            return validator;
        }
    }
}

