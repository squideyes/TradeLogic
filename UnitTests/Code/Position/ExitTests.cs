using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class ExitTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, idGen, logger);
        }

        private void OpenPosition(PositionManager pm, Side side, int quantity)
        {
            var orderId = pm.SubmitEntry(OrderType.Market, side, quantity);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
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
        public void SubmitEntry_WithSetExitPrices()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));

            // Set exit prices after entry submission
            pm.SetExitPrices(95m, 105m);
            position = pm.GetPosition();
            Assert.That(position.StopLossPrice, Is.EqualTo(95m));
            Assert.That(position.TakeProfitPrice, Is.EqualTo(105m));
        }

        [Test]
        public void SubmitEntry_ReturnsEntryOrderId()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);

            Assert.That(entryId, Is.Not.Null);
            Assert.That(entryId, Is.Not.Empty);
        }

        [Test]
        public void SubmitEntry_Short()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Short, 1);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
            Assert.That(position.Side, Is.EqualTo(Side.Short));

            // Set exit prices after entry submission
            pm.SetExitPrices(105m, 95m);
            position = pm.GetPosition();
            Assert.That(position.StopLossPrice, Is.EqualTo(105m));
            Assert.That(position.TakeProfitPrice, Is.EqualTo(95m));
        }

        [Test]
        public void SetExitPrices_ExitArmedEventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.ExitArmed += (id, position, extra) => { eventFired = true; };

            pm.SubmitEntry(OrderType.Market, Side.Long, 1);
            pm.SetExitPrices(95m, 105m);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void SubmitEntry_CannotCallFromNonFlat()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
            var acceptUpdate = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(entryId, "fill1", 100m, 1, new DateTime(2024, 1, 15, 10, 1, 0));

            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 1));
        }
    }
}


