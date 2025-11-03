using System;
using TradeLogic.UnitTests.Fixtures;
using TradeLogic.UnitTests.TestFramework;

namespace TradeLogic.UnitTests
{
    public class SessionEndTests
    {
        private PositionManager CreatePositionManager(DateTime et)
        {
            var config = new PositionConfig { Symbol = "AAPL" };
            var clock = new MockClock(et);
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, clock, feeModel, idGen, logger);
        }

        [TestFramework.Test]
        public void OnClock_BeforeSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 15, 0, 0); // 3 PM ET
            var pm = CreatePositionManager(et);
            var nextTime = new DateTime(2024, 1, 15, 15, 30, 0); // 3:30 PM ET
            Assert.DoesNotThrow(() => pm.OnClock(nextTime));
        }

        [TestFramework.Test]
        public void OnClock_AtSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 15, 59, 0); // 3:59 PM ET
            var pm = CreatePositionManager(et);
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0); // 4 PM ET (session end)
            Assert.DoesNotThrow(() => pm.OnClock(sessionEnd));
        }

        [TestFramework.Test]
        public void OnClock_AfterSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 16, 0, 0); // 4 PM ET (session end)
            var pm = CreatePositionManager(et);
            var afterEnd = new DateTime(2024, 1, 15, 16, 30, 0); // 4:30 PM ET
            Assert.DoesNotThrow(() => pm.OnClock(afterEnd));
        }

        [TestFramework.Test]
        public void OnClock_MultiDay()
        {
            var et = new DateTime(2024, 1, 15, 16, 0, 0); // 4 PM ET (session end)
            var pm = CreatePositionManager(et);
            var nextDay = new DateTime(2024, 1, 16, 9, 30, 0); // Next day 9:30 AM ET
            Assert.DoesNotThrow(() => pm.OnClock(nextDay));
        }

        [TestFramework.Test]
        public void OnClock_WithOpenPosition_BeforeSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var pm = CreatePositionManager(et);
            
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var beforeEnd = new DateTime(2024, 1, 15, 15, 30, 0);
            Assert.DoesNotThrow(() => pm.OnClock(beforeEnd));
            
            var view = pm.GetView();
            Assert.Equal(PositionState.Open, view.State);
        }

        [TestFramework.Test]
        public void OnClock_WithOpenPosition_AtSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var pm = CreatePositionManager(et);
            
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(sessionEnd));
            
            var view = pm.GetView();
            // Position should still be open or transitioning to closing
            Assert.True(view.State == PositionState.Open || view.State == PositionState.Closing);
        }

        [TestFramework.Test]
        public void OnClock_WithArmedExits_AtSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var pm = CreatePositionManager(et);
            
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            pm.ArmExits(stopLossPrice: 95m, takeProfitPrice: 105m);
            
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(sessionEnd));
        }

        [TestFramework.Test]
        public void OnClock_WithoutArmedExits_AtSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var pm = CreatePositionManager(et);
            
            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));
            
            // No exits armed
            
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(sessionEnd));
        }

        [TestFramework.Test]
        public void OnClock_FlatPosition_AtSessionEnd()
        {
            var et = new DateTime(2024, 1, 15, 10, 0, 0);
            var pm = CreatePositionManager(et);
            
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnClock(sessionEnd));
            
            var view = pm.GetView();
            Assert.Equal(PositionState.Flat, view.State);
        }
    }
}

