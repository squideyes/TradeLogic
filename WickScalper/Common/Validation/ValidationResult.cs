using System.Collections.Generic;
using System.Linq;

namespace WickScalper.Common
{
    public class ValidationResult
    {
        private readonly List<ValidationError> errors;

        public ValidationResult()
        {
            errors = new List<ValidationError>();
        }

        public bool IsValid => !errors.Any();

        public IReadOnlyList<ValidationError> Errors => errors.AsReadOnly();

        internal void AddError(string propertyName, string errorMessage) =>
            errors.Add(new ValidationError(propertyName, errorMessage));

        public string GetErrorMessages() =>
            string.Join("; ", errors.Select(e => e.ErrorMessage));
    }
}