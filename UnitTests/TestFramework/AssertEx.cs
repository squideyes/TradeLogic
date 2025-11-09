using NUnit.Framework;

namespace TradeLogic.UnitTests.TestFramework
{
    // Minimal classic-style assertions shim for NUnit 4
    public static class AssertEx
    {
        public static void AreEqual<T>(T expected, T actual)
            => NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));

        public static void IsTrue(bool condition)
            => NUnit.Framework.Assert.That(condition, Is.True);

        public static void IsFalse(bool condition)
            => NUnit.Framework.Assert.That(condition, Is.False);

        public static void IsNotNull(object obj)
            => NUnit.Framework.Assert.That(obj, Is.Not.Null);
    }
}

