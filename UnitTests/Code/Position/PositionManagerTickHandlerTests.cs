using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class PositionManagerTickHandlerTests
    {
        private PositionManager _pm;
        private PositionConfig _config;
        private List<Bar> _barsReceived;
        private List<(Guid posId, Bar bar)> _barClosedEvents;

        [SetUp]
        public void Setup()
        {
            _config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.01m,
                PointValue = 1m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            _pm = new PositionManager(_config, new GuidIdGenerator(), new MockLogger());
            _barsReceived = new List<Bar>();
            _barClosedEvents = new List<(Guid, Bar)>();

            // Subscribe to BarClosed event
            _pm.BarClosed += (posId, bar) =>
            {
                _barClosedEvents.Add((posId, bar));
                _barsReceived.Add(bar);
            };
        }

        [Test]
        public void PositionManager_ImplementsITickHandler()
        {
            Assert.That(_pm, Is.InstanceOf<ITickHandler>());
        }

        [Test]
        public void OnTick_WithValidTick_DoesNotThrow()
        {
            var tick = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            Assert.DoesNotThrow(() => _pm.OnTick(tick));
        }

        [Test]
        public void OnBar_WithValidBar_RaisesBarClosedEvent()
        {
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            _pm.OnBar(bar);

            Assert.That(_barClosedEvents.Count, Is.EqualTo(1));
            Assert.That(_barClosedEvents[0].bar, Is.EqualTo(bar));
        }

        [Test]
        public void OnBar_WithValidBar_EventContainsPositionId()
        {
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            _pm.OnBar(bar);

            Assert.That(_barClosedEvents[0].posId, Is.EqualTo(_pm.PositionId));
        }

        [Test]
        public void OnBar_WithNullBar_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _pm.OnBar(null));
        }

        [Test]
        public void OnBar_MultipleBarsClosed_RaisesEventForEach()
        {
            var bar1 = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            var bar2 = new Bar(new DateTime(2024, 1, 15, 9, 31, 0), 102m, 107m, 100m, 105m, 1200);
            var bar3 = new Bar(new DateTime(2024, 1, 15, 9, 32, 0), 105m, 110m, 103m, 108m, 1500);

            _pm.OnBar(bar1);
            _pm.OnBar(bar2);
            _pm.OnBar(bar3);

            Assert.That(_barClosedEvents.Count, Is.EqualTo(3));
            Assert.That(_barsReceived[0], Is.EqualTo(bar1));
            Assert.That(_barsReceived[1], Is.EqualTo(bar2));
            Assert.That(_barsReceived[2], Is.EqualTo(bar3));
        }

        [Test]
        public void OnBar_BarDataPreserved_InEvent()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 105m, 95m, 102m, 1000);
            _pm.OnBar(bar);

            var receivedBar = _barsReceived[0];
            Assert.That(receivedBar.OpenET, Is.EqualTo(openET));
            Assert.That(receivedBar.Open, Is.EqualTo(100m));
            Assert.That(receivedBar.High, Is.EqualTo(105m));
            Assert.That(receivedBar.Low, Is.EqualTo(95m));
            Assert.That(receivedBar.Close, Is.EqualTo(102m));
            Assert.That(receivedBar.Volume, Is.EqualTo(1000));
        }

        [Test]
        public void OnTick_ThenOnBar_BothProcessed()
        {
            var tick = new Tick(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 99.99m, 100.01m, 100);
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);

            _pm.OnTick(tick);
            _pm.OnBar(bar);

            Assert.That(_barClosedEvents.Count, Is.EqualTo(1));
            Assert.That(_barsReceived[0], Is.EqualTo(bar));
        }

        [Test]
        public void OnBar_WithZeroVolume_RaisesEvent()
        {
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 0);
            _pm.OnBar(bar);

            Assert.That(_barClosedEvents.Count, Is.EqualTo(1));
            Assert.That(_barsReceived[0].Volume, Is.EqualTo(0));
        }

        [Test]
        public void OnBar_WithLargeVolume_RaisesEvent()
        {
            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, long.MaxValue);
            _pm.OnBar(bar);

            Assert.That(_barClosedEvents.Count, Is.EqualTo(1));
            Assert.That(_barsReceived[0].Volume, Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void OnBar_EventFiredBeforeReturning()
        {
            bool eventFired = false;
            _pm.BarClosed += (posId, bar) => eventFired = true;

            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            _pm.OnBar(bar);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnBar_WithMultipleSubscribers_AllReceiveEvent()
        {
            int subscriber1Count = 0;
            int subscriber2Count = 0;

            _pm.BarClosed += (posId, bar) => subscriber1Count++;
            _pm.BarClosed += (posId, bar) => subscriber2Count++;

            var bar = new Bar(new DateTime(2024, 1, 15, 9, 30, 0), 100m, 105m, 95m, 102m, 1000);
            _pm.OnBar(bar);

            Assert.That(subscriber1Count, Is.EqualTo(1));
            Assert.That(subscriber2Count, Is.EqualTo(1));
        }
    }
}

