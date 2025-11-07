namespace WickScalper.Common
{
    public static partial class PropertyValidatorExtenders
    {
        public static PropertyValidator<T, P> WithMessage<T, P>(this PropertyValidator<T, P> validator, string message)
            where T : class
        {
            validator.SetLastErrorMessage(message);

            return validator;
        }
    }
}

