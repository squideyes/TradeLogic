using System.Text.RegularExpressions;

namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, string> Matches<T>(
            this PropertyValidator<T, string> validator, string pattern)
                where T : class
        {
            var regex = new Regex(pattern);
    
            validator.AddRule(
                value => string.IsNullOrEmpty(value) || regex.IsMatch(value),
                $"{validator.PropertyName} must match the pattern '{pattern}'");

            return validator;
        }

        public static PropertyValidator<T, string> Matches<T>(
            this PropertyValidator<T, string> validator, Regex regex)
                where T : class
        {
            validator.AddRule(
                value => string.IsNullOrEmpty(value) || regex.IsMatch(value),
                $"{validator.PropertyName} must match the pattern '{regex.ToString()}'");

            return validator;
        }
    }
}

