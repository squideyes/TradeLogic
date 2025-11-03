using System;
using TradeLogic.UnitTests.Fixtures;
using TradeLogic.UnitTests.TestFramework;

namespace TradeLogic.UnitTests
{
    public class ExitTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var clock = new MockClock(et);
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, clock, feeModel, idGen, logger);
        }

        private void OpenPosition(PositionManager pm, Side side, int quantity)
        {
            var orderId = pm.SubmitEntry(OrderType.Market, side, quantity);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, quantity, new DateTime(2024, 1, 15, 10, 1, 0));
        }

        [TestFramework.Test]
        public void ArmExits_WithStopLoss()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: null);
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void ArmExits_WithTakeProfit()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: null, takeProfitPrice: 105m);
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void ArmExits_WithBoth()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void ReplaceExits_UpdatesPrices()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);
            pm.ReplaceExits(newStopLossPrice: 94m, newTakeProfitPrice: 106m);
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void GoFlat_TransitionsToClosing()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);
            pm.GoFlat();
            var view = pm.GetView();
            Assert.Equal(PositionState.Closing, view.State);
        }

        [TestFramework.Test]
        public void ExitArmed_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.ExitArmed += (id, view, extra) => { eventFired = true; };
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);
            Assert.True(eventFired);
        }

        [TestFramework.Test]
        public void ExitReplaced_EventFired()
        {
            var pm = CreatePositionManager();
            bool eventFired = false;
            pm.ExitReplaced += (id, view, extra) => { eventFired = true; };
            OpenPosition(pm, Side.Long, 100);
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);
            pm.ReplaceExits(newStopLossPrice: 94m, newTakeProfitPrice: 106m);
            Assert.True(eventFired);
        }

        [TestFramework.Test]
        public void ArmExits_RequiresOpenPosition()
        {
            var pm = CreatePositionManager();
            // Position is Flat, ArmExits should not throw but also not do anything
            Assert.DoesNotThrow(() => pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m));
        }

        [TestFramework.Test]
        public void ReplaceExits_RequiresOpenPosition()
        {
            var pm = CreatePositionManager();
            // Position is Flat, ReplaceExits should throw
            Assert.Throws<InvalidOperationException>(() =>
                pm.ReplaceExits(newStopLossPrice: 95m, newTakeProfitPrice: 105m));
        }

        [TestFramework.Test]
        public void ArmExits_Short_WithStopLoss()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Short, 100);
            pm.ArmExits(stopLossPrice: 105m, takeProfitPrice: null);
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void ArmExits_Short_WithTakeProfit()
        {
            var pm = CreatePositionManager();
            OpenPosition(pm, Side.Short, 100);
            pm.ArmExits(stopLossPrice: null, takeProfitPrice: 95m);
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }
    }
}

