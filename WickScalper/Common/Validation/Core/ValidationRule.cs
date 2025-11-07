namespace WickScalper.Common
{
    /// <summary>
    /// Base class for validation rules that can be applied to objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    public abstract class ValidationRule<T>
    {
        /// <summary>
        /// Validates the specified instance and adds any errors to the result.
        /// </summary>
        /// <param name="instance">The instance to validate</param>
        /// <param name="result">The validation result to add errors to</param>
        public abstract void Validate(T instance, ValidationResult result);
    }
}