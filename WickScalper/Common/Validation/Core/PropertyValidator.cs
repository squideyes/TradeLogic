using System;
using System.Collections.Generic;

namespace WickScalper.Common
{
    public class PropertyValidator<T, P> : ValidationRule<T>
    {
        private readonly string propertyName;
        private readonly Func<T, P> propertyGetter;

        private readonly List<Func<P, bool>> predicates = new List<Func<P, bool>>();
        private readonly List<string> errorMessages = new List<string>();

        internal PropertyValidator(string propertyName, Func<T, P> propertyGetter)
        {
            this.propertyName = propertyName;
            this.propertyGetter = propertyGetter;
        }

        /// <summary>
        /// Gets the property name for use in extension methods.
        /// </summary>
        public string PropertyName => propertyName;

        /// <summary>
        /// Adds a validation rule with a predicate and error message.
        /// Used by extension methods.
        /// </summary>
        public void AddRule(Func<P, bool> predicate, string errorMessage)
        {
            predicates.Add(predicate);
            errorMessages.Add(errorMessage);
        }

        /// <summary>
        /// Sets the error message for the most recently added rule.
        /// Used by WithMessage extension method.
        /// </summary>
        public void SetLastErrorMessage(string message)
        {
            if (errorMessages.Count > 0)
                errorMessages[errorMessages.Count - 1] = message;
        }



        public override void Validate(T instance, ValidationResult result)
        {
            var propertyValue = propertyGetter(instance);

            for (int i = 0; i < predicates.Count; i++)
            {
                if (!predicates[i](propertyValue))
                    result.AddError(propertyName, errorMessages[i]);
            }
        }
    }
}