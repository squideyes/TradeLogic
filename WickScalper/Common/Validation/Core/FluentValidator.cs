using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WickScalper.Common
{
    public abstract class FluentValidator<T> 
        where T : class
    {
        private readonly List<ValidationRule<T>> rules;

        protected FluentValidator()
        {
            rules = new List<ValidationRule<T>>();
        }

        protected PropertyValidator<T, P> RuleFor<P>(Expression<Func<T, P>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);

            var getter = propertyExpression.Compile();

            var validator = new PropertyValidator<T, P>(propertyName, getter);

            rules.Add(validator);

            return validator;
        }

        public ValidationResult Validate(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var result = new ValidationResult();

            foreach (var rule in rules)
                rule.Validate(instance, result);

            return result;
        }

        private string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Member.Name;

            throw new ArgumentException("Expression must be a member expression");
        }
    }
}