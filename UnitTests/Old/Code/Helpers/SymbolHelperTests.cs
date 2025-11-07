//using System;
//using NUnit.Framework;
//using TradeLogic;

//namespace TradeLogic.UnitTests
//{
//    [TestFixture]
//    public class SymbolHelperTests
//    {
//        [Test]
//        public void Parse_WithValidSymbol_ReturnsCorrectEnum()
//        {
//            Assert.That(SymbolHelper.Parse("ES"), Is.EqualTo(Symbol.ES));
//            Assert.That(SymbolHelper.Parse("NQ"), Is.EqualTo(Symbol.NQ));
//            Assert.That(SymbolHelper.Parse("CL"), Is.EqualTo(Symbol.CL));
//            Assert.That(SymbolHelper.Parse("GC"), Is.EqualTo(Symbol.GC));
//        }

//        [Test]
//        public void Parse_WithLowercaseSymbol_ReturnsCorrectEnum()
//        {
//            Assert.That(SymbolHelper.Parse("es"), Is.EqualTo(Symbol.ES));
//            Assert.That(SymbolHelper.Parse("nq"), Is.EqualTo(Symbol.NQ));
//        }

//        [Test]
//        public void Parse_WithWhitespace_ReturnsCorrectEnum()
//        {
//            Assert.That(SymbolHelper.Parse("  ES  "), Is.EqualTo(Symbol.ES));
//            Assert.That(SymbolHelper.Parse("\tNQ\t"), Is.EqualTo(Symbol.NQ));
//        }

//        [Test]
//        public void Parse_WithMixedCase_ReturnsCorrectEnum()
//        {
//            Assert.That(SymbolHelper.Parse("Es"), Is.EqualTo(Symbol.ES));
//            Assert.That(SymbolHelper.Parse("nQ"), Is.EqualTo(Symbol.NQ));
//        }

//        [Test]
//        public void Parse_WithNullString_ThrowsArgumentException()
//        {
//            var ex = Assert.Throws<ArgumentException>(() => SymbolHelper.Parse(null));
//            Assert.That(ex.Message, Contains.Substring("cannot be null or empty"));
//        }

//        [Test]
//        public void Parse_WithEmptyString_ThrowsArgumentException()
//        {
//            var ex = Assert.Throws<ArgumentException>(() => SymbolHelper.Parse(""));
//            Assert.That(ex.Message, Contains.Substring("cannot be null or empty"));
//        }

//        [Test]
//        public void Parse_WithWhitespaceOnlyString_ThrowsArgumentException()
//        {
//            var ex = Assert.Throws<ArgumentException>(() => SymbolHelper.Parse("   "));
//            Assert.That(ex.Message, Contains.Substring("cannot be null or empty"));
//        }

//        [Test]
//        public void Parse_WithInvalidSymbol_ThrowsArgumentException()
//        {
//            var ex = Assert.Throws<ArgumentException>(() => SymbolHelper.Parse("INVALID"));
//            Assert.That(ex.Message, Contains.Substring("Unknown symbol"));
//        }

//        [Test]
//        public void Parse_WithAllValidSymbols_ReturnsCorrectEnums()
//        {
//            Assert.That(SymbolHelper.Parse("BP"), Is.EqualTo(Symbol.BP));
//            Assert.That(SymbolHelper.Parse("CL"), Is.EqualTo(Symbol.CL));
//            Assert.That(SymbolHelper.Parse("ES"), Is.EqualTo(Symbol.ES));
//            Assert.That(SymbolHelper.Parse("EU"), Is.EqualTo(Symbol.EU));
//            Assert.That(SymbolHelper.Parse("FV"), Is.EqualTo(Symbol.FV));
//            Assert.That(SymbolHelper.Parse("GC"), Is.EqualTo(Symbol.GC));
//            Assert.That(SymbolHelper.Parse("JY"), Is.EqualTo(Symbol.JY));
//            Assert.That(SymbolHelper.Parse("MES"), Is.EqualTo(Symbol.MES));
//            Assert.That(SymbolHelper.Parse("MNQ"), Is.EqualTo(Symbol.MNQ));
//            Assert.That(SymbolHelper.Parse("MCL"), Is.EqualTo(Symbol.MCL));
//            Assert.That(SymbolHelper.Parse("NQ"), Is.EqualTo(Symbol.NQ));
//            Assert.That(SymbolHelper.Parse("TY"), Is.EqualTo(Symbol.TY));
//            Assert.That(SymbolHelper.Parse("US"), Is.EqualTo(Symbol.US));
//        }

//        [Test]
//        public void TryParse_WithValidSymbol_ReturnsTrue()
//        {
//            bool result = SymbolHelper.TryParse("ES", out var symbol);
//            Assert.That(result, Is.True);
//            Assert.That(symbol, Is.EqualTo(Symbol.ES));
//        }

//        [Test]
//        public void TryParse_WithInvalidSymbol_ReturnsFalse()
//        {
//            bool result = SymbolHelper.TryParse("INVALID", out var symbol);
//            Assert.That(result, Is.False);
//        }

//        [Test]
//        public void TryParse_WithNullString_ReturnsFalse()
//        {
//            bool result = SymbolHelper.TryParse(null, out var symbol);
//            Assert.That(result, Is.False);
//        }

//        [Test]
//        public void TryParse_WithEmptyString_ReturnsFalse()
//        {
//            bool result = SymbolHelper.TryParse("", out var symbol);
//            Assert.That(result, Is.False);
//        }

//        [Test]
//        public void TryParse_WithWhitespaceOnlyString_ReturnsFalse()
//        {
//            bool result = SymbolHelper.TryParse("   ", out var symbol);
//            Assert.That(result, Is.False);
//        }

//        [Test]
//        public void TryParse_WithLowercaseSymbol_ReturnsTrue()
//        {
//            bool result = SymbolHelper.TryParse("es", out var symbol);
//            Assert.That(result, Is.True);
//            Assert.That(symbol, Is.EqualTo(Symbol.ES));
//        }

//        [Test]
//        public void TryParse_WithWhitespace_ReturnsTrue()
//        {
//            bool result = SymbolHelper.TryParse("  NQ  ", out var symbol);
//            Assert.That(result, Is.True);
//            Assert.That(symbol, Is.EqualTo(Symbol.NQ));
//        }
//    }
//}

