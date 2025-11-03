using System;
using NUnit.Framework;
using TradeLogic.UnitTests.Fixtures;

namespace TradeLogic.UnitTests.Core.Position
{
    [TestFixture]
    public class OrderCallbackTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, feeModel, idGen, logger);
        }

        [Test]
        public void OnOrderAccepted_UpdatesState()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var update = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(update);
            var view = pm.GetView();
            Assert.That(view.State, Is.EqualTo(PositionState.PendingEntry));
        }

        [Test]
        public void OnOrderRejected_ReturnsToFlat()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var update = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
            pm.OnOrderRejected(update);
            var view = pm.GetView();
            Assert.That(view.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void OnOrderCanceled_ReturnsToFlat()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var cancelUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Canceled, "User canceled");
            pm.OnOrderCanceled(cancelUpdate);
            var view = pm.GetView();
            Assert.That(view.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void OnOrderExpired_EventFired()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            bool eventFired = false;
            pm.OrderExpired += (id, order) => { eventFired = true; };
            var expireUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Expired, "Order expired");
            pm.OnOrderExpired(expireUpdate);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnOrderFilled_EntryTransitionsToOpen()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            var view = pm.GetView();
            Assert.That(view.State, Is.EqualTo(PositionState.Open));
            Assert.That(view.NetQuantity, Is.EqualTo(100));
        }

        [Test]
        public void OrderAccepted_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderAccepted += (id, order) => { eventFired = true; };
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var update = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(update);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OrderRejected_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderRejected += (id, order) => { eventFired = true; };
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var update = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
            pm.OnOrderRejected(update);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OrderCanceled_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderCanceled += (id, order) => { eventFired = true; };
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var cancelUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Canceled, "User canceled");
            pm.OnOrderCanceled(cancelUpdate);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OrderExpired_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderExpired += (id, order) => { eventFired = true; };
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var expireUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Expired, "Order expired");
            pm.OnOrderExpired(expireUpdate);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OrderFilled_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.OrderFilled += (id, order, fill) => { eventFired = true; };
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnOrderFilled_PartialFill()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
            var view = pm.GetView();
            Assert.That(view.State, Is.EqualTo(PositionState.PendingEntry));
        }

        [Test]
        public void OnOrderFilled_AfterPartialFill()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
            pm.OnOrderFilled(orderId, "fill2", 100m, 50, new DateTime(2024, 1, 15, 10, 2, 0));
            var view = pm.GetView();
            Assert.That(view.State, Is.EqualTo(PositionState.Open));
            Assert.That(view.NetQuantity, Is.EqualTo(50));
        }
    }
}


