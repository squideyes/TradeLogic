using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class FillTests
    {
        [Test]
        public void Constructor_WithValidData_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, 1, 2.5m, et);
            
            Assert.That(fill.OrderIdOrClientOrderId, Is.EqualTo("ORDER-001"));
            Assert.That(fill.FillId, Is.EqualTo("FILL-001"));
            Assert.That(fill.Price, Is.EqualTo(150m));
            Assert.That(fill.Quantity, Is.EqualTo(1));
            Assert.That(fill.Commission, Is.EqualTo(2.5m));
            Assert.That(fill.ETTime, Is.EqualTo(et));
        }

        [Test]
        public void Constructor_WithZeroCommission_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, 1, 0m, et);
            
            Assert.That(fill.Commission, Is.EqualTo(0m));
        }

        [Test]
        public void Constructor_WithNegativeCommission_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, 1, -2.5m, et);
            
            Assert.That(fill.Commission, Is.EqualTo(-2.5m));
        }

        [Test]
        public void Constructor_WithZeroQuantity_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, 0, 2.5m, et);
            
            Assert.That(fill.Quantity, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithNegativeQuantity_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, -1, 2.5m, et);
            
            Assert.That(fill.Quantity, Is.EqualTo(-1));
        }

        [Test]
        public void Constructor_WithZeroPrice_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 0m, 1, 2.5m, et);
            
            Assert.That(fill.Price, Is.EqualTo(0m));
        }

        [Test]
        public void Constructor_WithNullOrderId_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill(null, "FILL-001", 150m, 1, 2.5m, et);
            
            Assert.That(fill.OrderIdOrClientOrderId, Is.Null);
        }

        [Test]
        public void Constructor_WithNullFillId_CreatesFillSuccessfully()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", null, 150m, 1, 2.5m, et);
            
            Assert.That(fill.FillId, Is.Null);
        }

        [Test]
        public void WithCommission_CreatesNewFillWithUpdatedCommission()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill1 = new Fill("ORDER-001", "FILL-001", 150m, 1, 2.5m, et);
            var fill2 = fill1.WithCommission(5m);
            
            Assert.That(fill1.Commission, Is.EqualTo(2.5m));
            Assert.That(fill2.Commission, Is.EqualTo(5m));
            Assert.That(fill2.OrderIdOrClientOrderId, Is.EqualTo(fill1.OrderIdOrClientOrderId));
            Assert.That(fill2.FillId, Is.EqualTo(fill1.FillId));
            Assert.That(fill2.Price, Is.EqualTo(fill1.Price));
            Assert.That(fill2.Quantity, Is.EqualTo(fill1.Quantity));
            Assert.That(fill2.ETTime, Is.EqualTo(fill1.ETTime));
        }

        [Test]
        public void WithCommission_WithZeroCommission_CreatesNewFill()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill1 = new Fill("ORDER-001", "FILL-001", 150m, 1, 2.5m, et);
            var fill2 = fill1.WithCommission(0m);
            
            Assert.That(fill2.Commission, Is.EqualTo(0m));
        }

        [Test]
        public void WithCommission_WithNegativeCommission_CreatesNewFill()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill1 = new Fill("ORDER-001", "FILL-001", 150m, 1, 2.5m, et);
            var fill2 = fill1.WithCommission(-5m);
            
            Assert.That(fill2.Commission, Is.EqualTo(-5m));
        }

        [Test]
        public void WithCommission_DoesNotModifyOriginalFill()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill1 = new Fill("ORDER-001", "FILL-001", 150m, 1, 2.5m, et);
            var fill2 = fill1.WithCommission(5m);
            
            Assert.That(fill1.Commission, Is.EqualTo(2.5m));
            Assert.That(fill2.Commission, Is.EqualTo(5m));
        }

        [Test]
        public void Properties_AreReadOnly()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, 1, 2.5m, et);
            
            // Verify properties are accessible
            Assert.That(fill.OrderIdOrClientOrderId, Is.Not.Null);
            Assert.That(fill.FillId, Is.Not.Null);
            Assert.That(fill.Price, Is.Not.Null);
            Assert.That(fill.Quantity, Is.Not.Null);
            Assert.That(fill.Commission, Is.Not.Null);
            Assert.That(fill.ETTime, Is.Not.Null);
        }

        [Test]
        public void FillCost_CanBeCalculated()
        {
            var et = new DateTime(2024, 1, 15, 10, 30, 0);
            var fill = new Fill("ORDER-001", "FILL-001", 150m, 10, 2.5m, et);
            
            var cost = fill.Price * fill.Quantity + fill.Commission;
            Assert.That(cost, Is.EqualTo(1502.5m));
        }
    }
}

