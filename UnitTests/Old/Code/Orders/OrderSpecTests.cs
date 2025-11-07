//using System;
//using NUnit.Framework;
//using TradeLogic;

//namespace TradeLogic.UnitTests
//{
//    [TestFixture]
//    public class OrderSpecTests
//    {
//        [Test]
//        public void Constructor_WithValidData_CreatesSpecSuccessfully()
//        {
//            var gtt = new DateTime(2024, 1, 15, 16, 30, 0);
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, gtt, true, false, null);
            
//            Assert.That(spec.ClientOrderId, Is.EqualTo("ORDER-001"));
//            Assert.That(spec.Side, Is.EqualTo(Side.Long));
//            Assert.That(spec.OrderType, Is.EqualTo(OrderType.Market));
//            Assert.That(spec.Quantity, Is.EqualTo(1));
//            Assert.That(spec.TimeInForce, Is.EqualTo(TimeInForce.FOK));
//            Assert.That(spec.LimitPrice, Is.Null);
//            Assert.That(spec.StopPrice, Is.Null);
//            Assert.That(spec.GoodTillTimeUtc, Is.EqualTo(gtt));
//            Assert.That(spec.IsEntry, Is.True);
//            Assert.That(spec.IsExit, Is.False);
//            Assert.That(spec.OcoGroupId, Is.Null);
//        }

//        [Test]
//        public void Constructor_WithLimitPrice_CreatesSpecSuccessfully()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Limit, 1,
//                TimeInForce.GTD, 150m, null, null, true, false, null);
            
//            Assert.That(spec.LimitPrice, Is.EqualTo(150m));
//            Assert.That(spec.StopPrice, Is.Null);
//        }

//        [Test]
//        public void Constructor_WithStopPrice_CreatesSpecSuccessfully()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Short, OrderType.Stop, 1,
//                TimeInForce.GTD, null, 145m, null, false, true, null);
            
//            Assert.That(spec.LimitPrice, Is.Null);
//            Assert.That(spec.StopPrice, Is.EqualTo(145m));
//        }

//        [Test]
//        public void Constructor_WithStopLimitPrices_CreatesSpecSuccessfully()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.StopLimit, 1,
//                TimeInForce.GTD, 150m, 145m, null, false, true, null);
            
//            Assert.That(spec.LimitPrice, Is.EqualTo(150m));
//            Assert.That(spec.StopPrice, Is.EqualTo(145m));
//        }

//        [Test]
//        public void Constructor_WithOcoGroupId_CreatesSpecSuccessfully()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, null, false, true, "OCO-GROUP-001");
            
//            Assert.That(spec.OcoGroupId, Is.EqualTo("OCO-GROUP-001"));
//        }

//        [Test]
//        public void Constructor_WithZeroQuantity_CreatesSpecSuccessfully()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 0,
//                TimeInForce.FOK, null, null, null, true, false, null);
            
//            Assert.That(spec.Quantity, Is.EqualTo(0));
//        }

//        [Test]
//        public void Constructor_WithNegativeQuantity_CreatesSpecSuccessfully()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, -1,
//                TimeInForce.FOK, null, null, null, true, false, null);
            
//            Assert.That(spec.Quantity, Is.EqualTo(-1));
//        }

//        [Test]
//        public void WithVenueOrderId_UpdatesVenueOrderId()
//        {
//            var spec1 = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, null, true, false, null);
            
//            var spec2 = spec1.WithVenueOrderId("VENUE-001");
            
//            Assert.That(spec1.VenueOrderId, Is.Null);
//            Assert.That(spec2.VenueOrderId, Is.EqualTo("VENUE-001"));
//        }

//        [Test]
//        public void WithVenueOrderId_PreservesOtherFields()
//        {
//            var spec1 = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, null, true, false, null);
            
//            var spec2 = spec1.WithVenueOrderId("VENUE-001");
            
