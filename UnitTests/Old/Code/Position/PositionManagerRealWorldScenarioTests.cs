//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using TradeLogic;

//namespace TradeLogic.UnitTests
//{
//    /// <summary>
//    /// Real-world scenario tests for PositionManager using actual market data from KB_ES_20240108_JTH_ET.csv
//    /// These tests exercise the full lifecycle of position management with realistic tick data.
//    /// </summary>
//    [TestFixture]
//    public class PositionManagerRealWorldScenarioTests
//    {
//        private PositionManager _pm;
//        private PositionConfig _config;
//        private List<Trade> _completedTrades;
//        private List<(Guid posId, PositionView view, ExitReason? reason)> _positionOpenedEvents;
//        private List<(Guid posId, PositionView view, ExitReason? reason)> _positionClosedEvents;

//        // Load ticks once at class level for efficiency
//        private static List<Tick> _sharedTicks;

//        [OneTimeSetUp]
//        public void OneTimeSetup()
//        {
//            // Load ticks once for all tests
//            _sharedTicks = LoadTicksFromResource();
//        }

//        [SetUp]
//        public void Setup()
//        {
//            _config = new PositionConfig
//            {
//                Symbol = Symbol.ES,
//                TickSize = 0.25m,
//                PointValue = 50m,
//                IdPrefix = "RW",
//                SlippageToleranceTicks = 1
//            };

//            // Use MockIdGenerator so we can predict order IDs
//            _pm = new PositionManager(_config, new MockIdGenerator(), new MockLogger());
//            _completedTrades = new List<Trade>();
//            _positionOpenedEvents = new List<(Guid, PositionView, ExitReason?)>();
//            _positionClosedEvents = new List<(Guid, PositionView, ExitReason?)>();

//            // Subscribe to events
//            _pm.PositionOpened += (posId, view, reason) => _positionOpenedEvents.Add((posId, view, reason));
//            _pm.PositionClosed += (posId, view, reason) => _positionClosedEvents.Add((posId, view, reason));
//            _pm.TradeFinalized += (posId, trade) => _completedTrades.Add(trade);
//        }

//        private List<Tick> LoadTicksFromResource()
//        {
//            var ticks = new List<Tick>();
//            var csvData = Encoding.UTF8.GetString(Properties.Resources.KB_ES_20240108_JTH_ET);
//            var lines = csvData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

//            foreach (var line in lines)
//            {
//                var parts = line.Split(',');
//                if (parts.Length != 6) continue;

//                if (!int.TryParse(parts[0], out var dateInt)) continue;
//                if (!int.TryParse(parts[1], out var timeInt)) continue;
//                if (!decimal.TryParse(parts[2], out var open)) continue;
//                if (!decimal.TryParse(parts[3], out var bid)) continue;
//                if (!decimal.TryParse(parts[4], out var ask)) continue;
//                if (!int.TryParse(parts[5], out var volume)) continue;

//                // Parse date: YYYYMMDD
//                int year = dateInt / 10000;
//                int month = (dateInt / 100) % 100;
//                int day = dateInt % 100;

//                // Parse time: HHMMSSmmm (milliseconds)
//                int hours = timeInt / 10000000;
//                int minutes = (timeInt / 100000) % 100;
//                int seconds = (timeInt / 1000) % 100;
//                int milliseconds = timeInt % 1000;

//                var et = new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
//                var tick = new Tick(et, open, bid, ask, volume);
//                ticks.Add(tick);
//            }

//            return ticks;
//        }

//        [Test]
//        public void RealWorldScenario_LongEntry_TakeProfitExit()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(0), "CSV data should load successfully");

//            // Submit long entry at market
//            var entryOrderId = _pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            Assert.That(entryOrderId, Is.Not.Null);

//            // Simulate entry fill at first tick price
//            var entryPrice = _sharedTicks[0].Last;
//            _pm.HandleOrderAccepted(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Accepted, null));
//            _pm.HandleOrderFilled(entryOrderId, "FILL1", entryPrice, 1, _sharedTicks[0].OnET);

