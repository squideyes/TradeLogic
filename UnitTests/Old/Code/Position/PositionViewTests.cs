//using System;
//using NUnit.Framework;
//using TradeLogic;

//namespace TradeLogic.UnitTests
//{
//    [TestFixture]
//    public class PositionViewTests
//    {
//        [Test]
//        public void Constructor_WithFlatPosition_CreatesViewSuccessfully()
//        {
//            var view = new PositionView(
//                PositionState.Flat, null, 0, 0m, 0m, 0m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.State, Is.EqualTo(PositionState.Flat));
//            Assert.That(view.Side, Is.Null);
//            Assert.That(view.NetQuantity, Is.EqualTo(0));
//            Assert.That(view.AvgEntryPrice, Is.EqualTo(0m));
//            Assert.That(view.RealizedPnl, Is.EqualTo(0m));
//            Assert.That(view.UnrealizedPnl, Is.EqualTo(0m));
//            Assert.That(view.OpenET, Is.Null);
//            Assert.That(view.ClosedET, Is.Null);
//            Assert.That(view.Symbol, Is.EqualTo(Symbol.ES));
//            Assert.That(view.StopLossPrice, Is.Null);
//            Assert.That(view.TakeProfitPrice, Is.Null);
//        }

//        [Test]
//        public void Constructor_WithOpenLongPosition_CreatesViewSuccessfully()
//        {
//            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
//            var view = new PositionView(
//                PositionState.Open, Side.Long, 10, 150m, 0m, 500m,
//                openTime, null, Symbol.ES, 145m, 160m);
            
//            Assert.That(view.State, Is.EqualTo(PositionState.Open));
//            Assert.That(view.Side, Is.EqualTo(Side.Long));
//            Assert.That(view.NetQuantity, Is.EqualTo(10));
//            Assert.That(view.AvgEntryPrice, Is.EqualTo(150m));
//            Assert.That(view.RealizedPnl, Is.EqualTo(0m));
//            Assert.That(view.UnrealizedPnl, Is.EqualTo(500m));
//            Assert.That(view.OpenET, Is.EqualTo(openTime));
//            Assert.That(view.ClosedET, Is.Null);
//            Assert.That(view.StopLossPrice, Is.EqualTo(145m));
//            Assert.That(view.TakeProfitPrice, Is.EqualTo(160m));
//        }

//        [Test]
//        public void Constructor_WithOpenShortPosition_CreatesViewSuccessfully()
//        {
//            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
//            var view = new PositionView(
//                PositionState.Open, Side.Short, -10, 150m, 0m, -500m,
//                openTime, null, Symbol.ES, 155m, 140m);
            
//            Assert.That(view.State, Is.EqualTo(PositionState.Open));
//            Assert.That(view.Side, Is.EqualTo(Side.Short));
//            Assert.That(view.NetQuantity, Is.EqualTo(-10));
//            Assert.That(view.AvgEntryPrice, Is.EqualTo(150m));
//            Assert.That(view.UnrealizedPnl, Is.EqualTo(-500m));
//            Assert.That(view.StopLossPrice, Is.EqualTo(155m));
//            Assert.That(view.TakeProfitPrice, Is.EqualTo(140m));
//        }

//        [Test]
//        public void Constructor_WithClosedPosition_CreatesViewSuccessfully()
//        {
//            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
//            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
//            var view = new PositionView(
//                PositionState.Closed, Side.Long, 0, 150m, 500m, 0m,
//                openTime, closeTime, Symbol.ES, null, null);
            
//            Assert.That(view.State, Is.EqualTo(PositionState.Closed));
//            Assert.That(view.Side, Is.EqualTo(Side.Long));
//            Assert.That(view.NetQuantity, Is.EqualTo(0));
//            Assert.That(view.RealizedPnl, Is.EqualTo(500m));
//            Assert.That(view.UnrealizedPnl, Is.EqualTo(0m));
//            Assert.That(view.OpenET, Is.EqualTo(openTime));
//            Assert.That(view.ClosedET, Is.EqualTo(closeTime));
//        }

