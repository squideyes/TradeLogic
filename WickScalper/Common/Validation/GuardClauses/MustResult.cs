using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WickScalper.Common
{
    public class MustResult<T> : GuardClauseResult<T>
    {
        public MustResult(GuardClauseContext<T> context) : base(context) { }

        public MustResult<T> BeNotNull(string message = null)
        {
            if (Context.Value == null)
                Throw(message ?? GetMessage("must not be null"));

            return this;
        }

        public MustResult<T> BeGreaterThan(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) <= 0)
                Throw(message ?? GetMessage($"must be greater than {other}"));

            return this;
        }

        public MustResult<T> BeGreaterThanOrEqual(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) < 0)
                Throw(message ?? GetMessage($"must be greater than or equal to {other}"));

            return this;
        }

        public MustResult<T> BeLessThan(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) >= 0)
                Throw(message ?? GetMessage($"must be less than {other}"));

            return this;
        }

        public MustResult<T> BeLessThanOrEqual(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) > 0)
                Throw(message ?? GetMessage($"must be less than or equal to {other}"));

            return this;
        }

        public MustResult<T> BeBetween(T min, T max, string message = null)
        {
            if (Context.Value is IComparable comparable && (comparable.CompareTo(min) < 0 || comparable.CompareTo(max) > 0))
                Throw(message ?? GetMessage($"must be between {min} and {max}"));

            return this;
        }

        public MustResult<T> BeNotEmpty(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrWhiteSpace(str))
                Throw(message ?? GetMessage("must not be empty"));

            if (Context.Value is System.Collections.IEnumerable enumerable && !enumerable.Cast<object>().Any())
                Throw(message ?? GetMessage("must not be empty"));

            return this;
        }

        public MustResult<T> BeNotNullOrEmpty(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrEmpty(str))
                Throw(message ?? GetMessage("must not be null or empty"));

            return this;
        }

        public MustResult<T> BeNotNullOrWhiteSpace(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrWhiteSpace(str))
                Throw(message ?? GetMessage("must not be null or whitespace"));

            return this;
        }

        public MustResult<T> Match(string pattern, string message = null)
        {
            if (Context.Value is string str && !Regex.IsMatch(str, pattern))
                Throw(message ?? GetMessage($"must match pattern '{pattern}'"));

            return this;
        }

        public MustResult<T> BeValidEmail(string message = null)
        {
            if (Context.Value is string str)
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                if (!Regex.IsMatch(str, emailPattern))
                    Throw(message ?? GetMessage("must be a valid email"));
            }
            return this;
        }

        public MustResult<T> BeValidUrl(string message = null)
        {
            if (Context.Value is string str)
            {
                if (!Uri.TryCreate(str, UriKind.Absolute, out _))
                    Throw(message ?? GetMessage("must be a valid URL"));
            }

            return this;
        }

        public MustResult<T> HaveCount(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() != count)
                Throw(message ?? GetMessage($"must have count of {count}"));

            return this;
        }

        public MustResult<T> HaveCountGreaterThan(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() <= count)
                Throw(message ?? GetMessage($"must have count greater than {count}"));

            return this;
        }

        public MustResult<T> HaveCountLessThan(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() < count)
                Throw(message ?? GetMessage($"must have count less than {count}"));

            return this;
        }

        public MustResult<T> Satisfy(Func<T, bool> predicate, string message = null)
        {
            if (!predicate(Context.Value))
                Throw(message ?? GetMessage("must satisfy the condition"));

            return this;
        }

        public MustResult<T> BeEqualTo(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) != 0)
                Throw(message ?? GetMessage($"must be equal to {other}"));
            return this;
        }

        public MustResult<T> HaveLength(int length, string message = null)
        {
            if (Context.Value is string str && str.Length != length)
                Throw(message ?? GetMessage($"must have length of {length}"));

            return this;
        }

        public MustResult<T> HaveLengthGreaterThan(int length, string message = null)
        {
            if (Context.Value is string str && str.Length <= length)
                Throw(message ?? GetMessage($"must have length greater than {length}"));

            return this;
        }

        public MustResult<T> HaveLengthLessThan(int length, string message = null)
        {
            if (Context.Value is string str && str.Length >= length)
                Throw(message ?? GetMessage($"must have length less than {length}"));

            return this;
        }

        public MustResult<T> HaveLengthBetween(int minLength, int maxLength, string message = null)
        {
            if (Context.Value is string str && (str.Length < minLength || str.Length > maxLength))
                Throw(message ?? GetMessage($"must have length between {minLength} and {maxLength}"));
            
            return this;
        }

        public MustResult<T> Contain(string substring, string message = null)
        {
            if (Context.Value is string str && !str.Contains(substring))
                Throw(message ?? GetMessage($"must contain '{substring}'"));

            return this;
        }

        public MustResult<T> StartWith(string prefix, string message = null)
        {
            if (Context.Value is string str && !str.StartsWith(prefix))
                Throw(message ?? GetMessage($"must start with '{prefix}'"));

            return this;
        }

        public MustResult<T> EndWith(string suffix, string message = null)
        {
            if (Context.Value is string str && !str.EndsWith(suffix))
                Throw(message ?? GetMessage($"must end with '{suffix}'"));

            return this;
        }
    }
}

