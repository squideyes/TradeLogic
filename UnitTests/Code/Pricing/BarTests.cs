using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class BarTests
    {
        [Test]
        public void Constructor_ValidInputs_CreatesBar()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 105m, 95m, 102m, 1000);

            Assert.That(bar.OpenET, Is.EqualTo(openET));
            Assert.That(bar.Open, Is.EqualTo(100m));
            Assert.That(bar.High, Is.EqualTo(105m));
            Assert.That(bar.Low, Is.EqualTo(95m));
            Assert.That(bar.Close, Is.EqualTo(102m));
            Assert.That(bar.Volume, Is.EqualTo(1000));
        }

        [Test]
        public void Constructor_InvalidTimeKind_ThrowsException()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0, DateTimeKind.Utc);
            Assert.Throws<ArgumentException>(() => new Bar(openET, 100m, 105m, 95m, 102m, 1000));
        }

        [Test]
        public void Constructor_ZeroVolume_CreatesBar()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 105m, 95m, 102m, 0);
            Assert.That(bar.Volume, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_NegativeVolume_CreatesBar()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 105m, 95m, 102m, -100);
            Assert.That(bar.Volume, Is.EqualTo(-100));
        }

        [Test]
        public void Constructor_HighLowEqual_CreatesBar()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 100m, 100m, 100m, 1000);
            Assert.That(bar.High, Is.EqualTo(bar.Low));
        }

        [Test]
        public void Constructor_AllPricesEqual_CreatesBar()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 100m, 100m, 100m, 1000);
            Assert.That(bar.Open, Is.EqualTo(bar.High).And.EqualTo(bar.Low).And.EqualTo(bar.Close));
        }

        [Test]
        public void Constructor_LargeVolume_CreatesBar()
        {
            var openET = new DateTime(2024, 1, 15, 9, 30, 0);
            var bar = new Bar(openET, 100m, 105m, 95m, 102m, long.MaxValue);
            Assert.That(bar.Volume, Is.EqualTo(long.MaxValue));
        }
    }
}

