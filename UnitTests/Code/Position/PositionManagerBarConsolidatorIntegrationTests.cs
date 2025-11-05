using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class PositionManagerBarConsolidatorIntegrationTests
    {
        private PositionManager _pm;
        private BarConsolidator _consolidator;
        private List<Bar> _barsFromConsolidator;
        private List<Bar> _barsFromPositionManager;

        [SetUp]
        public void Setup()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.01m,
                PointValue = 1m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            _pm = new PositionManager(config, new GuidIdGenerator(), new MockLogger());
            _barsFromConsolidator = new List<Bar>();
            _barsFromPositionManager = new List<Bar>();

            // Create consolidator that feeds bars to PositionManager
            _consolidator = new BarConsolidator(TimeSpan.FromMinutes(1), bar =>
            {
                _barsFromConsolidator.Add(bar);
                _pm.OnBar(bar);
            });

            // Subscribe to PositionManager's BarClosed event
            _pm.BarClosed += (posId, bar) => _barsFromPositionManager.Add(bar);
        }

        [Test]
        public void BarConsolidator_FeedsTicksToPositionManager_BarClosedEventRaised()
        {
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 30), 101m, 100.99m, 101.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 102m, 101.99m, 102.01m, 200);

            _consolidator.ProcessTick(tick1);
            _consolidator.ProcessTick(tick2);
            _consolidator.ProcessTick(tick3);

            Assert.That(_barsFromConsolidator.Count, Is.EqualTo(1));
            Assert.That(_barsFromPositionManager.Count, Is.EqualTo(1));
        }

        [Test]
        public void BarConsolidator_BarDataMatches_BetweenConsolidatorAndPositionManager()
        {
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 30), 105m, 104.99m, 105.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 102m, 101.99m, 102.01m, 200);

            _consolidator.ProcessTick(tick1);
            _consolidator.ProcessTick(tick2);
            _consolidator.ProcessTick(tick3);

            var barFromConsolidator = _barsFromConsolidator[0];
            var barFromPM = _barsFromPositionManager[0];

            Assert.That(barFromPM.OpenET, Is.EqualTo(barFromConsolidator.OpenET));
            Assert.That(barFromPM.Open, Is.EqualTo(barFromConsolidator.Open));
            Assert.That(barFromPM.High, Is.EqualTo(barFromConsolidator.High));
            Assert.That(barFromPM.Low, Is.EqualTo(barFromConsolidator.Low));
            Assert.That(barFromPM.Close, Is.EqualTo(barFromConsolidator.Close));
            Assert.That(barFromPM.Volume, Is.EqualTo(barFromConsolidator.Volume));
        }

        [Test]
        public void MultipleBarPeriods_AllBarsReachedPositionManager()
        {
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 101m, 100.99m, 101.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 32, 0), 102m, 101.99m, 102.01m, 200);

            _consolidator.ProcessTick(tick1);
            _consolidator.ProcessTick(tick2);
            _consolidator.ProcessTick(tick3);

            Assert.That(_barsFromConsolidator.Count, Is.EqualTo(2));
            Assert.That(_barsFromPositionManager.Count, Is.EqualTo(2));
        }

        [Test]
        public void PositionManager_ImplementsITickHandler_CanBeUsedAsHandler()
        {
            Assert.That(_pm, Is.InstanceOf<ITickHandler>());
            
            var tick = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            ITickHandler handler = _pm;
            
            Assert.DoesNotThrow(() => handler.OnTick(tick));
        }

        [Test]
        public void PositionManager_OnBar_CanBeCalledDirectly()
        {
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            
            Assert.DoesNotThrow(() => _pm.OnBar(bar));
            Assert.That(_barsFromPositionManager.Count, Is.EqualTo(1));
        }

        [Test]
        public void BarConsolidator_FiveSecondBars_AllReachPositionManager()
        {
            var consolidator = new BarConsolidator(TimeSpan.FromSeconds(5), bar => _pm.OnBar(bar));
            
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 5), 101m, 100.99m, 101.01m, 150);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 30, 10), 102m, 101.99m, 102.01m, 200);

            consolidator.ProcessTick(tick1);
            consolidator.ProcessTick(tick2);
            consolidator.ProcessTick(tick3);

            Assert.That(_barsFromPositionManager.Count, Is.EqualTo(2));
        }

        [Test]
        public void BarConsolidator_TicksWithinBar_PositionManagerReceivesOncePerBar()
        {
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 30, 10), 100.5m, 100.49m, 100.51m, 50);
            var tick3 = new Tick(new DateTime(2024, 1, 15, 9, 30, 20), 101m, 100.99m, 101.01m, 75);
            var tick4 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 102m, 101.99m, 102.01m, 200);

            _consolidator.ProcessTick(tick1);
            _consolidator.ProcessTick(tick2);
            _consolidator.ProcessTick(tick3);
            _consolidator.ProcessTick(tick4);

            // Only 1 bar should be closed (first bar), so only 1 event
            Assert.That(_barsFromPositionManager.Count, Is.EqualTo(1));
        }

        [Test]
        public void PositionManager_BarClosedEvent_IncludesCorrectPositionId()
        {
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            var positionId = _pm.PositionId;

            _pm.OnBar(bar);

            Assert.That(_barsFromPositionManager.Count, Is.EqualTo(1));
            // Verify through the event subscription that position ID matches
            var eventFired = false;
            _pm.BarClosed += (posId, b) =>
            {
                if (posId == positionId)
                    eventFired = true;
            };

            _pm.OnBar(bar);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void BarConsolidator_LargeVolume_PreservedInBar()
        {
            var tick1 = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 1000000);
            var tick2 = new Tick(new DateTime(2024, 1, 15, 9, 31, 0), 101m, 100.99m, 101.01m, 2000000);

            _consolidator.ProcessTick(tick1);
            _consolidator.ProcessTick(tick2);

            Assert.That(_barsFromPositionManager[0].Volume, Is.EqualTo(1000000));
        }
    }
}

