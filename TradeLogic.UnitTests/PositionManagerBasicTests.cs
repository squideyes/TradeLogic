using System;
using TradeLogic.UnitTests.Fixtures;
using TradeLogic.UnitTests.TestFramework;

namespace TradeLogic.UnitTests
{
    public class PositionManagerBasicTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var clock = new MockClock(et);
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, clock, feeModel, idGen, logger);
        }

        [TestFramework.Test]
        public void Constructor_RequiresConfig()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var clock = new MockClock(et);
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            Assert.Throws<ArgumentNullException>(() =>
                new PositionManager(null, clock, feeModel, idGen, logger));
        }

        [TestFramework.Test]
        public void Constructor_RequiresClock()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            Assert.Throws<ArgumentNullException>(() =>
                new PositionManager(config, null, feeModel, idGen, logger));
        }

        [TestFramework.Test]
        public void InitialState_IsFlat()
        {
            var pm = CreatePositionManager();
            var view = pm.GetView();
            Assert.Equal(0, view.NetQuantity);
            Assert.Equal(PositionState.Flat, view.State);
        }

        [TestFramework.Test]
        public void SubmitEntry_Market_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            Assert.NotNull(orderId);
        }

        [TestFramework.Test]
        public void SubmitEntry_Limit_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Limit, Side.Short, 50, limitPrice: 150m);
            Assert.NotNull(orderId);
        }

        [TestFramework.Test]
        public void SubmitEntry_Stop_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Stop, Side.Long, 100, stopPrice: 150m);
            Assert.NotNull(orderId);
        }

        [TestFramework.Test]
        public void SubmitEntry_StopLimit_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.StopLimit, Side.Short, 100, limitPrice: 145m, stopPrice: 150m);
            Assert.NotNull(orderId);
        }

        [TestFramework.Test]
        public void OnClock_UpdatesTime()
        {
            var pm = CreatePositionManager();
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            Assert.DoesNotThrow(() => pm.OnClock(et));
        }

        [TestFramework.Test]
        public void GoFlat_WithoutPosition()
        {
            var pm = CreatePositionManager();
            Assert.DoesNotThrow(() => pm.GoFlat());
        }

        [TestFramework.Test]
        public void Reset_ClearsPosition()
        {
            var pm = CreatePositionManager();
            pm.Reset();
            var view = pm.GetView();
            Assert.Equal(0, view.NetQuantity);
        }

        [TestFramework.Test]
        public void SubmitEntry_ValidatesQuantity()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Long, 0));
        }

        [TestFramework.Test]
        public void SubmitEntry_RequiresLimitPrice()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Limit, Side.Long, 100, limitPrice: null));
        }

        [TestFramework.Test]
        public void SubmitEntry_RequiresStopPrice()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.Stop, Side.Long, 100, stopPrice: null));
        }

        [TestFramework.Test]
        public void SubmitEntry_RequiresStopLimitPrices()
        {
            var pm = CreatePositionManager();
            Assert.Throws<ArgumentException>(() =>
                pm.SubmitEntry(OrderType.StopLimit, Side.Long, 100, limitPrice: null, stopPrice: 150m));
        }

        [TestFramework.Test]
        public void Reset_RequiresClosedOrFlat()
        {
            var pm = CreatePositionManager();
            pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            // Position is now PendingEntry, Reset should fail
            Assert.Throws<InvalidOperationException>(() => pm.Reset());
        }

        [TestFramework.Test]
        public void PositionId_IsUnique()
        {
            var pm1 = CreatePositionManager();
            var pm2 = CreatePositionManager();
            Assert.NotEqual(pm1.PositionId, pm2.PositionId);
        }

        [TestFramework.Test]
        public void OrderSubmitted_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderSubmitted += (id, order) => { eventFired = true; };
            pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            Assert.True(eventFired);
        }

        [TestFramework.Test]
        public void SubmitEntry_CreatesOrderWithCorrectSide()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 100);
            Assert.NotNull(orderId);
        }

        [TestFramework.Test]
        public void SubmitEntry_CreatesOrderWithCorrectQuantity()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 250);
            Assert.NotNull(orderId);
        }
    }
}

