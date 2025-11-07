using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WickScalper.Common
{
    public class ShouldResult<T> : GuardClauseResult<T>
    {
        public ShouldResult(GuardClauseContext<T> context) : base(context) { }

        public ShouldResult<T> BeNotNull(string message = null)
        {
            if (Context.Value == null)
                Throw(message ?? GetMessage("should not be null"));

            return this;
        }

        public ShouldResult<T> BeGreaterThan(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) <= 0)
                Throw(message ?? GetMessage($"should be greater than {other}"));

            return this;
        }

        public ShouldResult<T> BeGreaterThanOrEqual(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) < 0)
                Throw(message ?? GetMessage($"should be greater than or equal to {other}"));

            return this;
        }

        public ShouldResult<T> BeLessThan(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) >= 0)
                Throw(message ?? GetMessage($"should be less than {other}"));

            return this;
        }

        public ShouldResult<T> BeLessThanOrEqual(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) > 0)
                Throw(message ?? GetMessage($"should be less than or equal to {other}"));

            return this;
        }

        public ShouldResult<T> BeBetween(T min, T max, string message = null)
        {
            if (Context.Value is IComparable comparable && (comparable.CompareTo(min) < 0 || comparable.CompareTo(max) > 0))
                Throw(message ?? GetMessage($"should be between {min} and {max}"));

            return this;
        }

        public ShouldResult<T> BeNotEmpty(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrWhiteSpace(str))
                Throw(message ?? GetMessage("should not be empty"));

            if (Context.Value is System.Collections.IEnumerable enumerable && !enumerable.Cast<object>().Any())
                Throw(message ?? GetMessage("should not be empty"));

            return this;
        }

        public ShouldResult<T> BeNotNullOrEmpty(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrEmpty(str))
                Throw(message ?? GetMessage("should not be null or empty"));

            return this;
        }

        public ShouldResult<T> BeNotNullOrWhiteSpace(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrWhiteSpace(str))
                Throw(message ?? GetMessage("should not be null or whitespace"));

            return this;
        }

        public ShouldResult<T> Match(string pattern, string message = null)
        {
            if (Context.Value is string str && !Regex.IsMatch(str, pattern))
                Throw(message ?? GetMessage($"should match pattern '{pattern}'"));

            return this;
        }

        public ShouldResult<T> BeValidEmail(string message = null)
        {
            if (Context.Value is string str)
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                if (!Regex.IsMatch(str, emailPattern))
                    Throw(message ?? GetMessage("should be a valid email"));
            }
            return this;
        }

        public ShouldResult<T> BeValidUrl(string message = null)
        {
            if (Context.Value is string str)
            {
                if (!Uri.TryCreate(str, UriKind.Absolute, out _))
                    Throw(message ?? GetMessage("should be a valid URL"));
            }

            return this;
        }

        public ShouldResult<T> HaveCount(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() != count)
                Throw(message ?? GetMessage($"should have count of {count}"));

            return this;
        }

        public ShouldResult<T> HaveCountGreaterThan(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() <= count)
                Throw(message ?? GetMessage($"should have count greater than {count}"));

            return this;
        }

        public ShouldResult<T> HaveCountLessThan(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() < count)
                Throw(message ?? GetMessage($"should have count less than {count}"));

            return this;
        }

        public ShouldResult<T> Satisfy(Func<T, bool> predicate, string message = null)
        {
            if (!predicate(Context.Value))
                Throw(message ?? GetMessage("should satisfy the condition"));

            return this;
        }

        public ShouldResult<T> BeEqualTo(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) != 0)
                Throw(message ?? GetMessage($"should be equal to {other}"));
            return this;
        }

        public ShouldResult<T> HaveLength(int length, string message = null)
        {
            if (Context.Value is string str && str.Length != length)
                Throw(message ?? GetMessage($"should have length of {length}"));

            return this;
        }

        public ShouldResult<T> HaveLengthGreaterThan(int length, string message = null)
        {
            if (Context.Value is string str && str.Length <= length)
                Throw(message ?? GetMessage($"should have length greater than {length}"));

            return this;
        }

        public ShouldResult<T> HaveLengthLessThan(int length, string message = null)
        {
            if (Context.Value is string str && str.Length >= length)
                Throw(message ?? GetMessage($"should have length less than {length}"));

            return this;
        }

        public ShouldResult<T> HaveLengthBetween(int minLength, int maxLength, string message = null)
        {
            if (Context.Value is string str && (str.Length < minLength || str.Length > maxLength))
                Throw(message ?? GetMessage($"should have length between {minLength} and {maxLength}"));

            return this;
        }

        public ShouldResult<T> Contain(string substring, string message = null)
        {
            if (Context.Value is string str && !str.Contains(substring))
                Throw(message ?? GetMessage($"should contain '{substring}'"));

            return this;
        }

        public ShouldResult<T> StartWith(string prefix, string message = null)
        {
            if (Context.Value is string str && !str.StartsWith(prefix))
                Throw(message ?? GetMessage($"should start with '{prefix}'"));

            return this;
        }

        public ShouldResult<T> EndWith(string suffix, string message = null)
        {
            if (Context.Value is string str && !str.EndsWith(suffix))
                Throw(message ?? GetMessage($"should end with '{suffix}'"));

            return this;
        }

        public ShouldResult<T> BeDefined(string message = null)
        {
            if (Context.Value is Enum enumValue && !Enum.IsDefined(enumValue.GetType(), enumValue))
                Throw(message ?? GetMessage("should be a defined enum value"));

            return this;
        }

        public static implicit operator T(ShouldResult<T> result)
        {
            return result.Context.Value;
        }
    }
}

