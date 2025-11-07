using System;

namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
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

