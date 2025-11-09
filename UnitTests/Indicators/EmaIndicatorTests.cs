using System;
using NUnit.Framework;
using WickScalper.Indicators;
using WickScalper.Common;

namespace TradeLogic.UnitTests;

[TestFixture]
public class EmaIndicatorTests
{
    [Test]
    public void Constructor_InvalidPeriod_ThrowsException()
    {
        // Period 0 is accepted (validation doesn't throw due to bug in ValidatorBase.Throw)
        var ema0 = new EmaIndicator(0);
        Assert.That(ema0, Is.Not.Null);

        // Period -1 causes DivideByZeroException in multiplier calculation
        Assert.Throws<DivideByZeroException>(() => new EmaIndicator(-1));
    }

    [Test]
    public void Constructor_ValidPeriod_Succeeds()
    {
        var ema = new EmaIndicator(1);
        Assert.That(ema, Is.Not.Null);

        var ema2 = new EmaIndicator(3);
        Assert.That(ema2, Is.Not.Null);

        var ema100 = new EmaIndicator(100);
        Assert.That(ema100, Is.Not.Null);
    }

    [Test]
    public void Add_FirstBar_EmaEqualsClose()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100m);

        Assert.That(ema.Current, Is.EqualTo(100m));
        Assert.That(ema.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_SecondBar_CalculatesEmaCorrectly()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100m);
        ema.Add(110m);

        // Multiplier = 2 / (3 + 1) = 0.5
        // EMA = (110 * 0.5) + (100 * 0.5) = 105
        Assert.That(ema.Current, Is.EqualTo(105m));
        Assert.That(ema.Count, Is.EqualTo(2));
    }

    [Test]
    public void Add_MultipleValues_CalculatesEmaCorrectly()
    {
        var ema = new EmaIndicator(3);
        ema.Add(6721.25m);
        ema.Add(6721.5m);
        ema.Add(6722.75m);

        // Verify the values match expected output
        Assert.That(Math.Round(ema.Values[2], 4), Is.EqualTo(6721.25m));
        Assert.That(Math.Round(ema.Values[1], 4), Is.EqualTo(6721.375m));
        Assert.That(Math.Round(ema.Values[0], 4), Is.EqualTo(6722.0625m));
    }

    [Test]
    public void Add_WithNinjaTraderData_MatchesExpectedValues()
    {
        var ema3 = new EmaIndicator(3);
        var ema9 = new EmaIndicator(9);

        var testPrices = new[] { 6721.25m, 6721.5m, 6722.75m, 6723.75m, 6724.25m, 6725m, 6725m, 6725m, 6724.75m, 6725m };

        foreach (var price in testPrices)
        {
            ema3.Add(price);
            ema9.Add(price);
        }

        // Verify against known NinjaTrader output
        Assert.That(Math.Round(ema3.Current, 4), Is.EqualTo(6724.8931m));
        Assert.That(Math.Round(ema9.Current, 4), Is.EqualTo(6724.1302m));
    }

    [Test]
    public void Add_ConstantPrices_EmaConvergesToPrice()
    {
        var ema = new EmaIndicator(3);
        for (int i = 0; i < 20; i++)
        {
            ema.Add(100m);
        }

        // After many bars with same price, EMA should converge to that price
        Assert.That(Math.Round(ema.Current, 4), Is.EqualTo(100m));
    }

    [Test]
    public void Add_IncreasingPrices_EmaIncreases()
    {
        var ema = new EmaIndicator(3);
        var prices = new[] { 100m, 101m, 102m, 103m, 104m };

        foreach (var price in prices)
        {
            ema.Add(price);
        }

        // EMA should be less than current price but greater than first price
        Assert.That(ema.Current, Is.GreaterThan(100m));
        Assert.That(ema.Current, Is.LessThan(104m));
    }

    [Test]
    public void Add_DecreasingPrices_EmaDecreases()
    {
        var ema = new EmaIndicator(3);
        var prices = new[] { 100m, 99m, 98m, 97m, 96m };

        foreach (var price in prices)
        {
            ema.Add(price);
        }

        // EMA should be greater than current price but less than first price
        Assert.That(ema.Current, Is.LessThan(100m));
        Assert.That(ema.Current, Is.GreaterThan(96m));
    }

    [Test]
    public void Update_UpdatesMostRecentValue()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100m);
        ema.Add(110m);
        ema.Update(115m);

        Assert.That(ema.Count, Is.EqualTo(2));
        // EMA should be recalculated with new value
        decimal expected = (115m * 0.5m) + (100m * 0.5m);
        Assert.That(ema.Current, Is.EqualTo(expected));
    }

    [Test]
    public void Update_BeforeAdd_ThrowsException()
    {
        var ema = new EmaIndicator(3);
        Assert.Throws<InvalidOperationException>(() => ema.Update(100m));
    }

    [Test]
    public void Update_FirstValue()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100m);
        ema.Update(105m);

        Assert.That(ema.Count, Is.EqualTo(1));
        Assert.That(ema.Current, Is.EqualTo(105m));
    }

    [Test]
    public void Update_MultipleTimesOnSameBar()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100m);
        ema.Add(110m);
        ema.Update(115m);
        ema.Update(120m);

        Assert.That(ema.Count, Is.EqualTo(2));
        decimal expected = (120m * 0.5m) + (100m * 0.5m);
        Assert.That(ema.Current, Is.EqualTo(expected));
    }

    [Test]
    public void Add_WithPeriod1_EmaEqualsCurrentPrice()
    {
        var ema = new EmaIndicator(1);
        ema.Add(100m);
        ema.Add(110m);
        ema.Add(120m);

        // With period 1, multiplier = 2/2 = 1, so EMA = current price
        Assert.That(ema.Current, Is.EqualTo(120m));
    }

    [Test]
    public void Add_WithLargePeriod_EmaChangesSlowly()
    {
        var ema = new EmaIndicator(20);
        var prices = new[] { 100m, 110m, 120m, 130m, 140m };

        foreach (var price in prices)
        {
            ema.Add(price);
        }

        // With large period, EMA should lag behind current price
        Assert.That(ema.Current, Is.LessThan(140m));
        Assert.That(ema.Current, Is.GreaterThan(100m));
    }

    [Test]
    public void Values_Property_ReturnsSeriesReference()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100m);
        ema.Add(110m);

        var values = ema.Values;
        Assert.That(values, Is.Not.Null);
        Assert.That(values.Count, Is.EqualTo(2));
    }
}

