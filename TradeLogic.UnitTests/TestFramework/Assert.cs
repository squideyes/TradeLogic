using System;

namespace TradeLogic.UnitTests.TestFramework
{
    public static class Assert
    {
        public static void True(bool condition, string message = "")
        {
            if (!condition)
                throw new AssertionException($"Expected true but was false. {message}");
        }

        public static void False(bool condition, string message = "")
        {
            if (condition)
                throw new AssertionException($"Expected false but was true. {message}");
        }

        public static void Equal<T>(T expected, T actual, string message = "")
        {
            if (!Equals(expected, actual))
                throw new AssertionException($"Expected {expected} but was {actual}. {message}");
        }

        public static void NotEqual<T>(T expected, T actual, string message = "")
        {
            if (Equals(expected, actual))
                throw new AssertionException($"Expected not equal to {expected}. {message}");
        }

        public static void Null(object obj, string message = "")
        {
            if (obj != null)
                throw new AssertionException($"Expected null but was {obj}. {message}");
        }

        public static void NotNull(object obj, string message = "")
        {
            if (obj == null)
                throw new AssertionException($"Expected not null. {message}");
        }

        public static void Throws<TException>(Action action, string message = "") where TException : Exception
        {
            try
            {
                action();
                throw new AssertionException($"Expected {typeof(TException).Name} but no exception was thrown. {message}");
            }
            catch (TException)
            {
                // Expected
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Expected {typeof(TException).Name} but got {ex.GetType().Name}. {message}");
            }
        }

        public static void DoesNotThrow(Action action, string message = "")
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new AssertionException($"Expected no exception but got {ex.GetType().Name}: {ex.Message}. {message}");
            }
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}

