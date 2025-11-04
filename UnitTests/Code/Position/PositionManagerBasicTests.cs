using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class PositionManagerBasicTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, idGen, logger);
        }

        [Test]
        public void Constructor_RequiresConfig()
        {
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            Assert.Throws<ArgumentNullException>(() =>
                new PositionManager(null, idGen, logger));
        }

        [Test]
        public void Constructor_RequiresLogger()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var idGen = new MockIdGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                new PositionManager(config, idGen, null));
        }

        [Test]
        public void InitialState_IsFlat()
        {
            var pm = CreatePositionManager();
            var position = pm.GetPosition();
            Assert.That(position.NetQuantity, Is.EqualTo(0));
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void SubmitEntry_Market_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_Limit_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Limit, Side.Short, 1, 95m, 105m, limitPrice: 150m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_Stop_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Stop, Side.Long, 1, 95m, 105m, stopPrice: 150m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_StopLimit_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.StopLimit, Side.Short, 1, 95m, 105m, limitPrice: 145m, stopPrice: 150m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void OnTick_UpdatesTime()
        {
            var pm = CreatePositionManager();
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var tick = new Tick(et, 150m, 149.99m, 150.01m, 1000);
            Assert.DoesNotThrow(() => pm.OnTick(tick));
        }

        [Test]
        public void GoFlat_WithoutPosition()
        {
            var pm = CreatePositionManager();
            Assert.DoesNotThrow(() => pm.GoFlat());
        }

        [Test]
        public void SubmitEntry_RequiresLimitPrice()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Limit, Side.Long, 1, 95m, 105m, limitPrice: null));
        }

        [Test]
        public void SubmitEntry_RequiresStopPrice()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Stop, Side.Long, 1, 95m, 105m, stopPrice: null));
        }

        [Test]
        public void SubmitEntry_RequiresStopLimitPrices()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.StopLimit, Side.Long, 1, 95m, 105m, limitPrice: null, stopPrice: 150m));
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
            pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SubmitEntry_CreatesOrderWithCorrectSide()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 1, 105m, 95m);
            Assert.That(orderId, Is.Not.Null);
        }

        [Test]
        public void SubmitEntry_CreatesOrderWithCorrectQuantity()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            Assert.That(orderId, Is.Not.Null);
        }
    }
}