//        [Test]
//        public void Constructor_WithPendingEntryState_CreatesViewSuccessfully()
//        {
//            var view = new PositionView(
//                PositionState.PendingEntry, null, 0, 0m, 0m, 0m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.State, Is.EqualTo(PositionState.PendingEntry));
//            Assert.That(view.Side, Is.Null);
//        }

//        [Test]
//        public void Constructor_WithNegativeRealizedPnl_CreatesViewSuccessfully()
//        {
//            var view = new PositionView(
//                PositionState.Closed, Side.Long, 0, 150m, -500m, 0m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.RealizedPnl, Is.EqualTo(-500m));
//        }

//        [Test]
//        public void Constructor_WithNegativeUnrealizedPnl_CreatesViewSuccessfully()
//        {
//            var view = new PositionView(
//                PositionState.Open, Side.Long, 10, 150m, 0m, -500m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.UnrealizedPnl, Is.EqualTo(-500m));
//        }

//        [Test]
//        public void Constructor_WithDifferentSymbols_CreatesViewSuccessfully()
//        {
//            var esView = new PositionView(
//                PositionState.Flat, null, 0, 0m, 0m, 0m,
//                null, null, Symbol.ES, null, null);
            
//            var nqView = new PositionView(
//                PositionState.Flat, null, 0, 0m, 0m, 0m,
//                null, null, Symbol.NQ, null, null);
            
//            Assert.That(esView.Symbol, Is.EqualTo(Symbol.ES));
//            Assert.That(nqView.Symbol, Is.EqualTo(Symbol.NQ));
//        }

//        [Test]
//        public void Properties_AreReadOnly()
//        {
//            var view = new PositionView(
//                PositionState.Flat, null, 0, 0m, 0m, 0m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.State, Is.Not.Null);
//            Assert.That(view.NetQuantity, Is.Not.Null);
//            Assert.That(view.AvgEntryPrice, Is.Not.Null);
//            Assert.That(view.RealizedPnl, Is.Not.Null);
//            Assert.That(view.UnrealizedPnl, Is.Not.Null);
//            Assert.That(view.Symbol, Is.Not.Null);
//        }

//        [Test]
//        public void Constructor_WithLargeQuantities_CreatesViewSuccessfully()
//        {
//            var view = new PositionView(
//                PositionState.Open, Side.Long, 1000, 150m, 0m, 50000m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.NetQuantity, Is.EqualTo(1000));
//        }

//        [Test]
//        public void Constructor_WithLargePrices_CreatesViewSuccessfully()
//        {
//            var view = new PositionView(
//                PositionState.Open, Side.Long, 10, 99999.99m, 0m, 0m,
//                null, null, Symbol.ES, null, null);
            
//            Assert.That(view.AvgEntryPrice, Is.EqualTo(99999.99m));
//        }

//        [Test]
//        public void Constructor_WithAllStates_CreatesViewSuccessfully()
//        {
//            var flat = new PositionView(PositionState.Flat, null, 0, 0m, 0m, 0m, null, null, Symbol.ES, null, null);
//            var pending = new PositionView(PositionState.PendingEntry, null, 0, 0m, 0m, 0m, null, null, Symbol.ES, null, null);
//            var open = new PositionView(PositionState.Open, Side.Long, 1, 150m, 0m, 0m, null, null, Symbol.ES, null, null);
//            var closing = new PositionView(PositionState.Closing, Side.Long, 1, 150m, 0m, 0m, null, null, Symbol.ES, null, null);
//            var closed = new PositionView(PositionState.Closed, Side.Long, 0, 150m, 100m, 0m, null, null, Symbol.ES, null, null);
            
//            Assert.That(flat.State, Is.EqualTo(PositionState.Flat));
//            Assert.That(pending.State, Is.EqualTo(PositionState.PendingEntry));
//            Assert.That(open.State, Is.EqualTo(PositionState.Open));
//            Assert.That(closing.State, Is.EqualTo(PositionState.Closing));
//            Assert.That(closed.State, Is.EqualTo(PositionState.Closed));
//        }
//    }
//}

