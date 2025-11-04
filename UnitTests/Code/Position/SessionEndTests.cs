using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class SessionEndTests
    {
        private PositionManager CreatePositionManager()
        {
            var config = new PositionConfig { Symbol = Symbol.ES };
            var feeModel = new MockFeeModel(1m);
            var idGen = new MockIdGenerator();
            var logger = new MockLogger();
            return new PositionManager(config, feeModel, idGen, logger);
        }

        private Tick MakeTick(DateTime et)
        {
            return new Tick(et, 150m, 149.99m, 150.01m, 1000);
        }

        [Test]
        public void OnTick_BeforeSessionEnd()
        {
            var pm = CreatePositionManager();
            var nextTime = new DateTime(2024, 1, 15, 15, 30, 0); // 3:30 PM ET
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(nextTime)));
        }

        [Test]
        public void OnTick_AtSessionEnd()
        {
            var pm = CreatePositionManager();
            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0); // 4 PM ET (session end)
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(sessionEnd)));
        }

        [Test]
        public void OnTick_AfterSessionEnd()
        {
            var pm = CreatePositionManager();
            var afterEnd = new DateTime(2024, 1, 15, 16, 30, 0); // 4:30 PM ET
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(afterEnd)));
        }

        [Test]
        public void OnTick_MultiDay()
        {
            var pm = CreatePositionManager();
            var nextDay = new DateTime(2024, 1, 16, 9, 30, 0); // Next day 9:30 AM ET
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(nextDay)));
        }

        [Test]
        public void OnTick_WithOpenPosition_BeforeSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            var beforeEnd = new DateTime(2024, 1, 15, 15, 30, 0);
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(beforeEnd)));

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Open));
        }

        [Test]
        public void OnTick_WithOpenPosition_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(sessionEnd)));

            var position = pm.GetPosition();
            // Position should still be open or transitioning to closing
            Assert.That(position.State == PositionState.Open || position.State == PositionState.Closing, Is.True);
        }

        [Test]
        public void OnTick_WithArmedExits_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, 95m, 105m);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(sessionEnd)));
        }

        [Test]
        public void OnTick_WithoutArmedExits_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var orderId = pm.SubmitEntry(OrderType.Market, Side.Long, 100, null, null);
            var acceptUpdate = new OrderUpdate(orderId, "venue1", OrderStatus.Accepted, null);
            pm.OnOrderAccepted(acceptUpdate);
            pm.OnOrderFilled(orderId, "fill1", 100m, 100, new DateTime(2024, 1, 15, 10, 1, 0));

            // No exits armed

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(sessionEnd)));
        }

        [Test]
        public void OnTick_FlatPosition_AtSessionEnd()
        {
            var pm = CreatePositionManager();

            var sessionEnd = new DateTime(2024, 1, 15, 16, 0, 0);
            Assert.DoesNotThrow(() => pm.OnTick(MakeTick(sessionEnd)));

            var position = pm.GetPosition();
            Assert.That(position.State, Is.EqualTo(PositionState.Flat));
        }
    }
}


