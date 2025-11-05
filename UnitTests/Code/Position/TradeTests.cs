using System;
using System.Collections.Generic;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class TradeTests
    {
        private Trade CreateSampleTrade()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            
            var entryFills = new List<Fill>
            {
                new Fill("ORDER-001", "FILL-001", 150m, 10, 2.5m, openTime)
            };
            
            var exitFills = new List<Fill>
            {
                new Fill("ORDER-002", "FILL-002", 155m, 10, 2.5m, closeTime)
            };
            
            return new Trade(
                tradeId, positionId, Symbol.ES, Side.Long,
                openTime, closeTime, ExitReason.TakeProfit,
                10, 150m, 155m, 500m, 5m, 0m,
                entryFills, exitFills);
        }

        [Test]
        public void Constructor_WithValidData_CreatesTradeSuccessfully()
        {
            var trade = CreateSampleTrade();
            
            Assert.That(trade.TradeId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(trade.PositionId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(trade.Symbol, Is.EqualTo(Symbol.ES));
            Assert.That(trade.Side, Is.EqualTo(Side.Long));
            Assert.That(trade.NetQty, Is.EqualTo(10));
            Assert.That(trade.AvgEntryPrice, Is.EqualTo(150m));
            Assert.That(trade.AvgExitPrice, Is.EqualTo(155m));
            Assert.That(trade.RealizedPnl, Is.EqualTo(500m));
            Assert.That(trade.TotalFees, Is.EqualTo(5m));
            Assert.That(trade.Slippage, Is.EqualTo(0m));
        }

        [Test]
        public void Constructor_WithShortPosition_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            
            var entryFills = new List<Fill>
            {
                new Fill("ORDER-001", "FILL-001", 150m, -10, 2.5m, openTime)
            };
            
            var exitFills = new List<Fill>
            {
                new Fill("ORDER-002", "FILL-002", 145m, 10, 2.5m, closeTime)
            };
            
            var trade = new Trade(
                tradeId, positionId, Symbol.ES, Side.Short,
                openTime, closeTime, ExitReason.StopLoss,
                -10, 150m, 145m, 500m, 5m, 0m,
                entryFills, exitFills);
            
            Assert.That(trade.Side, Is.EqualTo(Side.Short));
            Assert.That(trade.NetQty, Is.EqualTo(-10));
        }

        [Test]
        public void Constructor_WithNegativeRealizedPnl_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            
            var entryFills = new List<Fill>
            {
                new Fill("ORDER-001", "FILL-001", 150m, 10, 2.5m, openTime)
            };
            
            var exitFills = new List<Fill>
            {
                new Fill("ORDER-002", "FILL-002", 145m, 10, 2.5m, closeTime)
            };
            
            var trade = new Trade(
                tradeId, positionId, Symbol.ES, Side.Long,
                openTime, closeTime, ExitReason.StopLoss,
                10, 150m, 145m, -500m, 5m, 0m,
                entryFills, exitFills);
            
            Assert.That(trade.RealizedPnl, Is.EqualTo(-500m));
        }

        [Test]
        public void Constructor_WithSlippage_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            
            var entryFills = new List<Fill>
            {
                new Fill("ORDER-001", "FILL-001", 150m, 10, 2.5m, openTime)
            };
            
            var exitFills = new List<Fill>
            {
                new Fill("ORDER-002", "FILL-002", 155m, 10, 2.5m, closeTime)
            };
            
            var trade = new Trade(
                tradeId, positionId, Symbol.ES, Side.Long,
                openTime, closeTime, ExitReason.TakeProfit,
                10, 150m, 155m, 500m, 5m, 10m,
                entryFills, exitFills);
            
            Assert.That(trade.Slippage, Is.EqualTo(10m));
        }

        [Test]
        public void Constructor_WithMultipleFills_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            
            var entryFills = new List<Fill>
            {
                new Fill("ORDER-001", "FILL-001", 150m, 5, 1.25m, openTime),
                new Fill("ORDER-001", "FILL-002", 150.5m, 5, 1.25m, openTime.AddSeconds(1))
            };
            
            var exitFills = new List<Fill>
            {
                new Fill("ORDER-002", "FILL-003", 155m, 10, 2.5m, closeTime)
            };
            
            var trade = new Trade(
                tradeId, positionId, Symbol.ES, Side.Long,
                openTime, closeTime, ExitReason.TakeProfit,
                10, 150.25m, 155m, 500m, 5m, 0m,
                entryFills, exitFills);
            
            Assert.That(trade.EntryFills.Count, Is.EqualTo(2));
            Assert.That(trade.ExitFills.Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithDifferentExitReasons_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            
            var fills = new List<Fill>
            {
                new Fill("ORDER-001", "FILL-001", 150m, 10, 2.5m, openTime)
            };
            
            var tpTrade = new Trade(tradeId, positionId, Symbol.ES, Side.Long, openTime, closeTime, ExitReason.TakeProfit, 10, 150m, 155m, 500m, 5m, 0m, fills, fills);
            var slTrade = new Trade(tradeId, positionId, Symbol.ES, Side.Long, openTime, closeTime, ExitReason.StopLoss, 10, 150m, 145m, -500m, 5m, 0m, fills, fills);
            var manualTrade = new Trade(tradeId, positionId, Symbol.ES, Side.Long, openTime, closeTime, ExitReason.ManualGoFlat, 10, 150m, 152m, 200m, 5m, 0m, fills, fills);
            var eosTrade = new Trade(tradeId, positionId, Symbol.ES, Side.Long, openTime, closeTime, ExitReason.EndOfSession, 10, 150m, 151m, 100m, 5m, 0m, fills, fills);
            
            Assert.That(tpTrade.ExitReason, Is.EqualTo(ExitReason.TakeProfit));
            Assert.That(slTrade.ExitReason, Is.EqualTo(ExitReason.StopLoss));
            Assert.That(manualTrade.ExitReason, Is.EqualTo(ExitReason.ManualGoFlat));
            Assert.That(eosTrade.ExitReason, Is.EqualTo(ExitReason.EndOfSession));
        }

        [Test]
        public void Constructor_WithDifferentSymbols_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            var fills = new List<Fill> { new Fill("O1", "F1", 150m, 10, 2.5m, openTime) };
            
            var esTrade = new Trade(tradeId, positionId, Symbol.ES, Side.Long, openTime, closeTime, ExitReason.TakeProfit, 10, 150m, 155m, 500m, 5m, 0m, fills, fills);
            var nqTrade = new Trade(tradeId, positionId, Symbol.NQ, Side.Long, openTime, closeTime, ExitReason.TakeProfit, 10, 150m, 155m, 500m, 5m, 0m, fills, fills);
            
            Assert.That(esTrade.Symbol, Is.EqualTo(Symbol.ES));
            Assert.That(nqTrade.Symbol, Is.EqualTo(Symbol.NQ));
        }

        [Test]
        public void Properties_AreReadOnly()
        {
            var trade = CreateSampleTrade();
            
            Assert.That(trade.TradeId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(trade.PositionId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(trade.Symbol, Is.Not.Null);
            Assert.That(trade.Side, Is.Not.Null);
            Assert.That(trade.EntryFills, Is.Not.Null);
            Assert.That(trade.ExitFills, Is.Not.Null);
        }

        [Test]
        public void EntryFills_IsReadOnlyList()
        {
            var trade = CreateSampleTrade();
            
            Assert.That(trade.EntryFills, Is.InstanceOf<IReadOnlyList<Fill>>());
        }

        [Test]
        public void ExitFills_IsReadOnlyList()
        {
            var trade = CreateSampleTrade();
            
            Assert.That(trade.ExitFills, Is.InstanceOf<IReadOnlyList<Fill>>());
        }

        [Test]
        public void Constructor_WithZeroFees_CreatesTradeSuccessfully()
        {
            var tradeId = Guid.NewGuid();
            var positionId = Guid.NewGuid();
            var openTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var closeTime = new DateTime(2024, 1, 15, 14, 30, 0);
            var fills = new List<Fill> { new Fill("O1", "F1", 150m, 10, 0m, openTime) };
            
            var trade = new Trade(tradeId, positionId, Symbol.ES, Side.Long, openTime, closeTime, ExitReason.TakeProfit, 10, 150m, 155m, 500m, 0m, 0m, fills, fills);
            
            Assert.That(trade.TotalFees, Is.EqualTo(0m));
        }
    }
}

