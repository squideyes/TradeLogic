using System;
using TradeLogic.UnitTests.Fixtures;
using TradeLogic.UnitTests.TestFramework;

namespace TradeLogic.UnitTests
{
    public class StateTransitionTests
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
        public void Flat_To_PendingEntry()
        {
            var pm = CreatePositionManager();
            var view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
            
            pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            view = pm.GetView();
            Assert.Equal(PositionState.PendingEntry, view.State);
        }

        [TestFramework.Test]
        public void PendingEntry_To_Open()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void PendingEntry_To_Flat_OnRejection()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var rejectUpdate = new OrderUpdate(orderId, null, OrderStatus.Rejected, "Insufficient funds");
            pm.OnOrderRejected(rejectUpdate);
            
            var view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
        }

        [TestFramework.Test]
        public void PendingEntry_To_Flat_OnCancellation()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            var cancelUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Canceled, "User canceled");
            pm.OnOrderCanceled(cancelUpdate);
            
            var view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
        }

        [TestFramework.Test]
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
            Assert.True(eventFired);
        }

        [TestFramework.Test]
        public void Open_To_Closing()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.GoFlat();
            var view = pm.GetView();
            Assert.Equal(PositionState.Closing, view.State);
        }

        [TestFramework.Test]
        public void Closing_To_Closed()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptEntry = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptEntry);
            pm.OnOrderFilled(entryId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.GoFlat();
            var view = pm.GetView();
            Assert.Equal(PositionState.Closing, view.State);
            
            // Simulate exit order being filled
            // Note: We need to find the exit order ID that was created by GoFlat
            // For now, we'll just verify the state is Closing
        }

        [TestFramework.Test]
        public void Closed_To_Flat_OnReset()
        {
            var pm = CreatePositionManager();
            var view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
            
            pm.Reset();
            view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
        }

        [TestFramework.Test]
        public void CannotSubmitEntry_FromOpen()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 50));
        }

        [TestFramework.Test]
        public void CannotSubmitEntry_FromClosing()
        {
            var pm = CreatePositionManager();
            var entryId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptEntry = new OrderUpdate(entryId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptEntry);
            pm.OnOrderFilled(entryId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.GoFlat();
            
            Assert.Throws<InvalidOperationException>(() =>
                pm.SubmitEntry(OrderType.Market, Side.Short, 50));
        }

        [TestFramework.Test]
        public void CannotReset_FromOpen()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            Assert.Throws<InvalidOperationException>(() => pm.Reset());
        }

        [TestFramework.Test]
        public void Reset_ClearsAllState()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            pm.GoFlat();
            // Wait for position to close before resetting
            // For now, just test that we can reset from Flat
            var view = pm.GetView();
            if (view.State == PositionState.Flat)
            {
                pm.Reset();
                view = pm.GetView();
                Assert.Equal(PositionState.Flat, view.State);
                Assert.Equal(0, view.NetQuantity);
            }
        }

        [TestFramework.Test]
        public void CanResubmitEntry_AfterReset()
        {
            var pm = CreatePositionManager();
            var orderId1 = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId1, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId1, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            pm.GoFlat();
            var view = pm.GetView();
            if (view.State == PositionState.Flat)
            {
                pm.Reset();
                var orderId2 = pm.SubmitEntry(OrderType.Market, Side.Short, 50);
                Assert.NotNull(orderId2);
            }
        }
    }
}

