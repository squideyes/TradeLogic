using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    public class BarFeedTests
    {
        private Session testSession;
        private List<Bar> closedBars;

        [SetUp]
        public void Setup()
        {
            var tradeDate = new DateOnly(2024, 1, 2); // Tuesday
            testSession = new Session(tradeDate);
            closedBars = new List<Bar>();
        }

        [Test]
        public void Constructor_WithValidParameters_Succeeds()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));

            Assert.AreEqual(Symbol.ES, feed.Symbol);
            Assert.AreEqual(testSession, feed.Session);
        }

        [Test]
        public void Constructor_WithInvalidBarSeconds_Throws()
        {
            Assert.Throws<GuardException>(() =>
                new BarFeed(Symbol.ES, testSession, 7, bar => closedBars.Add(bar)));
        }

        [Test]
        public void Constructor_WithBarSecondsNotDivisibleBy5_Throws()
        {
            Assert.Throws<GuardException>(() =>
                new BarFeed(Symbol.ES, testSession, 63, bar => closedBars.Add(bar)));
        }

        [Test]
        public void Constructor_WithNullSession_Throws()
        {
            Assert.Throws<GuardException>(() =>
                new BarFeed(Symbol.ES, null, 60, bar => closedBars.Add(bar)));
        }

        [Test]
        public void Constructor_WithNullCallback_Throws()
        {
            Assert.Throws<GuardException>(() =>
                new BarFeed(Symbol.ES, testSession, 60, null));
        }

        [Test]
        public void HandleTick_FirstTick_CreatesBar()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));
            var tick = new Tick(testSession.From.AddSeconds(10), 4500.00m, 4499.75m, 4500.25m, 100);

            feed.HandleTick(tick);

            Assert.AreEqual(0, closedBars.Count);
        }

        [Test]
        public void HandleTick_SameBar_AdjustsBar()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));
            var tick1 = new Tick(testSession.From.AddSeconds(10), 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(testSession.From.AddSeconds(30), 4505.00m, 4504.75m, 4505.25m, 50);

            feed.HandleTick(tick1);
            feed.HandleTick(tick2);

            Assert.AreEqual(0, closedBars.Count);
        }

        [Test]
        public void HandleTick_NewBar_ClosesPreviousBar()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));
            var tick1 = new Tick(testSession.From.AddSeconds(10), 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(testSession.From.AddSeconds(70), 4505.00m, 4504.75m, 4505.25m, 50);

            feed.HandleTick(tick1);
            feed.HandleTick(tick2);

            Assert.AreEqual(1, closedBars.Count);
            Assert.AreEqual(testSession.From, closedBars[0].OpenET);
        }

        [Test]
        public void HandleTick_MultipleBarTransitions()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));
            
            feed.HandleTick(new Tick(testSession.From.AddSeconds(10), 4500.00m, 4499.75m, 4500.25m, 100));
            feed.HandleTick(new Tick(testSession.From.AddSeconds(70), 4505.00m, 4504.75m, 4505.25m, 50));
            feed.HandleTick(new Tick(testSession.From.AddSeconds(130), 4510.00m, 4509.75m, 4510.25m, 75));

            Assert.AreEqual(2, closedBars.Count);
        }

        [Test]
        public void HandleTick_WithNullTick_Throws()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));

            Assert.Throws<GuardException>(() => feed.HandleTick(null));
        }

        [Test]
        public void HandleTick_CalculatesCorrectBarOpenTime()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 60, bar => closedBars.Add(bar));
            var tick1 = new Tick(testSession.From.AddSeconds(45), 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(testSession.From.AddSeconds(65), 4505.00m, 4504.75m, 4505.25m, 50);

            feed.HandleTick(tick1);
            feed.HandleTick(tick2);

            Assert.AreEqual(testSession.From, closedBars[0].OpenET);
        }

        [Test]
        public void HandleTick_With5SecondBars()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 5, bar => closedBars.Add(bar));
            var tick1 = new Tick(testSession.From.AddSeconds(2), 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(testSession.From.AddSeconds(7), 4505.00m, 4504.75m, 4505.25m, 50);

            feed.HandleTick(tick1);
            feed.HandleTick(tick2);

            Assert.AreEqual(1, closedBars.Count);
        }

        [Test]
        public void HandleTick_With300SecondBars()
        {
            var feed = new BarFeed(Symbol.ES, testSession, 300, bar => closedBars.Add(bar));
            var tick1 = new Tick(testSession.From.AddSeconds(100), 4500.00m, 4499.75m, 4500.25m, 100);
            var tick2 = new Tick(testSession.From.AddSeconds(350), 4505.00m, 4504.75m, 4505.25m, 50);

            feed.HandleTick(tick1);
            feed.HandleTick(tick2);

            Assert.AreEqual(1, closedBars.Count);
        }
    }
}

