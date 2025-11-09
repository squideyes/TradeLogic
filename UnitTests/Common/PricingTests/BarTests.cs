using NUnit.Framework;
using System;
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
    public class BarTests
    {
        private DateTime testOpenET;
        private Tick testTick;

        [SetUp]
        public void Setup()
        {
            testOpenET = new DateTime(2024, 1, 2, 9, 30, 0);
            testTick = new Tick(testOpenET, 4500.00m, 4499.75m, 4500.25m, 100);
        }

        [Test]
        public void Constructor_WithTick_InitializesBarCorrectly()
        {
            var bar = new Bar(testTick, testOpenET);

            Assert.AreEqual(testOpenET, bar.OpenET);
            Assert.AreEqual(4500.00m, bar.Open);
            Assert.AreEqual(4500.00m, bar.High);
            Assert.AreEqual(4500.00m, bar.Low);
            Assert.AreEqual(4500.00m, bar.Close);
            Assert.AreEqual(100, bar.Volume);
        }

        [Test]
        public void Constructor_WithValues_InitializesBarCorrectly()
        {
            var bar = new Bar(testOpenET, 4500.00m, 4505.00m, 4495.00m, 4502.00m, 500);

            Assert.AreEqual(testOpenET, bar.OpenET);
            Assert.AreEqual(4500.00m, bar.Open);
            Assert.AreEqual(4505.00m, bar.High);
            Assert.AreEqual(4495.00m, bar.Low);
            Assert.AreEqual(4502.00m, bar.Close);
            Assert.AreEqual(500, bar.Volume);
        }

        [Test]
        public void Adjust_UpdatesHighLowCloseVolume()
        {
            var bar = new Bar(testTick, testOpenET);
            var newTick = new Tick(testOpenET.AddSeconds(1), 4510.00m, 4509.75m, 4510.25m, 50);

            bar.Adjust(newTick);

            Assert.AreEqual(4510.00m, bar.High);
            Assert.AreEqual(4500.00m, bar.Low);
            Assert.AreEqual(4510.00m, bar.Close);
            Assert.AreEqual(150, bar.Volume);
        }

        [Test]
        public void Adjust_WithLowerPrice_UpdatesLow()
        {
            var bar = new Bar(testTick, testOpenET);
            var newTick = new Tick(testOpenET.AddSeconds(1), 4490.00m, 4489.75m, 4490.25m, 50);

            bar.Adjust(newTick);

            Assert.AreEqual(4500.00m, bar.High);
            Assert.AreEqual(4490.00m, bar.Low);
            Assert.AreEqual(4490.00m, bar.Close);
        }

        [Test]
        public void GetTrueRange_WithoutPrevClose_ReturnsHighMinusLow()
        {
            var bar = new Bar(testOpenET, 4500.00m, 4510.00m, 4490.00m, 4505.00m, 500);

            var tr = bar.GetTrueRange();

            Assert.AreEqual(20.00m, tr);
        }

        [Test]
        public void GetTrueRange_WithPrevClose_ReturnsMaxRange()
        {
            var bar = new Bar(testOpenET, 4500.00m, 4510.00m, 4490.00m, 4505.00m, 500);
            var prevClose = 4520.00m;

            var tr = bar.GetTrueRange(prevClose);

            Assert.AreEqual(30.00m, tr); // Max of (20, 10, 30)
        }

        [Test]
        public void GetTrueRange_WithPrevCloseBelowLow_ReturnsCorrectRange()
        {
            var bar = new Bar(testOpenET, 4500.00m, 4510.00m, 4490.00m, 4505.00m, 500);
            var prevClose = 4480.00m;

            var tr = bar.GetTrueRange(prevClose);

            Assert.AreEqual(30.00m, tr); // Max of (20, 30, 10)
        }

        [Test]
        public void OpenET_IsReadOnly()
        {
            var bar = new Bar(testTick, testOpenET);
            Assert.AreEqual(testOpenET, bar.OpenET);
        }

        [Test]
        public void Open_IsReadOnly()
        {
            var bar = new Bar(testTick, testOpenET);
            Assert.AreEqual(4500.00m, bar.Open);
        }
    }
}

