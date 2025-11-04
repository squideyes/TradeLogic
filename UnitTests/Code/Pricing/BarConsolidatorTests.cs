using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class BarConsolidatorTests
    {
        private List<Bar> _closedBars;

        [SetUp]
        public void Setup()
        {
            _closedBars = new List<Bar>();
        }

        private BarConsolidator CreateConsolidator(TimeSpan barPeriod)
        {
            return new BarConsolidator(barPeriod, bar => _closedBars.Add(bar));
        }

        [Test]
        public void Constructor_ValidPeriod_CreatesConsolidator()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            Assert.That(consolidator, Is.Not.Null);
        }

        [Test]
        public void Constructor_ZeroPeriod_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => CreateConsolidator(TimeSpan.Zero));
        }

        [Test]
        public void Constructor_NegativePeriod_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => CreateConsolidator(TimeSpan.FromSeconds(-1)));
        }

        [Test]
        public void Constructor_NullCallback_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new BarConsolidator(TimeSpan.FromMinutes(1), null));
        }

        [Test]
        public void ProcessTick_NullTick_ThrowsException()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            Assert.Throws<ArgumentNullException>(() => consolidator.ProcessTick(null));
        }

        [Test]
        public void ProcessTick_SingleTick_NoBarClosed()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            
            consolidator.ProcessTick(tick);
            
            Assert.That(_closedBars.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessTick_TwoTicksSameBar_NoBarClosed()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 30), 101m, 100.99m, 101.01m, 150);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            
            Assert.That(_closedBars.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessTick_TicksCrossingBarBoundary_ClosesBar()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 101m, 100.99m, 101.01m, 150);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            
            Assert.That(_closedBars.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProcessTick_ClosedBar_HasCorrectOpenET()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 15), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 101m, 100.99m, 101.01m, 150);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            
            Assert.That(_closedBars[0].OpenET, Is.EqualTo(new DateTime(2024, 1, 15, 9, 30, 0)));
        }

        [Test]
        public void ProcessTick_ClosedBar_HasCorrectOHLC()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 30), 105m, 104.99m, 105.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 102m, 101.99m, 102.01m, 200);

            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            consolidator.ProcessTick(tick3);

            var bar = _closedBars[0];
            Assert.That(bar.Open, Is.EqualTo(100m));
            Assert.That(bar.High, Is.EqualTo(105m));
            Assert.That(bar.Low, Is.EqualTo(100m));
            Assert.That(bar.Close, Is.EqualTo(105m));  // Last tick in bar is tick2
        }

        [Test]
        public void ProcessTick_ClosedBar_HasCorrectVolume()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 30), 101m, 100.99m, 101.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 102m, 101.99m, 102.01m, 200);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            consolidator.ProcessTick(tick3);
            
            Assert.That(_closedBars[0].Volume, Is.EqualTo(250));
        }

        [Test]
        public void ProcessTick_MultipleBarPeriods_ClosesMultipleBars()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 101m, 100.99m, 101.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 32, 0), 102m, 101.99m, 102.01m, 200);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            consolidator.ProcessTick(tick3);
            
            Assert.That(_closedBars.Count, Is.EqualTo(2));
        }

        [Test]
        public void ProcessTick_SecondBarPeriod_HasCorrectOpenET()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 101m, 100.99m, 101.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 32, 0), 102m, 101.99m, 102.01m, 200);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            consolidator.ProcessTick(tick3);
            
            Assert.That(_closedBars[1].OpenET, Is.EqualTo(new DateTime(2024, 1, 15, 9, 31, 0)));
        }

        [Test]
        public void ProcessTick_FiveSecondBars_ClosesCorrectly()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromSeconds(5));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 5), 101m, 100.99m, 101.01m, 150);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            
            Assert.That(_closedBars.Count, Is.EqualTo(1));
            Assert.That(_closedBars[0].OpenET, Is.EqualTo(new DateTime(2024, 1, 15, 9, 30, 0)));
        }

        [Test]
        public void ProcessTick_TickWithinBar_UpdatesHighLow()
        {
            var consolidator = CreateConsolidator(TimeSpan.FromMinutes(1));
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 15), 95m, 94.99m, 95.01m, 50);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 30, 30), 110m, 109.99m, 110.01m, 75);
            var tick4 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 105m, 104.99m, 105.01m, 100);
            
            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            consolidator.ProcessTick(tick3);
            consolidator.ProcessTick(tick4);
            
            var bar = _closedBars[0];
            Assert.That(bar.High, Is.EqualTo(110m));
            Assert.That(bar.Low, Is.EqualTo(95m));
        }
    }
}

