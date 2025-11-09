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

namespace WickScalper.UnitTests.Common.PricingTests
{
    [TestFixture]
    public class KnownFuturesTests
    {
        [Test]
        public void GetFuture_ES_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.ES);

            Assert.AreEqual(Symbol.ES, future.Symbol);
            Assert.AreEqual("E-mini S&P 500", future.Name);
            Assert.AreEqual(0.25m, future.TickSize);
            Assert.AreEqual(12.50m, future.TickValue);
            Assert.AreEqual(4, future.TicksPerPoint);
            Assert.AreEqual(2, future.DecimalPlaces);
            Assert.AreEqual("0.00", future.PriceFormat);
        }

        [Test]
        public void GetFuture_CL_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.CL);

            Assert.AreEqual(Symbol.CL, future.Symbol);
            Assert.AreEqual("Crude Oil", future.Name);
            Assert.AreEqual(0.01m, future.TickSize);
            Assert.AreEqual(10.00m, future.TickValue);
            Assert.AreEqual(100, future.TicksPerPoint);
            Assert.AreEqual(2, future.DecimalPlaces);
        }

        [Test]
        public void GetFuture_BP_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.BP);

            Assert.AreEqual(Symbol.BP, future.Symbol);
            Assert.AreEqual("British Pound", future.Name);
            Assert.AreEqual(0.0001m, future.TickSize);
            Assert.AreEqual(6.25m, future.TickValue);
            Assert.AreEqual(10000, future.TicksPerPoint);
            Assert.AreEqual(4, future.DecimalPlaces);
        }

        [Test]
        public void GetFuture_EU_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.EU);

            Assert.AreEqual(Symbol.EU, future.Symbol);
            Assert.AreEqual("Euro FX", future.Name);
            Assert.AreEqual(0.00005m, future.TickSize);
            Assert.AreEqual(6.25m, future.TickValue);
            Assert.AreEqual(20000, future.TicksPerPoint);
            Assert.AreEqual(5, future.DecimalPlaces);
        }

        [Test]
        public void GetFuture_GC_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.GC);

            Assert.AreEqual(Symbol.GC, future.Symbol);
            Assert.AreEqual("Gold", future.Name);
            Assert.AreEqual(0.10m, future.TickSize);
            Assert.AreEqual(10.00m, future.TickValue);
            Assert.AreEqual(10, future.TicksPerPoint);
            Assert.AreEqual(1, future.DecimalPlaces);
        }

        [Test]
        public void GetFuture_NQ_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.NQ);

            Assert.AreEqual(Symbol.NQ, future.Symbol);
            Assert.AreEqual("E-mini Nasdaq-100", future.Name);
            Assert.AreEqual(0.25m, future.TickSize);
            Assert.AreEqual(5.00m, future.TickValue);
        }

        [Test]
        public void GetFuture_MES_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.MES);

            Assert.AreEqual(Symbol.MES, future.Symbol);
            Assert.AreEqual("Micro E-mini S&P 500", future.Name);
            Assert.AreEqual(0.25m, future.TickSize);
            Assert.AreEqual(1.25m, future.TickValue);
        }

        [Test]
        public void GetFuture_MNQ_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.MNQ);

            Assert.AreEqual(Symbol.MNQ, future.Symbol);
            Assert.AreEqual("Micro E-mini Nasdaq-100", future.Name);
        }

        [Test]
        public void GetFuture_MCL_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.MCL);

            Assert.AreEqual(Symbol.MCL, future.Symbol);
            Assert.AreEqual("Micro Crude Oil", future.Name);
        }

        [Test]
        public void GetFuture_JY_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.JY);

            Assert.AreEqual(Symbol.JY, future.Symbol);
            Assert.AreEqual("Japanese Yen", future.Name);
            Assert.AreEqual(0.0000005m, future.TickSize);
            Assert.AreEqual(7, future.DecimalPlaces);
        }

        [Test]
        public void GetFuture_FV_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.FV);

            Assert.AreEqual(Symbol.FV, future.Symbol);
            Assert.AreEqual("5-Year Treasury Note", future.Name);
        }

        [Test]
        public void GetFuture_TY_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.TY);

            Assert.AreEqual(Symbol.TY, future.Symbol);
            Assert.AreEqual("10-Year Treasury Note", future.Name);
        }

        [Test]
        public void GetFuture_US_ReturnsCorrectFuture()
        {
            var future = KnownFutures.GetFuture(Symbol.US);

            Assert.AreEqual(Symbol.US, future.Symbol);
            Assert.AreEqual("30-Year Treasury Bond", future.Name);
        }

        [Test]
        public void GetFuture_AllSymbols_ReturnNonNull()
        {
            foreach (Symbol symbol in System.Enum.GetValues(typeof(Symbol)))
            {
                var future = KnownFutures.GetFuture(symbol);
                Assert.IsNotNull(future);
                Assert.AreEqual(symbol, future.Symbol);
            }
        }
    }
}

