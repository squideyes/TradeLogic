using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WickScalper.Common
{
    public class MayNotResult<T> : GuardClauseResult<T>
    {
        public MayNotResult(GuardClauseContext<T> context) : base(context) { }

        public MayNotResult<T> BeNull(string message = null)
        {
            if (Context.Value == null)
                Throw(message ?? GetMessage("may not be null"));

            return this;
        }

        public MayNotResult<T> BeNegative(string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(default(T)) < 0)
                Throw(message ?? GetMessage("may not be negative"));

            return this;
        }

        public MayNotResult<T> BeZero(string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(default(T)) == 0)
                Throw(message ?? GetMessage("may not be zero"));

            return this;
        }

        public MayNotResult<T> BePositive(string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(default(T)) > 0)
                Throw(message ?? GetMessage("may not be positive"));

            return this;
        }

        public MayNotResult<T> Match(string pattern, string message = null)
        {
            if (Context.Value is string str && Regex.IsMatch(str, pattern))
                Throw(message ?? GetMessage($"may not match pattern '{pattern}'"));

            return this;
        }

        public MayNotResult<T> BeEmpty(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrWhiteSpace(str))
                Throw(message ?? GetMessage("may not be empty"));

            if (Context.Value is System.Collections.IEnumerable enumerable && !enumerable.Cast<object>().Any())
                Throw(message ?? GetMessage("may not be empty"));

            return this;
        }

        public MayNotResult<T> BeNullOrEmpty(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrEmpty(str))
                Throw(message ?? GetMessage("may not be null or empty"));

            return this;
        }

        public MayNotResult<T> BeNullOrWhiteSpace(string message = null)
        {
            if (Context.Value is string str && string.IsNullOrWhiteSpace(str))

                Throw(message ?? GetMessage("may not be null or whitespace"));

            return this;
        }

        public MayNotResult<T> BeValidEmail(string message = null)
        {
            if (Context.Value is string str)
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                if (Regex.IsMatch(str, emailPattern))
                
                    Throw(message ?? GetMessage("may not be a valid email"));
            }

            return this;
        }

        public MayNotResult<T> BeValidUrl(string message = null)
        {
            if (Context.Value is string str)
            {
                if (Uri.TryCreate(str, UriKind.Absolute, out _))
                    Throw(message ?? GetMessage("may not be a valid URL"));
            }

            return this;
        }

        public MayNotResult<T> HaveCount(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() == count)
                Throw(message ?? GetMessage($"may not have count of {count}"));

            return this;
        }

        public MayNotResult<T> HaveCountGreaterThan(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() > count)
                Throw(message ?? GetMessage($"may not have count greater than {count}"));

            return this;
        }

        public MayNotResult<T> HaveCountLessThan(int count, string message = null)
        {
            if (Context.Value is System.Collections.IEnumerable enumerable && enumerable.Cast<object>().Count() < count)
                Throw(message ?? GetMessage($"may not have count less than {count}"));

            return this;
        }

        public MayNotResult<T> BeEqualTo(T other, string message = null)
        {
            if (Context.Value is IComparable comparable && comparable.CompareTo(other) == 0)
                Throw(message ?? GetMessage($"may not be equal to {other}"));

            return this;
        }

        public MayNotResult<T> HaveLength(int length, string message = null)
        {
            if (Context.Value is string str && str.Length == length)
                Throw(message ?? GetMessage($"may not have length of {length}"));

            return this;
        }

        public MayNotResult<T> HaveLengthGreaterThan(int length, string message = null)
        {
            if (Context.Value is string str && str.Length > length)
                Throw(message ?? GetMessage($"may not have length greater than {length}"));

            return this;
        }

        public MayNotResult<T> HaveLengthLessThan(int length, string message = null)
        {
            if (Context.Value is string str && str.Length < length)
                Throw(message ?? GetMessage($"may not have length less than {length}"));

            return this;
        }

        public MayNotResult<T> HaveLengthBetween(int minLength, int maxLength, string message = null)
        {
            if (Context.Value is string str && str.Length >= minLength && str.Length <= maxLength)
                Throw(message ?? GetMessage($"may not have length between {minLength} and {maxLength}"));
            
            return this;
        }

        public MayNotResult<T> Contain(string substring, string message = null)
        {
            if (Context.Value is string str && str.Contains(substring))
                Throw(message ?? GetMessage($"may not contain '{substring}'"));

            return this;
        }

        public MayNotResult<T> StartWith(string prefix, string message = null)
        {
            if (Context.Value is string str && str.StartsWith(prefix))
                Throw(message ?? GetMessage($"may not start with '{prefix}'"));

            return this;
        }

        public MayNotResult<T> EndWith(string suffix, string message = null)
        {
            if (Context.Value is string str && str.EndsWith(suffix))
                Throw(message ?? GetMessage($"may not end with '{suffix}'"));

            return this;
        }

        public MayNotResult<T> BeDefined(string message = null)
        {
            if (Context.Value is Enum enumValue && Enum.IsDefined(enumValue.GetType(), enumValue))
                Throw(message ?? GetMessage("may not be a defined enum value"));

            return this;
        }

        public static implicit operator T(MayNotResult<T> result)
        {
            return result.Context.Value;
        }
    }
}