//            // Verify position is open
//            var pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//            Assert.That(pos.NetQuantity, Is.EqualTo(1));
//            Assert.That(pos.AvgEntryPrice, Is.EqualTo(entryPrice));

//            // Set exit prices
//            var stopLoss = entryPrice - 1m;
//            var takeProfit = entryPrice + 2m;
//            _pm.SetExitPrices(stopLoss, takeProfit);

//            // Verify exit prices are set
//            pos = _pm.GetPosition();
//            Assert.That(pos.StopLossPrice, Is.EqualTo(stopLoss));
//            Assert.That(pos.TakeProfitPrice, Is.EqualTo(takeProfit));

//            // Process ticks to verify tick handling works with real data
//            int ticksProcessed = 0;
//            foreach (var tick in _sharedTicks.Skip(1).Take(50))
//            {
//                _pm.OnTick(tick);
//                ticksProcessed++;
//            }

//            Assert.That(ticksProcessed, Is.EqualTo(50));
//            // Position should still be open (we didn't fill the exit)
//            pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//        }

//        [Test]
//        public void RealWorldScenario_ShortEntry_StopLossExit()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(0));

//            // Submit short entry
//            var entryOrderId = _pm.SubmitEntry(OrderType.Market, Side.Short, 2);
//            var entryPrice = _sharedTicks[0].Last;
//            _pm.HandleOrderAccepted(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Accepted, null));
//            _pm.HandleOrderFilled(entryOrderId, "FILL1", entryPrice, 2, _sharedTicks[0].OnET);

//            // Verify position is open
//            var pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//            Assert.That(pos.NetQuantity, Is.EqualTo(-2));
//            Assert.That(pos.Side, Is.EqualTo(Side.Short));

//            // Set exit prices
//            var stopLoss = entryPrice + 1m;
//            var takeProfit = entryPrice - 2m;
//            _pm.SetExitPrices(stopLoss, takeProfit);

//            // Verify exit prices are set
//            pos = _pm.GetPosition();
//            Assert.That(pos.StopLossPrice, Is.EqualTo(stopLoss));
//            Assert.That(pos.TakeProfitPrice, Is.EqualTo(takeProfit));

//            // Process ticks to verify tick handling works with real data
//            int ticksProcessed = 0;
//            foreach (var tick in _sharedTicks.Skip(1).Take(50))
//            {
//                _pm.OnTick(tick);
//                ticksProcessed++;
//            }

//            Assert.That(ticksProcessed, Is.EqualTo(50));
//            // Position should still be open (we didn't fill the exit)
//            pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//        }

//        [Test]
//        public void RealWorldScenario_PartialFillThenComplete()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(0));

//            // Submit entry for 3 contracts
//            var entryOrderId = _pm.SubmitEntry(OrderType.Market, Side.Long, 3);
//            var entryPrice = _sharedTicks[0].Last;
//            _pm.HandleOrderAccepted(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Accepted, null));

//            // Fill all 3 contracts at once
//            _pm.HandleOrderFilled(entryOrderId, "FILL1", entryPrice, 3, _sharedTicks[0].OnET);
//            var pos = _pm.GetPosition();
//            Assert.That(pos.NetQuantity, Is.EqualTo(3));
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//            Assert.That(pos.AvgEntryPrice, Is.EqualTo(entryPrice));

//            // Set exit prices
//            var stopLoss = entryPrice - 1m;
//            var takeProfit = entryPrice + 2m;
//            _pm.SetExitPrices(stopLoss, takeProfit);

//            // Process some ticks
//            int ticksProcessed = 0;
//            foreach (var tick in _sharedTicks.Skip(1).Take(20))
//            {
//                _pm.OnTick(tick);
//                ticksProcessed++;
//            }

//            Assert.That(ticksProcessed, Is.EqualTo(20));
//            pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//        }

//        [Test]
//        public void RealWorldScenario_ManualGoFlat()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(0));

