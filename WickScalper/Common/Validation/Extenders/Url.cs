using System;

namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, string> Url<T>(
            this PropertyValidator<T, string> validator)
                where T : class
        {
            validator.AddRule(
                value =>
                {
                    if (string.IsNullOrEmpty(value))
                        return true;

                    return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                },
                $"{validator.PropertyName} must be a valid URL");

            return validator;
        }
    }
}

