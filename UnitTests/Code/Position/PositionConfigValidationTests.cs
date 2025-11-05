using System;
using NUnit.Framework;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    [TestFixture]
    public class PositionConfigValidationTests
    {
        [Test]
        public void Validate_WithValidConfig_DoesNotThrow()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            Assert.DoesNotThrow(() => config.Validate());
        }

        [Test]
        public void Validate_WithZeroTickSize_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0m,
                PointValue = 50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("TickSize must be greater than 0"));
        }

        [Test]
        public void Validate_WithNegativeTickSize_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = -0.25m,
                PointValue = 50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("TickSize must be greater than 0"));
        }

        [Test]
        public void Validate_WithZeroPointValue_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 0m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("PointValue must be greater than 0"));
        }

        [Test]
        public void Validate_WithNegativePointValue_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = -50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("PointValue must be greater than 0"));
        }

        [Test]
        public void Validate_WithNullIdPrefix_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = null,
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("IdPrefix cannot be null or empty"));
        }

        [Test]
        public void Validate_WithEmptyIdPrefix_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = "",
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("IdPrefix cannot be null or empty"));
        }

        [Test]
        public void Validate_WithWhitespaceIdPrefix_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = "   ",
                SlippageToleranceTicks = 1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("IdPrefix cannot be null or empty"));
        }

        [Test]
        public void Validate_WithNegativeSlippageToleranceTicks_ThrowsArgumentException()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = -1
            };

            var ex = Assert.Throws<ArgumentException>(() => config.Validate());
            Assert.That(ex.Message, Contains.Substring("SlippageToleranceTicks cannot be negative"));
        }

        [Test]
        public void Validate_WithZeroSlippageToleranceTicks_DoesNotThrow()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 0
            };

            Assert.DoesNotThrow(() => config.Validate());
        }

        [Test]
        public void PositionManager_Constructor_ValidatesConfig()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0m,  // Invalid
                PointValue = 50m,
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            var ex = Assert.Throws<ArgumentException>(() =>
                new PositionManager(config, idGen, logger));
            Assert.That(ex.Message, Contains.Substring("TickSize must be greater than 0"));
        }

        [Test]
        public void PositionManager_Constructor_ValidatesIdPrefix()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = 50m,
                IdPrefix = null,  // Invalid
                SlippageToleranceTicks = 1
            };

            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            var ex = Assert.Throws<ArgumentException>(() =>
                new PositionManager(config, idGen, logger));
            Assert.That(ex.Message, Contains.Substring("IdPrefix cannot be null or empty"));
        }

        [Test]
        public void PositionManager_Constructor_ValidatesPointValue()
        {
            var config = new PositionConfig
            {
                Symbol = Symbol.ES,
                TickSize = 0.25m,
                PointValue = -50m,  // Invalid
                IdPrefix = "TEST",
                SlippageToleranceTicks = 1
            };

            var idGen = new MockIdGenerator();
            var logger = new MockLogger();

            var ex = Assert.Throws<ArgumentException>(() =>
                new PositionManager(config, idGen, logger));
            Assert.That(ex.Message, Contains.Substring("PointValue must be greater than 0"));
        }
    }
}

