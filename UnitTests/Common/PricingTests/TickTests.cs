using NUnit.Framework;
using System;
using WickScalper.Common;
using Assert = TradeLogic.UnitTests.TestFramework.AssertEx;
using Symbol = WickScalper.Common.Symbol;

namespace WickScalper.UnitTests.Common.PricingTests
{
    [TestFixture]
    public class TickTests
    {
        private DateTime testTime;

        [SetUp]
        public void Setup()
        {
            testTime = new DateTime(2024, 1, 2, 9, 30, 0);
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesTickCorrectly()
        {
            var tick = new Tick(testTime, 4500.00m, 4499.75m, 4500.25m, 100);

            Assert.AreEqual(testTime, tick.OnET);
            Assert.AreEqual(4500.00m, tick.Last);
            Assert.AreEqual(4499.75m, tick.Bid);
            Assert.AreEqual(4500.25m, tick.Ask);
            Assert.AreEqual(100, tick.Volume);
        }

        [Test]
        public void Properties_AreReadOnly()
        {
            var tick = new Tick(testTime, 4500.00m, 4499.75m, 4500.25m, 100);

            Assert.AreEqual(testTime, tick.OnET);
            Assert.AreEqual(4500.00m, tick.Last);
            Assert.AreEqual(4499.75m, tick.Bid);
            Assert.AreEqual(4500.25m, tick.Ask);
            Assert.AreEqual(100, tick.Volume);
        }

        [Test]
        public void Constructor_WithDifferentValues()
        {
            var tick = new Tick(testTime, 85.50m, 85.49m, 85.51m, 500);

            Assert.AreEqual(85.50m, tick.Last);
            Assert.AreEqual(85.49m, tick.Bid);
            Assert.AreEqual(85.51m, tick.Ask);
            Assert.AreEqual(500, tick.Volume);
        }

        [Test]
        public void Constructor_WithZeroVolume()
        {
            var tick = new Tick(testTime, 4500.00m, 4499.75m, 4500.25m, 0);

            Assert.AreEqual(0, tick.Volume);
        }

        [Test]
        public void Constructor_WithNegativeValues()
        {
            var tick = new Tick(testTime, -4500.00m, -4500.25m, -4499.75m, 100);

            Assert.AreEqual(-4500.00m, tick.Last);
            Assert.AreEqual(-4500.25m, tick.Bid);
            Assert.AreEqual(-4499.75m, tick.Ask);
        }

        [Test]
        public void Constructor_WithLargeVolume()
        {
            var tick = new Tick(testTime, 4500.00m, 4499.75m, 4500.25m, int.MaxValue);

            Assert.AreEqual(int.MaxValue, tick.Volume);
        }

        [Test]
        public void Parse_WithValidCSVLine_CreatesTickCorrectly()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            // CSV format: Date,Time,Last,Bid,Ask,Volume
            var csvLine = "20240102 093000000,4500.00,4499.75,4500.25,100";

            var tick = Tick.Parse(future, session, csvLine);

            Assert.AreEqual(4500.00m, tick.Last);
            Assert.AreEqual(4499.75m, tick.Bid);
            Assert.AreEqual(4500.25m, tick.Ask);
            Assert.AreEqual(100, tick.Volume);
        }

        [Test]
        public void Parse_WithDifferentPrices_CreatesTickCorrectly()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.CL);
            
            var csvLine = "20240102 093000000,85.50,85.49,85.51,500";

            var tick = Tick.Parse(future, session, csvLine);

            Assert.AreEqual(85.50m, tick.Last);
            Assert.AreEqual(85.49m, tick.Bid);
            Assert.AreEqual(85.51m, tick.Ask);
            Assert.AreEqual(500, tick.Volume);
        }

        [Test]
        public void Parse_WithMilliseconds_ParsesCorrectly()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            var csvLine = "20240102 093000123,4500.00,4499.75,4500.25,100";

            var tick = Tick.Parse(future, session, csvLine);

            Assert.AreEqual(123, tick.OnET.Millisecond);
        }

        [Test]
        public void Parse_OutOfSessionTime_Throws()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            // Time before session starts (6:30 AM ET)
            var csvLine = "20240102 050000000,4500.00,4499.75,4500.25,100";

            Assert.Throws<GuardException>(() => Tick.Parse(future, session, csvLine));
        }

        [Test]
        public void Parse_InvalidBidPrice_Throws()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            // Bid price not divisible by tick size
            var csvLine = "20240102 093000000,4500.00,4499.10,4500.25,100";

            Assert.Throws<GuardException>(() => Tick.Parse(future, session, csvLine));
        }

        [Test]
        public void Parse_AskLessThanBid_Throws()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            var csvLine = "20240102 093000000,4500.00,4500.25,4499.75,100";

            Assert.Throws<GuardException>(() => Tick.Parse(future, session, csvLine));
        }

        [Test]
        public void Parse_LastOutsideBidAsk_Throws()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            var csvLine = "20240102 093000000,4501.00,4499.75,4500.25,100";

            Assert.Throws<GuardException>(() => Tick.Parse(future, session, csvLine));
        }

        [Test]
        public void Parse_ZeroVolume_Throws()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            var session = new Session(tradeDate);
            var future = KnownFutures.GetFuture(Symbol.ES);
            
            var csvLine = "20240102 093000000,4500.00,4499.75,4500.25,0";

            Assert.Throws<GuardException>(() => Tick.Parse(future, session, csvLine));
        }
    }
}

