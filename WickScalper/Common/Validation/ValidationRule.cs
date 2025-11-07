namespace WickScalper.Common
{
    public abstract class ValidationRule<T>
    {
        public abstract void Validate(T instance, ValidationResult result);
    }
}