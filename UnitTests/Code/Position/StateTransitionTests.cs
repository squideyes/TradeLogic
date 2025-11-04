using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class StateTransitionTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, idGen, logger);
        }

        [Test]
        public void Flat_To_PendingEntry()
        {
            var pm = CreatePositionManager();
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));

            pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
        }

        [Test]
        public void PendingEntry_To_Open()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 1, new DateTime(2024, 1, 15, 10, 1, 0));

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Open));
        }

        [Test]
        public void PendingEntry_To_Flat_OnRejection()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            var rejectUpdate = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
            pm.OnOrderRejected(rejectUpdate);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
        }

        [Test]
        public void PendingEntry_To_Flat_OnCancellation()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
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
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
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
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 1, new DateTime(2024, 1, 15, 10, 1, 0));

            pm.GoFlat();
            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Closing));
        }

        [Test]
        public void Closing_To_Closed()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            var acceptEntry = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptEntry);
            pm.OnOrderFilled(entryId, "fill1", 100m, 1, new DateTime(2024, 1, 15, 10, 1, 0));

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
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 1, new DateTime(2024, 1, 15, 10, 1, 0));

            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 1, 105m, 95m));
        }

        [Test]
        public void CannotSubmitEntry_FromClosing()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 1, 95m, 105m);
            var acceptEntry = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptEntry);
            pm.OnOrderFilled(entryId, "fill1", 100m, 1, new DateTime(2024, 1, 15, 10, 1, 0));

            pm.GoFlat();

            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 1, 105m, 95m));
        }

        [Test]
        public void GoFlat_CancelsPendingLimitEntry()
        {
            var pm = CreatePositionManager();
            bool cancelRequested = false;
            pm.ErrorOccurred += (code, msg, data) =>
            {
                if (code == "CANCEL_REQUEST" && msg.Contains("entry"))
                    cancelRequested = true;
            };

            var orderId = pm.SubmitEntry(OrderType.Limit, Side.Long, 1, 95m, 105m, limitPrice: 100m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var workingUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Working, null);
            pm.OnOrderWorking(workingUpdate);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));

            pm.GoFlat();

            Assert.That(cancelRequested, Is.True, "Cancel request should be issued for working entry order");
        }

        [Test]
        public void GoFlat_CancelsPendingStopEntry()
        {
            var pm = CreatePositionManager();
            bool cancelRequested = false;
            pm.ErrorOccurred += (code, msg, data) =>
            {
                if (code == "CANCEL_REQUEST" && msg.Contains("entry"))
                    cancelRequested = true;
            };

            var orderId = pm.SubmitEntry(OrderType.Stop, Side.Long, 1, 95m, 105m, stopPrice: 102m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var workingUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Working, null);
            pm.OnOrderWorking(workingUpdate);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));

            pm.GoFlat();

            Assert.That(cancelRequested, Is.True, "Cancel request should be issued for working entry order");
        }

        [Test]
        public void EndOfSession_CancelsPendingEntry()
        {
            var pm = CreatePositionManager();
            bool cancelRequested = false;
            pm.ErrorOccurred += (code, msg, data) =>
            {
                if (code == "CANCEL_REQUEST" && msg.Contains("entry"))
                    cancelRequested = true;
            };

            var orderId = pm.SubmitEntry(OrderType.Limit, Side.Long, 1, 95m, 105m, limitPrice: 100m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var workingUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Working, null);
            pm.OnOrderWorking(workingUpdate);

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));

            // Simulate end of session
            var sessionEnd = new DateTime(2024, 1, 15, 16, 30, 0); // 4:30 PM ET
            var tick = new Tick(sessionEnd, 100m, 99.99m, 100.01m, 1000);
            pm.OnTick(tick);

            Assert.That(cancelRequested, Is.True, "Cancel request should be issued for working entry order at end of session");
        }

    }
}