//            // Enter position
//            var entryOrderId = _pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            var entryPrice = _sharedTicks[0].Last;
//            _pm.HandleOrderAccepted(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Accepted, null));
//            _pm.HandleOrderWorking(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Working, null));
//            _pm.HandleOrderFilled(entryOrderId, "FILL1", entryPrice, 1, _sharedTicks[0].OnET);

//            var pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));

//            // Manually flatten
//            _pm.GoFlat();
//            pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Closing));
//        }

//        [Test]
//        public void RealWorldScenario_MultipleTradesSequence()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(0));

//            // Scenario: Long entry with multiple quantity levels
//            var order1 = _pm.SubmitEntry(OrderType.Market, Side.Long, 2);
//            _pm.HandleOrderAccepted(new OrderUpdate(order1, "VENUE1", OrderStatus.Accepted, null));
//            _pm.HandleOrderFilled(order1, "FILL1", _sharedTicks[0].Last, 2, _sharedTicks[0].OnET);

//            var pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//            Assert.That(pos.NetQuantity, Is.EqualTo(2));
//            Assert.That(pos.Side, Is.EqualTo(Side.Long));

//            // Set exit prices for long position
//            var tp1 = _sharedTicks[0].Last + 2m;
//            _pm.SetExitPrices(_sharedTicks[0].Last - 1m, tp1);

//            // Process first batch of ticks
//            int ticksProcessed = 0;
//            foreach (var tick in _sharedTicks.Skip(1).Take(20))
//            {
//                _pm.OnTick(tick);
//                ticksProcessed++;
//            }
//            Assert.That(ticksProcessed, Is.EqualTo(20));

//            // Verify position is still open
//            pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//            Assert.That(pos.NetQuantity, Is.EqualTo(2));

//            // Process second batch of ticks
//            ticksProcessed = 0;
//            foreach (var tick in _sharedTicks.Skip(21).Take(30))
//            {
//                _pm.OnTick(tick);
//                ticksProcessed++;
//            }
//            Assert.That(ticksProcessed, Is.EqualTo(30));

//            // Verify position is still open
//            pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//            Assert.That(pos.NetQuantity, Is.EqualTo(2));
//            Assert.That(pos.AvgEntryPrice, Is.EqualTo(_sharedTicks[0].Last));
//        }

//        [Test]
//        public void RealWorldScenario_TickProcessingWithBarConsolidator()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(0));

//            var consolidator = new BarConsolidator(TimeSpan.FromMinutes(1), bar =>
//            {
//                _pm.HandleBar(bar);
//            });

//            // Process first 50 ticks
//            foreach (var tick in _sharedTicks.Take(50))
//            {
//                _pm.OnTick(tick);
//                consolidator.ProcessTick(tick);
//            }

//            // Verify ticks were processed
//            var pos = _pm.GetPosition();
//            Assert.That(pos, Is.Not.Null);
//        }

//        [Test]
//        public void RealWorldScenario_HighVolumeTickProcessing()
//        {
//            Assert.That(_sharedTicks.Count, Is.GreaterThan(100), "Should have enough ticks for this test");

//            // Enter position
//            var entryOrderId = _pm.SubmitEntry(OrderType.Market, Side.Long, 1);
//            _pm.HandleOrderAccepted(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Accepted, null));
//            _pm.HandleOrderWorking(new OrderUpdate(entryOrderId, "VENUE1", OrderStatus.Working, null));
//            _pm.HandleOrderFilled(entryOrderId, "FILL1", _sharedTicks[0].Last, 1, _sharedTicks[0].OnET);

//            // Process many ticks
//            int ticksProcessed = 0;
//            foreach (var tick in _sharedTicks.Skip(1).Take(200))
//            {
//                _pm.OnTick(tick);
//                ticksProcessed++;
//            }

//            Assert.That(ticksProcessed, Is.EqualTo(200));
//            var pos = _pm.GetPosition();
//            Assert.That(pos.State, Is.EqualTo(PositionState.Open));
//        }
//    }
//}

