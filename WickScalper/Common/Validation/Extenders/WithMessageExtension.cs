namespace WickScalper.Common
{
    /// <summary>
    /// Extension method for WithMessage validation rule.
    /// </summary>
    public static class WithMessageExtension
    {
        /// <summary>
        /// Sets a custom error message for the most recently added validation rule.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <typeparam name="P">The property type</typeparam>
        /// <param name="validator">The property validator</param>
        /// <param name="message">The custom error message</param>
        /// <returns>The property validator for method chaining</returns>
        public static PropertyValidator<T, P> WithMessage<T, P>(this PropertyValidator<T, P> validator, string message)
            where T : class
        {
            validator.SetLastErrorMessage(message);
            return validator;
        }
    }
}

