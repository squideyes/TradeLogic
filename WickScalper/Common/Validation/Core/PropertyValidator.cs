using System;
using System.Collections.Generic;

namespace WickScalper.Common
{
    /// <summary>
    /// Validator for a specific property with support for multiple validation rules.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated</typeparam>
    /// <typeparam name="P">The type of the property being validated</typeparam>
    public class PropertyValidator<T, P> : ValidationRule<T>
    {
        private readonly string propertyName;
        private readonly Func<T, P> propertyGetter;

        private readonly List<Func<P, bool>> predicates = new List<Func<P, bool>>();
        private readonly List<string> errorMessages = new List<string>();
        private readonly List<Func<T, bool>> conditions = new List<Func<T, bool>>();

        /// <summary>
        /// Initializes a new instance of the PropertyValidator class.
        /// </summary>
        /// <param name="propertyName">The name of the property being validated</param>
        /// <param name="propertyGetter">Function to extract the property value from an instance</param>
        internal PropertyValidator(string propertyName, Func<T, P> propertyGetter)
        {
            this.propertyName = propertyName;
            this.propertyGetter = propertyGetter;
        }

        /// <summary>
        /// Gets the name of the property being validated.
        /// </summary>
        public string PropertyName => propertyName;

        /// <summary>
        /// Gets the number of rules defined for this property.
        /// </summary>
        public int RuleCount => predicates.Count;

        /// <summary>
        /// Adds a validation rule with a predicate and error message.
        /// </summary>
        /// <param name="predicate">Function that returns true if the value is valid</param>
        /// <param name="errorMessage">Error message to display if validation fails</param>
        public void AddRule(Func<P, bool> predicate, string errorMessage)
        {
            predicates.Add(predicate);
            errorMessages.Add(errorMessage);
            conditions.Add(null);
        }

        /// <summary>
        /// Adds a conditional validation rule that only applies when a condition is met.
        /// </summary>
        /// <param name="condition">Function that returns true if the rule should be applied</param>
        /// <param name="predicate">Function that returns true if the value is valid</param>
        /// <param name="errorMessage">Error message to display if validation fails</param>
        public void AddConditionalRule(Func<T, bool> condition, Func<P, bool> predicate, string errorMessage)
        {
            predicates.Add(predicate);
            errorMessages.Add(errorMessage);
            conditions.Add(condition);
        }

        /// <summary>
        /// Sets the error message for the last added rule.
        /// </summary>
        /// <param name="message">The new error message</param>
        /// <exception cref="InvalidOperationException">Thrown when no rules have been added</exception>
        public void SetLastErrorMessage(string message)
        {
            if (errorMessages.Count == 0)
                throw new InvalidOperationException("No rules to customize message for");

            errorMessages[errorMessages.Count - 1] = message;
        }

        /// <summary>
        /// Validates the property value against all defined rules.
        /// </summary>
        /// <param name="instance">The instance being validated</param>
        /// <param name="result">The validation result to add errors to</param>
        public override void Validate(T instance, ValidationResult result)
        {
            var propertyValue = propertyGetter(instance);

            for (int i = 0; i < predicates.Count; i++)
            {
                // Skip rule if a condition is defined and it returns false
                if (conditions[i] != null && !conditions[i](instance))
                    continue;

                if (!predicates[i](propertyValue))
                    result.AddError(propertyName, errorMessages[i]);
            }
        }
    }
}