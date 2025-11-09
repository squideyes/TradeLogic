using NUnit.Framework;
using WickScalper.Common;
using Assert = TradeLogic.UnitTests.TestFramework.AssertEx;
using Bar = WickScalper.Common.Bar;
using Tick = WickScalper.Common.Tick;
using Future = WickScalper.Common.Future;
using Symbol = WickScalper.Common.Symbol;
using TickSet = WickScalper.Common.TickSet;
using TickSetParser = WickScalper.Common.TickSetParser;
using GuardException = WickScalper.Common.GuardException;

namespace TradeLogic.UnitTests.Common.PricingTests
{
    [TestFixture]
    public class FutureTests
    {
        [Test]
        public void IsPrice_WithValidPrice_ReturnsTrue()
        {
            var future = new Future
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                DecimalPlaces = 2
            };

            Assert.IsTrue(future.IsPrice(4500.00m));
            Assert.IsTrue(future.IsPrice(4500.25m));
            Assert.IsTrue(future.IsPrice(4500.50m));
            Assert.IsTrue(future.IsPrice(4500.75m));
        }

        [Test]
        public void IsPrice_WithInvalidPrice_ReturnsFalse()
        {
            var future = new Future
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                DecimalPlaces = 2
            };

            Assert.IsFalse(future.IsPrice(4500.10m));
            Assert.IsFalse(future.IsPrice(4500.33m));
            Assert.IsFalse(future.IsPrice(4500.99m));
        }

        [Test]
        public void IsPrice_WithCLFuture_ValidatesTwoDecimalPlaces()
        {
            var future = new Future
            {
                Symbol = Symbol.CL,
                TickSize = 0.01m,
                DecimalPlaces = 2
            };

            Assert.IsTrue(future.IsPrice(85.50m));
            Assert.IsTrue(future.IsPrice(85.01m));
            Assert.IsFalse(future.IsPrice(85.505m));
        }

        [Test]
        public void IsPrice_WithBPFuture_ValidatesFourDecimalPlaces()
        {
            var future = new Future
            {
                Symbol = Symbol.BP,
                TickSize = 0.0001m,
                DecimalPlaces = 4
            };

            Assert.IsTrue(future.IsPrice(1.2500m));
            Assert.IsTrue(future.IsPrice(1.2501m));
            Assert.IsFalse(future.IsPrice(1.25005m));
        }

        [Test]
        public void IsPrice_WithZero_ReturnsTrue()
        {
            var future = new Future
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m
            };

            Assert.IsTrue(future.IsPrice(0m));
        }

        [Test]
        public void IsPrice_WithNegativePrice_ReturnsTrue()
        {
            var future = new Future
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m
            };

            Assert.IsTrue(future.IsPrice(-4500.25m));
        }

        [Test]
        public void Properties_CanBeSet()
        {
            var future = new Future
            {
                Symbol = Symbol.ES,
                Name = "E-mini S&P 500",
                TickSize = 0.25m,
                TickValue = 12.50m,
                TicksPerPoint = 4,
                DecimalPlaces = 2,
                PriceFormat = "0.00"
            };

            Assert.AreEqual(Symbol.ES, future.Symbol);
            Assert.AreEqual("E-mini S&P 500", future.Name);
            Assert.AreEqual(0.25m, future.TickSize);
            Assert.AreEqual(12.50m, future.TickValue);
            Assert.AreEqual(4, future.TicksPerPoint);
            Assert.AreEqual(2, future.DecimalPlaces);
            Assert.AreEqual("0.00", future.PriceFormat);
        }
    }
}

