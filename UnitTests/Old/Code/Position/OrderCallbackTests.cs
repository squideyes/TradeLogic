//using System;
//using NUnit.Framework;
//using TradeLogic;

//namespace TradeLogic.UnitTests
//{
//    [TestFixture]
//    public class OrderCallbackTests
//    {
//        private PositionManager CreatePositionManager()
//        {
//            var config = new PositionConfig { Symbol = Symbol.ES };
//            var idGen = new MockIdGenerator();
//            var logger = new MockLogger();
//            return new PositionManager(config, idGen, logger);
//        }

//        [Test]
//        public void HandleOrderAccepted_UpdatesState()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var update = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(update);
//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
//        }

//        [Test]
//        public void HandleOrderRejected_ReturnsToFlat()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var update = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
//            pm.HandleOrderRejected(update);
//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
//        }

//        [Test]
//        public void HandleOrderCanceled_ReturnsToFlat()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            var cancelUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Canceled, "User canceled");
//            pm.HandleOrderCanceled(cancelUpdate);
//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
//        }

//        [Test]
//        public void HandleOrderExpired_EventFired()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            bool eventFired = false;
//            pm.OrderExpired += (id, order) => { eventFired = true; };
//            var expireUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Expired, "Order expired");
//            pm.HandleOrderExpired(expireUpdate);
//            Assert.That(eventFired, Is.True);
//        }

//        [Test]
//        public void HandleOrderFilled_EntryTransitionsToOpen()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.Open));
//            Assert.That(position.NetQuantity, Is.EqualTo(100));
//        }

//        [Test]
//        public void OrderAccepted_EventFired()
//        {
//            var pm = CreatePositionManager();
//            bool eventFired = false;
//            pm.OrderAccepted += (id, order) => { eventFired = true; };
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var update = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(update);
//            Assert.That(eventFired, Is.True);
//        }

//        [Test]
//        public void OrderRejected_EventFired()
//        {
//            var pm = CreatePositionManager();
//            bool eventFired = false;
//            pm.OrderRejected += (id, order) => { eventFired = true; };
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var update = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
//            pm.HandleOrderRejected(update);
//            Assert.That(eventFired, Is.True);
//        }

//        [Test]
//        public void OrderCanceled_EventFired()
//        {
//            var pm = CreatePositionManager();
//            bool eventFired = false;
//            pm.OrderCanceled += (id, order) => { eventFired = true; };
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            var cancelUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Canceled, "User canceled");
//            pm.HandleOrderCanceled(cancelUpdate);
//            Assert.That(eventFired, Is.True);
//        }

//        [Test]
//        public void OrderExpired_EventFired()
//        {
//            var pm = CreatePositionManager();
//            bool eventFired = false;
//            pm.OrderExpired += (id, order) => { eventFired = true; };
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            var expireUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Expired, "Order expired");
//            pm.HandleOrderExpired(expireUpdate);
//            Assert.That(eventFired, Is.True);
//        }

//        [Test]
//        public void OrderFilled_EventFired()
//        {
//            var pm = CreatePositionManager();
//            bool eventFired = false;
//            pm.OrderFilled += (id, order, fill) => { eventFired = true; };
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
//            Assert.That(eventFired, Is.True);
//        }

//        [Test]
//        public void HandleOrderPartiallyFilled_PartialFill()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
//        }

//        [Test]
//        public void HandleOrderFilled_AfterPartialFill()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
//            pm.HandleOrderFilled(orderId, "fill2", 100m, 50, new DateTime(2024, 1, 15, 10, 2, 0));
//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.Open));
//            Assert.That(position.NetQuantity, Is.EqualTo(50));
//        }
//    }
//}


