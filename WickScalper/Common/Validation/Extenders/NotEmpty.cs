namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, string> NotEmpty<T>(this PropertyValidator<T, string> validator)
            where T : class
        {
            validator.AddRule(value => !string.IsNullOrWhiteSpace(value), 
                $"{validator.PropertyName} must not be empty");
            
            return validator;
        }
    }
}

