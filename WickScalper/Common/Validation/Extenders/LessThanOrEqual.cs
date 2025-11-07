using System;

namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, P> LessThanOrEqual<T, P>(
            this PropertyValidator<T, P> validator, P threshold)
                where T : class
                where P : IComparable<P>
        {
            validator.AddRule(
                value => value != null && value.CompareTo(threshold) <= 0,
                $"{validator.PropertyName} must be less than or equal to {threshold}");

            return validator;
        }
    }
}

