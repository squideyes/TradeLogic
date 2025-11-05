using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class OrderSnapshotTests
    {
        private OrderSpec CreateOrderSpec()
        {
            return new OrderSpec(
                "ORDER-001", Side.Long, OrderType.Market, 1,
                TimeInForce.FOK, null, null, null, true, false, null);
        }

        [Test]
        public void Constructor_WithValidData_CreatesSnapshotSuccessfully()
        {
            var spec = CreateOrderSpec();
            var snapshot = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            
            Assert.That(snapshot.Spec, Is.EqualTo(spec));
            Assert.That(snapshot.Status, Is.EqualTo(OrderStatus.New));
            Assert.That(snapshot.FilledQuantity, Is.EqualTo(0));
            Assert.That(snapshot.AvgFillPrice, Is.Null);
            Assert.That(snapshot.RejectOrCancelReason, Is.Null);
        }

        [Test]
        public void Constructor_WithFilledQuantity_CreatesSnapshotSuccessfully()
        {
            var spec = CreateOrderSpec();
            var snapshot = new OrderSnapshot(spec, OrderStatus.PartiallyFilled, 5, 150m, null);
            
            Assert.That(snapshot.FilledQuantity, Is.EqualTo(5));
            Assert.That(snapshot.AvgFillPrice, Is.EqualTo(150m));
        }

        [Test]
        public void Constructor_WithReason_CreatesSnapshotSuccessfully()
        {
            var spec = CreateOrderSpec();
            var snapshot = new OrderSnapshot(spec, OrderStatus.Rejected, 0, null, "Insufficient funds");
            
            Assert.That(snapshot.RejectOrCancelReason, Is.EqualTo("Insufficient funds"));
        }

        [Test]
        public void With_UpdatesStatus()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.Accepted);
            
            Assert.That(snapshot1.Status, Is.EqualTo(OrderStatus.New));
            Assert.That(snapshot2.Status, Is.EqualTo(OrderStatus.Accepted));
            Assert.That(snapshot2.Spec, Is.EqualTo(spec));
        }

        [Test]
        public void With_UpdatesFilledQuantity()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.PartiallyFilled, filledQty: 5);
            
            Assert.That(snapshot1.FilledQuantity, Is.EqualTo(0));
            Assert.That(snapshot2.FilledQuantity, Is.EqualTo(5));
        }

        [Test]
        public void With_UpdatesAvgFillPrice()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.PartiallyFilled, avgFillPrice: 150m);
            
            Assert.That(snapshot1.AvgFillPrice, Is.Null);
            Assert.That(snapshot2.AvgFillPrice, Is.EqualTo(150m));
        }

        [Test]
        public void With_UpdatesReason()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.Rejected, reason: "Market closed");
            
            Assert.That(snapshot1.RejectOrCancelReason, Is.Null);
            Assert.That(snapshot2.RejectOrCancelReason, Is.EqualTo("Market closed"));
        }

        [Test]
        public void With_PreservesUnchangedFields()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.Accepted);
            
            Assert.That(snapshot2.Spec, Is.EqualTo(snapshot1.Spec));
            Assert.That(snapshot2.FilledQuantity, Is.EqualTo(snapshot1.FilledQuantity));
            Assert.That(snapshot2.AvgFillPrice, Is.EqualTo(snapshot1.AvgFillPrice));
            Assert.That(snapshot2.RejectOrCancelReason, Is.EqualTo(snapshot1.RejectOrCancelReason));
        }

        [Test]
        public void With_MultipleUpdates_CreatesCorrectSnapshot()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.Accepted);
            var snapshot3 = snapshot2.With(OrderStatus.PartiallyFilled, filledQty: 5, avgFillPrice: 150m);
            var snapshot4 = snapshot3.With(OrderStatus.Filled, filledQty: 10, avgFillPrice: 150.5m);
            
            Assert.That(snapshot4.Status, Is.EqualTo(OrderStatus.Filled));
            Assert.That(snapshot4.FilledQuantity, Is.EqualTo(10));
            Assert.That(snapshot4.AvgFillPrice, Is.EqualTo(150.5m));
        }

        [Test]
        public void With_WithNullFilledQty_PreservesOriginal()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 5, null, null);
            var snapshot2 = snapshot1.With(OrderStatus.Accepted, filledQty: null);
            
            Assert.That(snapshot2.FilledQuantity, Is.EqualTo(5));
        }

        [Test]
        public void With_WithNullAvgFillPrice_PreservesOriginal()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, 150m, null);
            var snapshot2 = snapshot1.With(OrderStatus.Accepted, avgFillPrice: null);
            
            Assert.That(snapshot2.AvgFillPrice, Is.EqualTo(150m));
        }

        [Test]
        public void With_WithNullReason_PreservesOriginal()
        {
            var spec = CreateOrderSpec();
            var snapshot1 = new OrderSnapshot(spec, OrderStatus.New, 0, null, "Original reason");
            var snapshot2 = snapshot1.With(OrderStatus.Accepted, reason: null);
            
            Assert.That(snapshot2.RejectOrCancelReason, Is.EqualTo("Original reason"));
        }

        [Test]
        public void Properties_AreReadOnly()
        {
            var spec = CreateOrderSpec();
            var snapshot = new OrderSnapshot(spec, OrderStatus.New, 0, null, null);
            
            Assert.That(snapshot.Spec, Is.Not.Null);
            Assert.That(snapshot.Status, Is.Not.Null);
            Assert.That(snapshot.FilledQuantity, Is.Not.Null);
        }
    }
}

