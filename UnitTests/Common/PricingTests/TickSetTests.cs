using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using WickScalper.Common;
using Assert = TradeLogic.UnitTests.TestFramework.AssertEx;
using Bar = WickScalper.Common.Bar;
using Tick = WickScalper.Common.Tick;
using Future = WickScalper.Common.Future;
using Symbol = WickScalper.Common.Symbol;
using TickSet = WickScalper.Common.TickSet;
using TickSetParser = WickScalper.Common.TickSetParser;
using GuardException = WickScalper.Common.GuardException;

namespace TradeLogic.UnitTests.Common.PricingTests
{
    [TestFixture]
    public class TickSetTests
    {
        private Session testSession;
        private Future testFuture;
        private DateTime baseTime;

        [SetUp]
        public void Setup()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            testSession = new Session(tradeDate);
            testFuture = KnownFutures.GetFuture(Symbol.ES);
            baseTime = testSession.From;
        }

        [Test]
        public void Constructor_WithSymbol_InitializesCorrectly()
        {
            var tickSet = new TickSet(Symbol.ES, testSession);

            Assert.AreEqual(Symbol.ES, tickSet.Future.Symbol);
            Assert.AreEqual(testSession, tickSet.Session);
            Assert.AreEqual(0, tickSet.Count);
        }

        [Test]
        public void Constructor_WithFuture_InitializesCorrectly()
        {
            var tickSet = new TickSet(testFuture, testSession);

            Assert.AreEqual(testFuture, tickSet.Future);
            Assert.AreEqual(testSession, tickSet.Session);
            Assert.AreEqual(0, tickSet.Count);
        }

        [Test]
        public void Constructor_WithNullFuture_Throws()
        {
            Assert.Throws<GuardException>(() => new TickSet((Future)null, testSession));
        }

        [Test]
        public void Constructor_WithNullSession_Throws()
        {
            Assert.Throws<GuardException>(() => new TickSet(testFuture, null));
        }

        [Test]
        public void Add_SingleTick_IncreasesCount()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var tick = new Tick(baseTime, 4500.00m, 4499.75m, 4500.25m, 100);

            tickSet.Add(tick);

            Assert.AreEqual(1, tickSet.Count);
        }

        [Test]
        public void Add_MultipleTicks_IncreasesCount()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var tick1 = new Tick(baseTime, 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(baseTime.AddSeconds(1), 4500.25m, 4500.00m, 4500.50m, 50);

            tickSet.Add(tick1);
            tickSet.Add(tick2);

            Assert.AreEqual(2, tickSet.Count);
        }

        [Test]
        public void Add_NullTick_Throws()
        {
            var tickSet = new TickSet(testFuture, testSession);

            Assert.Throws<GuardException>(() => tickSet.Add(null));
        }

        [Test]
        public void Add_OutOfOrderTick_Throws()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var tick1 = new Tick(baseTime.AddSeconds(10), 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(baseTime.AddSeconds(5), 4500.25m, 4500.00m, 4500.50m, 50);

            tickSet.Add(tick1);

            Assert.Throws<GuardException>(() => tickSet.Add(tick2));
        }

        [Test]
        public void Add_SameTimeTick_Succeeds()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var tick1 = new Tick(baseTime, 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(baseTime, 4500.25m, 4500.00m, 4500.50m, 50);

            tickSet.Add(tick1);
            tickSet.Add(tick2);

            Assert.AreEqual(2, tickSet.Count);
        }

        [Test]
        public void AddRange_WithMultipleTicks_AddsAll()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var ticks = new List<Tick>
            {
                new Tick(baseTime, 4500.00m, 4499.75m, 4500.25m, 100),
                new Tick(baseTime.AddSeconds(1), 4500.25m, 4500.00m, 4500.50m, 50),
                new Tick(baseTime.AddSeconds(2), 4500.50m, 4500.25m, 4500.75m, 75)
            };

            tickSet.AddRange(ticks);

            Assert.AreEqual(3, tickSet.Count);
        }

        [Test]
        public void AddRange_WithNullCollection_Throws()
        {
            var tickSet = new TickSet(testFuture, testSession);

            Assert.Throws<GuardException>(() => tickSet.AddRange(null));
        }

        [Test]
        public void AddRange_WithOutOfOrderTicks_Throws()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var ticks = new List<Tick>
            {
                new Tick(baseTime, 4500.00m, 4499.75m, 4500.25m, 100),
                new Tick(baseTime.AddSeconds(2), 4500.50m, 4500.25m, 4500.75m, 75),
                new Tick(baseTime.AddSeconds(1), 4500.25m, 4500.00m, 4500.50m, 50)
            };

            Assert.Throws<GuardException>(() => tickSet.AddRange(ticks));
        }

        [Test]
        public void Enumeration_ReturnsAllTicks()
        {
            var tickSet = new TickSet(testFuture, testSession);
            var ticks = new List<Tick>
            {
                new Tick(baseTime, 4500.00m, 4499.75m, 4500.25m, 100),
                new Tick(baseTime.AddSeconds(1), 4500.25m, 4500.00m, 4500.50m, 50),
                new Tick(baseTime.AddSeconds(2), 4500.50m, 4500.25m, 4500.75m, 75)
            };

            tickSet.AddRange(ticks);

            var enumerated = tickSet.ToList();

            Assert.AreEqual(3, enumerated.Count);
            Assert.AreEqual(ticks[0].Last, enumerated[0].Last);
            Assert.AreEqual(ticks[1].Last, enumerated[1].Last);
            Assert.AreEqual(ticks[2].Last, enumerated[2].Last);
        }

        [Test]
        public void Future_Property_IsReadOnly()
        {
            var tickSet = new TickSet(testFuture, testSession);

            Assert.AreEqual(testFuture, tickSet.Future);
        }

        [Test]
        public void Session_Property_IsReadOnly()
        {
            var tickSet = new TickSet(testFuture, testSession);

            Assert.AreEqual(testSession, tickSet.Session);
        }

        [Test]
        public void Count_StartsAtZero()
        {
            var tickSet = new TickSet(testFuture, testSession);

            Assert.AreEqual(0, tickSet.Count);
        }
    }
}

