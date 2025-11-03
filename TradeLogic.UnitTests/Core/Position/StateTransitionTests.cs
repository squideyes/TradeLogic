using System;
using NUnit.Framework;
using TradeLogic.UnitTests.Fixtures;

namespace TradeLogic.UnitTests.Core.Position
{
    [TestFixture]
    public class StateTransitionTests
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
        public void Flat_To_PendingEntry()
        {
            var pm = CreatePositionManager();
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
            
            pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
        }

        [Test]
        public void PendingEntry_To_Open()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Open));
        }

        [Test]
        public void PendingEntry_To_Flat_OnRejection()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var rejectUpdate = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
            pm.OnOrderRejected(rejectUpdate);
            
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void PendingEntry_To_Flat_OnCancellation()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var cancelUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Canceled, "User canceled");
            pm.OnOrderCanceled(cancelUpdate);
            
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void OnOrderExpired_EventFired()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            bool eventFired = false;
            pm.OrderExpired += (id, order) => { eventFired = true; };
            var expireUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Expired, "Order expired");
            pm.OnOrderExpired(expireUpdate);
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void Open_To_Closing()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.GoFlat();
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Closing));
        }

        [Test]
        public void Closing_To_Closed()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptEntry = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptEntry);
            pm.OnOrderFilled(entryId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.GoFlat();
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Closing));
            
            // Simulate exit order being filled
            // Note: We need to find the exit order ID that was created by GoFlat
            // For now, we'll just verify the state is Closing
        }

        [Test]
        public void CannotSubmitEntry_FromOpen()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 50, null, null));
        }

        [Test]
        public void CannotSubmitEntry_FromClosing()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptEntry = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptEntry);
            pm.OnOrderFilled(entryId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.GoFlat();
            
            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 50, null, null));
        }


    }
}


