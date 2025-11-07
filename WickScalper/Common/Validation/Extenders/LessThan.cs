using System;

namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, P> LessThan<T, P>(
            this PropertyValidator<T, P> validator, IComparable threshold)
                where T : class
        {
            validator.AddRule(
                value =>
                {
                    if (value is IComparable comparable)
                        return comparable.CompareTo(threshold) < 0;
                    return false;
                },
                $"{validator.PropertyName} must be less than {threshold}");

            return validator;
        }
    }
}

