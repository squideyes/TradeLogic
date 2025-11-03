using System;
using NUnit.Framework;
using TradeLogic.UnitTests.Fixtures;

namespace TradeLogic.UnitTests.Core.Position
{
    [TestFixture]
    public class ExitTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, feeModel, idGen, logger);
        }

        private void OpenPosition(PositionManager pm, Side side, int quantity)
        {
            var orderId = pm.SubmitEntry(OrderType.Market, side, quantity, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, quantity, new DateTime(2024, 1, 15, 10, 1, 0));
        }

        [Test]
        public void GoFlat_TransitionsToClosing()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Long, 100);
            pm.GoFlat();
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Closing));
        }

        [Test]
        public void SubmitEntry_WithExits_AtomicOperation()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(
                OrderType.Market, Side.Long, 100,
                stopLossPrice: 95m, takeProfitPrice: 105m);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
            Assert.That(position.StopLossPrice, Is.EqualTo(95m));
            Assert.That(position.TakeProfitPrice, Is.EqualTo(105m));
        }

        [Test]
        public void SubmitEntry_ReturnsEntryOrderId()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(
                OrderType.Market, Side.Long, 100,
                stopLossPrice: 95m, takeProfitPrice: 105m);

            Assert.That(entryId, Is.Not.Null);
            Assert.That(entryId, Is.Not.Empty);
        }

        [Test]
        public void SubmitEntry_WithoutExits()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(
                OrderType.Market, Side.Long, 100,
                stopLossPrice: null, takeProfitPrice: null);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
            Assert.That(position.StopLossPrice, Is.Null);
            Assert.That(position.TakeProfitPrice, Is.Null);
        }

        [Test]
        public void SubmitEntry_Short()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(
                OrderType.Market, Side.Short, 100,
                stopLossPrice: 105m, takeProfitPrice: 95m);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
            Assert.That(position.Side, Is.EqualTo(Side.Short));
            Assert.That(position.StopLossPrice, Is.EqualTo(105m));
            Assert.That(position.TakeProfitPrice, Is.EqualTo(95m));
        }

        [Test]
        public void SubmitEntry_ExitArmedEventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.ExitArmed += (id, position, extra) => { eventFired = true; };

            pm.SubmitEntry(
                OrderType.Market, Side.Long, 100,
                stopLossPrice: 95m, takeProfitPrice: 105m);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SubmitEntry_CannotCallFromNonFlat()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, 95m, 105m);
            var acceptUpdate = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(entryId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(
                    OrderType.Market, Side.Short, 50,
                    stopLossPrice: 95m, takeProfitPrice: 105m));
        }
    }
}


