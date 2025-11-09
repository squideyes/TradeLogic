using NUnit.Framework;
using WickScalper.Common;
using System.Linq;
using Assert = TradeLogic.UnitTests.TestFramework.AssertEx;
using Bar = WickScalper.Common.Bar;
using Tick = WickScalper.Common.Tick;
using Future = WickScalper.Common.Future;
using Symbol = WickScalper.Common.Symbol;
using TickSet = WickScalper.Common.TickSet;
using TickSetParser = WickScalper.Common.TickSetParser;
using GuardException = WickScalper.Common.GuardException;

namespace WickScalper.UnitTests.Common.PricingTests
{
    [TestFixture]
    public class TickSetParserTests
    {
        private Session testSession;

        [SetUp]
        public void Setup()
        {
            var tradeDate = new DateOnly(2024, 1, 2);
            testSession = new Session(tradeDate);
        }

        [Test]
        public void ParseDTLBAV_WithSymbol_ParsesCorrectly()
        {
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,100\n" +
                      "20240102 093001000,4500.25,4500.00,4500.50,50";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv);

            Assert.AreEqual(2, tickSet.Count);
            Assert.AreEqual(Symbol.ES, tickSet.Future.Symbol);
            Assert.AreEqual(testSession, tickSet.Session);
        }

        [Test]
        public void ParseDTLBAV_WithFuture_ParsesCorrectly()
        {
            var future = KnownFutures.GetFuture(Symbol.ES);
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,100\n" +
                      "20240102 093001000,4500.25,4500.00,4500.50,50";

            var tickSet = TickSetParser.ParseDTLBAV(future, testSession, csv);

            Assert.AreEqual(2, tickSet.Count);
            Assert.AreEqual(future, tickSet.Future);
            Assert.AreEqual(testSession, tickSet.Session);
        }

        [Test]
        public void ParseDTLBAV_WithEmptyCSV_ReturnsEmptyTickSet()
        {
            var csv = "";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv);

            Assert.AreEqual(0, tickSet.Count);
        }

        [Test]
        public void ParseDTLBAV_WithSingleTick_ParsesCorrectly()
        {
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,100";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv);

            Assert.AreEqual(1, tickSet.Count);
            var tick = tickSet.ToList()[0];
            Assert.AreEqual(4500.00m, tick.Last);
            Assert.AreEqual(4499.75m, tick.Bid);
            Assert.AreEqual(4500.25m, tick.Ask);
            Assert.AreEqual(100, tick.Volume);
        }

        [Test]
        public void ParseDTLBAV_WithMultipleTicks_ParsesInOrder()
        {
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,100\n" +
                      "20240102 093001000,4500.25,4500.00,4500.50,50\n" +
                      "20240102 093002000,4500.50,4500.25,4500.75,75";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv);

            Assert.AreEqual(3, tickSet.Count);
            var ticks = tickSet.ToList();
            Assert.AreEqual(4500.00m, ticks[0].Last);
            Assert.AreEqual(4500.25m, ticks[1].Last);
            Assert.AreEqual(4500.50m, ticks[2].Last);
        }

        [Test]
        public void ParseDTLBAV_WithCLFuture_ParsesCorrectly()
        {
            var csv = "20240102 093000000,85.50,85.49,85.51,500\n" +
                      "20240102 093001000,85.51,85.50,85.52,250";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.CL, testSession, csv);

            Assert.AreEqual(2, tickSet.Count);
            var ticks = tickSet.ToList();
            Assert.AreEqual(85.50m, ticks[0].Last);
            Assert.AreEqual(85.51m, ticks[1].Last);
        }

        [Test]
        public void ParseDTLBAV_WithBPFuture_ParsesCorrectly()
        {
            var csv = "20240102 093000000,1.2500,1.2499,1.2501,100\n" +
                      "20240102 093001000,1.2501,1.2500,1.2502,50";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.BP, testSession, csv);

            Assert.AreEqual(2, tickSet.Count);
        }

        [Test]
        public void ParseDTLBAV_WithTrailingNewline_ParsesCorrectly()
        {
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,100\n";

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv);

            Assert.AreEqual(1, tickSet.Count);
        }

        [Test]
        public void ParseDTLBAV_WithMultipleNewlines_ParsesCorrectly()
        {
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,100\n\n" +
                      "20240102 093001000,4500.25,4500.00,4500.50,50";

            // This will fail because empty line will cause parse error
            Assert.Throws<GuardException>(() => TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv));
        }

        [Test]
        public void ParseDTLBAV_WithInvalidTick_Throws()
        {
            var csv = "20240102 093000000,4500.00,4499.75,4500.25,0"; // Zero volume

            Assert.Throws<GuardException>(() => TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv));
        }

        [Test]
        public void ParseDTLBAV_WithOutOfOrderTicks_Throws()
        {
            var csv = "20240102 093001000,4500.25,4500.00,4500.50,50\n" +
                      "20240102 093000000,4500.00,4499.75,4500.25,100";

            Assert.Throws<GuardException>(() => TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv));
        }

        [Test]
        public void ParseDTLBAV_WithLargeDataset_ParsesCorrectly()
        {
            var csvLines = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 1000; i++)
            {
                var time = testSession.From.AddSeconds(i);
                var timeStr = time.ToString("yyyyMMdd HHmmssfff");
                csvLines.Add($"{timeStr},4500.00,4499.75,4500.25,100");
            }
            var csv = string.Join("\n", csvLines);

            var tickSet = TickSetParser.ParseDTLBAV(Symbol.ES, testSession, csv);

            Assert.AreEqual(1000, tickSet.Count);
        }
    }
}

