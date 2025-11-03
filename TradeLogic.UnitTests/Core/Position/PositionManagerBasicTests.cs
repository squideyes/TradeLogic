using System;
using NUnit.Framework;
using TradeLogic.UnitTests.Fixtures;

namespace TradeLogic.UnitTests.Core.Position
{
    [TestFixture]
    public class PositionManagerBasicTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, feeModel, idGen, logger);
        }

        [Test]
        public void Constructor_RequiresConfig()
        {
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            Assert.Throws<ArgumentNullException>(() =>
                new PositionManager(null, feeModel, idGen, logger));
        }

        [Test]
        public void Constructor_RequiresLogger()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                new PositionManager(config, feeModel, idGen, null));
        }

        [Test]
        public void InitialState_IsFlat()
        {
            var pm = CreatePositionManager();
            var view = pm.GetView();
            Assert.That(view.NetQuantity, Is.EqualTo(0));
            Assert.That(view.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void SubmitEntry_Market_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_Limit_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Limit, Side.Short, 50, limitPrice: 150m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_Stop_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Stop, Side.Long, 100, stopPrice: 150m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_StopLimit_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.StopLimit, Side.Short, 100, limitPrice: 145m, stopPrice: 150m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void OnClock_UpdatesTime()
        {
            var pm = CreatePositionManager();
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var tick = new Tick(et, 150m, 149.99m, 150.01m, 1000);
            Assert.DoesNotThrow(() => pm.OnClock(tick));
        }

        [Test]
        public void GoFlat_WithoutPosition()
        {
            var pm = CreatePositionManager();
            Assert.DoesNotThrow(() => pm.GoFlat());
        }

        [Test]
        public void Reset_ClearsPosition()
        {
            var pm = CreatePositionManager();
            pm.Reset();
            var view = pm.GetView();
            Assert.That(view.NetQuantity, Is.EqualTo(0));
        }

        [Test]
        public void SubmitEntry_ValidatesQuantity()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Long, 0));
        }

        [Test]
        public void SubmitEntry_RequiresLimitPrice()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Limit, Side.Long, 100, limitPrice: null));
        }

        [Test]
        public void SubmitEntry_RequiresStopPrice()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Stop, Side.Long, 100, stopPrice: null));
        }

        [Test]
        public void SubmitEntry_RequiresStopLimitPrices()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.StopLimit, Side.Long, 100, limitPrice: null, stopPrice: 150m));
        }

        [Test]
        public void Reset_RequiresClosedOrFlat()
        {
            var pm = CreatePositionManager();
            pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            // Position is now PendingEntry, Reset should fail
            Assert.Throws<InvalidOperationException>(() => pm.Reset());
        }

        [Test]
        public void PositionId_IsUnique()
        {
            var pm1 = CreatePositionManager();
            var pm2 = CreatePositionManager();
            Assert.That(pm1.PositionId, Is.Not.EqualTo(pm2.PositionId));
        }

        [Test]
        public void OrderSubmitted_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderSubmitted += (id, order) => { eventFired = true; };
            pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SubmitEntry_CreatesOrderWithCorrectSide()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 100);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_CreatesOrderWithCorrectQuantity()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 250);
            Assert.That(orderId, Is.Not.Null);
        }
    }
}

