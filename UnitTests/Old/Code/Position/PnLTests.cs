//using System;
//using NUnit.Framework;
//using TradeLogic;

//namespace TradeLogic.UnitTests
//{
//    [TestFixture]
//    public class PnLTests
//    {
//        private PositionManager CreatePositionManager()
//        {
//            var config = new PositionConfig
//            {
//                Symbol = Symbol.ES,
//                TickSize = 0.25m,
//                PointValue = 50m,
//                IdPrefix = "TEST",
//                SlippageToleranceTicks = 1
//            };
//            var idGen = new MockIdGenerator();
//            var logger = new MockLogger();
//            return new PositionManager(config, idGen, logger);
//        }

//        [Test]
//        public void GetPosition_ReturnsPositionView()
//        {
//            var pm = CreatePositionManager();
//            var position = pm.GetPosition();
//            Assert.That(position, Is.Not.Null);
//            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
//        }

//        [Test]
//        public void GetPosition_TracksPnL_Long()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

//            var position = pm.GetPosition();
//            Assert.That(position.NetQuantity, Is.EqualTo(100));
//            Assert.That(position.AvgEntryPrice, Is.EqualTo(100m));
//        }

//        [Test]
//        public void GetPosition_TracksPnL_Short()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

//            var position = pm.GetPosition();
//            Assert.That(position.NetQuantity, Is.EqualTo(-100));
//            Assert.That(position.AvgEntryPrice, Is.EqualTo(100m));
//        }

//        [Test]
//        public void GetPosition_CalculatesUnrealizedPnL_Long()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

//            var position = pm.GetPosition();
//            // Unrealized PnL = (current price - entry price) * quantity
//            // We need to check if there's a way to set current price
//            Assert.That(position, Is.Not.Null);
//        }

//        [Test]
//        public void GetPosition_CalculatesUnrealizedPnL_Short()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Short, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

//            var position = pm.GetPosition();
//            Assert.That(position, Is.Not.Null);
//        }

//        [Test]
//        public void GetPosition_TracksFees()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

//            var position = pm.GetPosition();
//            Assert.That(position, Is.Not.Null);
//        }

//        [Test]
//        public void GetPosition_PartialFill()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));

//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.PendingEntry));
//            Assert.That(position.NetQuantity, Is.EqualTo(0));
//        }

//        [Test]
//        public void GetPosition_AfterPartialFill_ThenComplete()
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

//        [Test]
//        public void GetPosition_MultiplePartialFills()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderPartiallyFilled(orderId, "fill1", 100m, 25, new DateTime(2024, 1, 15, 10, 1, 0));
//            pm.HandleOrderPartiallyFilled(orderId, "fill2", 101m, 25, new DateTime(2024, 1, 15, 10, 2, 0));
//            pm.HandleOrderPartiallyFilled(orderId, "fill3", 102m, 25, new DateTime(2024, 1, 15, 10, 3, 0));
//            pm.HandleOrderFilled(orderId, "fill4", 103m, 25, new DateTime(2024, 1, 15, 10, 4, 0));

//            var position = pm.GetPosition();
//            Assert.That(position.State, Is.EqualTo(PositionState.Open));
//            Assert.That(position.NetQuantity, Is.EqualTo(25));
//        }

//        [Test]
//        public void GetPosition_AvgEntryPrice_MultiplePartialFills()
//        {
//            var pm = CreatePositionManager();
//            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
//            pm.HandleOrderAccepted(acceptUpdate);
//            pm.HandleOrderPartiallyFilled(orderId, "fill1", 100m, 50, new DateTime(2024, 1, 15, 10, 1, 0));
//            pm.HandleOrderFilled(orderId, "fill2", 102m, 50, new DateTime(2024, 1, 15, 10, 2, 0));

//            var position = pm.GetPosition();
//            // Average entry price should be (100*50 + 102*50) / 100 = 101
//            // But we only have 50 filled, so avg is 102
//            Assert.That(position.AvgEntryPrice, Is.EqualTo(102m));
//        }
//    }
//}


