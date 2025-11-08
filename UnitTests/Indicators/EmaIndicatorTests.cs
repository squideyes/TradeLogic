using Xunit;

namespace EmaAndAtrMatch.Tests;

public class EmaIndicatorTests
{
    [Fact]
    public void Constructor_InvalidPeriod_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new EmaIndicator(0));
        Assert.Throws<ArgumentException>(() => new EmaIndicator(-1));
    }

    [Fact]
    public void Constructor_ValidPeriod_Succeeds()
    {
        var ema = new EmaIndicator(1);
        Assert.NotNull(ema);

        var ema2 = new EmaIndicator(3);
        Assert.NotNull(ema2);

        var ema100 = new EmaIndicator(100);
        Assert.NotNull(ema100);
    }

    [Fact]
    public void Add_FirstBar_EmaEqualsClose()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100.0);

        Assert.Equal(100.0, ema.Current);
        Assert.Equal(1, ema.Count);
    }

    [Fact]
    public void Add_SecondBar_CalculatesEmaCorrectly()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100.0);
        ema.Add(110.0);

        // Multiplier = 2 / (3 + 1) = 0.5
        // EMA = (110 * 0.5) + (100 * 0.5) = 105
        Assert.Equal(105.0, ema.Current);
        Assert.Equal(2, ema.Count);
    }

    [Fact]
    public void Add_MultipleValues_CalculatesEmaCorrectly()
    {
        var ema = new EmaIndicator(3);
        ema.Add(6721.25);
        ema.Add(6721.5);
        ema.Add(6722.75);

        // Verify the values match expected output
        Assert.Equal(6721.25, Math.Round(ema.Values[2], 4));
        Assert.Equal(6721.375, Math.Round(ema.Values[1], 4));
        Assert.Equal(6722.0625, Math.Round(ema.Values[0], 4));
    }

    [Fact]
    public void Add_WithNinjaTraderData_MatchesExpectedValues()
    {
        var ema3 = new EmaIndicator(3);
        var ema9 = new EmaIndicator(9);

        var testPrices = new[] { 6721.25, 6721.5, 6722.75, 6723.75, 6724.25, 6725, 6725, 6725, 6724.75, 6725 };

        foreach (var price in testPrices)
        {
            ema3.Add(price);
            ema9.Add(price);
        }

        // Verify against known NinjaTrader output
        Assert.Equal(6724.8931, Math.Round(ema3.Current, 4));
        Assert.Equal(6724.1302, Math.Round(ema9.Current, 4));
    }

    [Fact]
    public void Add_ConstantPrices_EmaConvergesToPrice()
    {
        var ema = new EmaIndicator(3);
        for (int i = 0; i < 20; i++)
        {
            ema.Add(100.0);
        }

        // After many bars with same price, EMA should converge to that price
        Assert.Equal(100.0, Math.Round(ema.Current, 4));
    }

    [Fact]
    public void Add_IncreasingPrices_EmaIncreases()
    {
        var ema = new EmaIndicator(3);
        var prices = new[] { 100.0, 101.0, 102.0, 103.0, 104.0 };

        foreach (var price in prices)
        {
            ema.Add(price);
        }

        // EMA should be less than current price but greater than first price
        Assert.True(ema.Current > 100.0);
        Assert.True(ema.Current < 104.0);
    }

    [Fact]
    public void Add_DecreasingPrices_EmaDecreases()
    {
        var ema = new EmaIndicator(3);
        var prices = new[] { 100.0, 99.0, 98.0, 97.0, 96.0 };

        foreach (var price in prices)
        {
            ema.Add(price);
        }

        // EMA should be greater than current price but less than first price
        Assert.True(ema.Current < 100.0);
        Assert.True(ema.Current > 96.0);
    }

    [Fact]
    public void Update_UpdatesMostRecentValue()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100.0);
        ema.Add(110.0);
        ema.Update(115.0);

        Assert.Equal(2, ema.Count);
        // EMA should be recalculated with new value
        double expected = (115.0 * 0.5) + (100.0 * 0.5);
        Assert.Equal(expected, ema.Current);
    }

    [Fact]
    public void Update_BeforeAdd_ThrowsException()
    {
        var ema = new EmaIndicator(3);
        Assert.Throws<InvalidOperationException>(() => ema.Update(100.0));
    }

    [Fact]
    public void Update_FirstValue()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100.0);
        ema.Update(105.0);

        Assert.Equal(1, ema.Count);
        Assert.Equal(105.0, ema.Current);
    }

    [Fact]
    public void Update_MultipleTimesOnSameBar()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100.0);
        ema.Add(110.0);
        ema.Update(115.0);
        ema.Update(120.0);

        Assert.Equal(2, ema.Count);
        double expected = (120.0 * 0.5) + (100.0 * 0.5);
        Assert.Equal(expected, ema.Current);
    }

    [Fact]
    public void Add_WithPeriod1_EmaEqualsCurrentPrice()
    {
        var ema = new EmaIndicator(1);
        ema.Add(100.0);
        ema.Add(110.0);
        ema.Add(120.0);

        // With period 1, multiplier = 2/2 = 1, so EMA = current price
        Assert.Equal(120.0, ema.Current);
    }

    [Fact]
    public void Add_WithLargePeriod_EmaChangesSlowly()
    {
        var ema = new EmaIndicator(20);
        var prices = new[] { 100.0, 110.0, 120.0, 130.0, 140.0 };

        foreach (var price in prices)
        {
            ema.Add(price);
        }

        // With large period, EMA should lag behind current price
        Assert.True(ema.Current < 140.0);
        Assert.True(ema.Current > 100.0);
    }

    [Fact]
    public void Values_Property_ReturnsSeriesReference()
    {
        var ema = new EmaIndicator(3);
        ema.Add(100.0);
        ema.Add(110.0);

        var values = ema.Values;
        Assert.NotNull(values);
        Assert.Equal(2, values.Count);
    }
}

