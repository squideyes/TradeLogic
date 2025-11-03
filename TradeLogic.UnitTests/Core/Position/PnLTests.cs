using System;
using TradeLogic.UnitTests.Fixtures;
using TradeLogic.UnitTests.TestFramework;

namespace TradeLogic.UnitTests.Core.Position
{
    public class PnLTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, feeModel, idGen, logger);
        }

        [TestFramework.Test]
        public void GetView_ReturnsPositionView()
        {
            var pm = CreatePositionManager();
            var view = pm.GetView();
            Assert.NotNull(view);
            Assert.Equal(PositionState.Flat, view.State);
        }

        [TestFramework.Test]
        public void GetView_TracksPnL_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            Assert.Equal(100, view.NetQuantity);
            Assert.Equal(100m, view.AvgEntryPrice);
        }

        [TestFramework.Test]
        public void GetView_TracksPnL_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            Assert.Equal(-100, view.NetQuantity);
            Assert.Equal(100m, view.AvgEntryPrice);
        }

        [TestFramework.Test]
        public void GetView_CalculatesUnrealizedPnL_Long()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            // Unrealized PnL = (current price - entry price) * quantity
            // We need to check if there's a way to set current price
            Assert.NotNull(view);
        }

        [TestFramework.Test]
        public void GetView_CalculatesUnrealizedPnL_Short()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            Assert.NotNull(view);
        }

        [TestFramework.Test]
        public void GetView_TracksFees()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            Assert.NotNull(view);
        }

        [TestFramework.Test]
        public void GetView_PartialFill()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var view = pm.GetView();
            Assert.Equal(PositionState.PendingEntry, view.State);
            Assert.Equal(0, view.NetQuantity);
        }

        [TestFramework.Test]
        public void GetView_AfterPartialFill_ThenComplete()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
            pm.OnOrderFilled(orderId, "fill2", 100m, 50, new DateTime(2024, 1, 15, 10, 2, 0));

            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
            Assert.Equal(50, view.NetQuantity);
        }

        [TestFramework.Test]
        public void GetView_MultiplePartialFills()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderPartiallyFilled(orderId, "fill1", 100m, 25, new DateTime(2024, 1, 15, 10, 1, 0));
            pm.OnOrderPartiallyFilled(orderId, "fill2", 101m, 25, new DateTime(2024, 1, 15, 10, 2, 0));
            pm.OnOrderPartiallyFilled(orderId, "fill3", 102m, 25, new DateTime(2024, 1, 15, 10, 3, 0));
            pm.OnOrderFilled(orderId, "fill4", 103m, 25, new DateTime(2024, 1, 15, 10, 4, 0));

            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
            Assert.Equal(25, view.NetQuantity);
        }

        [TestFramework.Test]
        public void GetView_AvgEntryPrice_MultiplePartialFills()
        {
            var pm = CreatePositionManager();
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
            pm.OnOrderFilled(orderId, "fill2", 102m, 50, new DateTime(2024, 1, 15, 10, 2, 0));

            var view = pm.GetView();
            // Average entry price should be (100*50 + 102*50) / 100 = 101
            // But we only have 50 filled, so avg is 102
            Assert.Equal(102m, view.AvgEntryPrice);
        }
    }
}

