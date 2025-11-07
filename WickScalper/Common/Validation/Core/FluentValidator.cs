using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WickScalper.Common
{
    /// <summary>
    /// Base class for fluent-style validators with support for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of object being validated</typeparam>
    public abstract class FluentValidator<T>
        where T : class
    {
        private readonly List<ValidationRule<T>> rules;
        private static readonly Dictionary<string, Delegate> ExpressionCache = new Dictionary<string, Delegate>();

        /// <summary>
        /// Initializes a new instance of the FluentValidator class.
        /// </summary>
        protected FluentValidator()
        {
            rules = new List<ValidationRule<T>>();
        }

        /// <summary>
        /// Defines a validation rule for a specific property.
        /// </summary>
        /// <typeparam name="P">The type of the property</typeparam>
        /// <param name="propertyExpression">Expression selecting the property to validate</param>
        /// <returns>A PropertyValidator for fluent rule configuration</returns>
        protected PropertyValidator<T, P> RuleFor<P>(Expression<Func<T, P>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            var cacheKey = $"{typeof(T).FullName}.{propertyName}";

            Func<T, P> getter;
            lock (ExpressionCache)
            {
                if (!ExpressionCache.TryGetValue(cacheKey, out var cachedGetter))
                {
                    cachedGetter = propertyExpression.Compile();
                    ExpressionCache[cacheKey] = cachedGetter;
                }
                getter = (Func<T, P>)cachedGetter;
            }

            var validator = new PropertyValidator<T, P>(propertyName, getter);
            rules.Add(validator);

            return validator;
        }

        /// <summary>
        /// Validates the specified instance.
        /// </summary>
        /// <param name="instance">The instance to validate</param>
        /// <returns>A ValidationResult containing any validation errors</returns>
        /// <exception cref="ArgumentNullException">Thrown when instance is null</exception>
        public ValidationResult Validate(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var result = new ValidationResult();

            foreach (var rule in rules)
                rule.Validate(instance, result);

            return result;
        }

        /// <summary>
        /// Extracts the property name from an expression, supporting nested properties.
        /// </summary>
        /// <typeparam name="TProperty">The property type</typeparam>
        /// <param name="expression">The property expression</param>
        /// <returns>The property name or dot-separated path for nested properties</returns>
        /// <exception cref="ArgumentException">Thrown when expression is not a member expression</exception>
        private string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var path = new List<string>();
            var expr = expression.Body;

            while (expr is MemberExpression memberExpr)
            {
                path.Add(memberExpr.Member.Name);
                expr = memberExpr.Expression;
            }

            if (path.Count == 0)
                throw new ArgumentException("Expression must be a member expression");

            path.Reverse();
            return string.Join(".", path);
        }
    }
}