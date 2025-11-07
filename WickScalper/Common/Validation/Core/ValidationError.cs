namespace WickScalper.Common
{
    /// <summary>
    /// Represents a single validation error for a property.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the ValidationError class.
        /// </summary>
        /// <param name="propertyName">The name of the property with the error</param>
        /// <param name="errorMessage">The error message</param>
        internal ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets the name of the property that failed validation.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the error message describing the validation failure.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Returns a string representation of the validation error.
        /// </summary>
        /// <returns>A string in the format "PropertyName: ErrorMessage"</returns>
        public override string ToString() => $"{PropertyName}: {ErrorMessage}";
    }
}
