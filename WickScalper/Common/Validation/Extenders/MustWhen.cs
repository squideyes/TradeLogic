using System;

namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, P> MustWhen<T, P>(
            this PropertyValidator<T, P> validator,
            Func<T, bool> condition,
            Func<P, bool> predicate,
            string errorMessage = null)
                where T : class
        {
            validator.AddConditionalRule(
                condition,
                predicate,
                errorMessage ?? $"{validator.PropertyName} is invalid");

            return validator;
        }
    }
}

