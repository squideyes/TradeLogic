using System;
using System.Collections.Generic;
using System.Linq;

namespace WickScalper.Common
{
    /// <summary>
    /// Contains the results of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        private readonly List<ValidationError> errors;

        /// <summary>
        /// Initializes a new instance of the ValidationResult class.
        /// </summary>
        public ValidationResult()
        {
            errors = new List<ValidationError>();
        }

        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid => !errors.Any();

        /// <summary>
        /// Gets the list of validation errors.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => errors.AsReadOnly();

        /// <summary>
        /// Gets the total number of validation errors.
        /// </summary>
        public int ErrorCount => errors.Count;

        /// <summary>
        /// Adds an error to the validation result.
        /// </summary>
        /// <param name="propertyName">The name of the property with the error</param>
        /// <param name="errorMessage">The error message</param>
        internal void AddError(string propertyName, string errorMessage) =>
            errors.Add(new ValidationError(propertyName, errorMessage));

        /// <summary>
        /// Gets all error messages concatenated with semicolons.
        /// </summary>
        /// <returns>A string containing all error messages</returns>
        public string GetErrorMessages() =>
            string.Join("; ", errors.Select(e => e.ErrorMessage));

        /// <summary>
        /// Gets all errors for a specific property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>A read-only list of errors for the property</returns>
        public IReadOnlyList<ValidationError> GetErrorsForProperty(string propertyName) =>
            errors.Where(e => e.PropertyName == propertyName).ToList().AsReadOnly();

        /// <summary>
        /// Throws a ValidationException if the validation failed.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when validation failed</exception>
        public void ThrowIfInvalid()
        {
            if (!IsValid)
                throw new ValidationException(GetErrorMessages());
        }
    }
}