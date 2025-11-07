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

        public string PropertyName => propertyName;

        public void AddRule(Func<P, bool> predicate, string errorMessage)
        {
            predicates.Add(predicate);
            errorMessages.Add(errorMessage);
        }

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