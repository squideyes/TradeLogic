namespace WickScalper.Common
{
    public class ValidationError
    {
        internal ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        public string PropertyName { get; }
        public string ErrorMessage { get; }

        public override string ToString() => $"{PropertyName}: {ErrorMessage}";
    }
}
