using System;
using TradeLogic.UnitTests.Fixtures;
using TradeLogic.UnitTests.TestFramework;

namespace TradeLogic.UnitTests.Core.Position
{
    public class SessionEndTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, feeModel, idGen, logger);
        }

        private Tick MakeTick(DateTime et)
        {
            return new Tick(et, 150m, 149.99m, 150.01m, 1000);
        }

        [TestFramework.Test]
        public void OnClock_BeforeSessionEnd()
        {
            var pm = CreatePositionManager();
            var nextTime = new DateTime(2024, 1, 15, 15, 30, 0); // 3:30 PM ET
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(nextTime)));
        }

        [TestFramework.Test]
        public void OnClock_AtSessionEnd()
        {
            var pm = CreatePositionManager();
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0); // 4 PM ET (session end)
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(sessionEnd)));
        }

        [TestFramework.Test]
        public void OnClock_AfterSessionEnd()
        {
            var pm = CreatePositionManager();
            var afterEnd = new DateTime(2024, 1, 15, 16, 30, 0); // 4:30 PM ET
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(afterEnd)));
        }

        [TestFramework.Test]
        public void OnClock_MultiDay()
        {
            var pm = CreatePositionManager();
            var nextDay = new DateTime(2024, 1, 16, 9, 30, 0); // Next day 9:30 AM ET
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(nextDay)));
        }

        [TestFramework.Test]
        public void OnClock_WithOpenPosition_BeforeSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            var beforeEnd = new DateTime(2024, 1, 15, 15, 30, 0);
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(beforeEnd)));

            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void OnClock_WithOpenPosition_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(sessionEnd)));

            var view = pm.GetView();
            // Position should still be open or transitioning to closing
            Assert.True(view.State == PositionState.Open || view.State == PositionState.Closing);
        }

        [TestFramework.Test]
        public void OnClock_WithArmedExits_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(sessionEnd)));
        }

        [TestFramework.Test]
        public void OnClock_WithoutArmedExits_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            // No exits armed

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(sessionEnd)));
        }

        [TestFramework.Test]
        public void OnClock_FlatPosition_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(MakeTick(sessionEnd)));

            var view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
        }
    }
}