//            Assert.That(spec2.ClientOrderId, Is.EqualTo(spec1.ClientOrderId));
//            Assert.That(spec2.Side, Is.EqualTo(spec1.Side));
//            Assert.That(spec2.OrderType, Is.EqualTo(spec1.OrderType));
//            Assert.That(spec2.Quantity, Is.EqualTo(spec1.Quantity));
//        }

//        [Test]
//        public void Clone_CreatesIdenticalCopy()
//        {
//            var spec1 = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Limit, 10,
//                TimeInForce.GTD, 150m, null, null, true, false, "OCO-001");
            
//            var spec2 = spec1.Clone();
            
//            Assert.That(spec2.ClientOrderId, Is.EqualTo(spec1.ClientOrderId));
//            Assert.That(spec2.Side, Is.EqualTo(spec1.Side));
//            Assert.That(spec2.OrderType, Is.EqualTo(spec1.OrderType));
//            Assert.That(spec2.Quantity, Is.EqualTo(spec1.Quantity));
//            Assert.That(spec2.TimeInForce, Is.EqualTo(spec1.TimeInForce));
//            Assert.That(spec2.LimitPrice, Is.EqualTo(spec1.LimitPrice));
//            Assert.That(spec2.StopPrice, Is.EqualTo(spec1.StopPrice));
//            Assert.That(spec2.IsEntry, Is.EqualTo(spec1.IsEntry));
//            Assert.That(spec2.IsExit, Is.EqualTo(spec1.IsExit));
//            Assert.That(spec2.OcoGroupId, Is.EqualTo(spec1.OcoGroupId));
//        }

//        [Test]
//        public void Clone_WithVenueOrderId_CopiesVenueOrderId()
//        {
//            var spec1 = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, null, true, false, null);
//            spec1 = spec1.WithVenueOrderId("VENUE-001");
            
//            var spec2 = spec1.Clone();
            
//            Assert.That(spec2.VenueOrderId, Is.EqualTo("VENUE-001"));
//        }

//        [Test]
//        public void Clone_CreatesIndependentCopy()
//        {
//            var spec1 = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, null, true, false, null);
            
//            var spec2 = spec1.Clone();
//            var spec3 = spec2.WithVenueOrderId("VENUE-001");
            
//            Assert.That(spec1.VenueOrderId, Is.Null);
//            Assert.That(spec2.VenueOrderId, Is.Null);
//            Assert.That(spec3.VenueOrderId, Is.EqualTo("VENUE-001"));
//        }

//        [Test]
//        public void Properties_AreReadOnly()
//        {
//            var spec = new OrderSpec(
//                "ORDER-001", Side.Long, OrderType.Market, 1,
//                TimeInForce.FOK, null, null, null, true, false, null);
            
//            Assert.That(spec.ClientOrderId, Is.Not.Null);
//            Assert.That(spec.Side, Is.Not.Null);
//            Assert.That(spec.OrderType, Is.Not.Null);
//            Assert.That(spec.Quantity, Is.Not.Null);
//        }

//        [Test]
//        public void Constructor_WithAllOrderTypes_CreatesSpecSuccessfully()
//        {
//            var market = new OrderSpec("O1", Side.Long, OrderType.Market, 1, TimeInForce.FOK, null, null, null, true, false, null);
//            var limit = new OrderSpec("O2", Side.Long, OrderType.Limit, 1, TimeInForce.GTD, 150m, null, null, true, false, null);
//            var stop = new OrderSpec("O3", Side.Long, OrderType.Stop, 1, TimeInForce.GTD, null, 145m, null, true, false, null);
//            var stopLimit = new OrderSpec("O4", Side.Long, OrderType.StopLimit, 1, TimeInForce.GTD, 150m, 145m, null, true, false, null);
            
//            Assert.That(market.OrderType, Is.EqualTo(OrderType.Market));
//            Assert.That(limit.OrderType, Is.EqualTo(OrderType.Limit));
//            Assert.That(stop.OrderType, Is.EqualTo(OrderType.Stop));
//            Assert.That(stopLimit.OrderType, Is.EqualTo(OrderType.StopLimit));
//        }
//    }
//}

