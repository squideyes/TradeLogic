using Xunit;

namespace EmaAndAtrMatch.Tests;

public class AtrIndicatorTests
{
    [Fact]
    public void Constructor_InvalidPeriod_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new AtrIndicator(0));
        Assert.Throws<ArgumentException>(() => new AtrIndicator(-1));
    }

    [Fact]
    public void Constructor_ValidPeriod_Succeeds()
    {
        var atr = new AtrIndicator(1);
        Assert.NotNull(atr);

        var atr6 = new AtrIndicator(6);
        Assert.NotNull(atr6);

        var atr100 = new AtrIndicator(100);
        Assert.NotNull(atr100);
    }

    [Fact]
    public void Add_FirstBar_AtrEqualsTrueRange()
    {
        var atr = new AtrIndicator(6);
        var bar = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        atr.Add(bar);

        Assert.Equal(15.0, atr.Current);
        Assert.Equal(1, atr.Count);
    }

    [Fact]
    public void Add_SecondBar_CalculatesSimpleAverage()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        var bar2 = new Candle(DateTime.Now, 105, 115, 100, 110, 1000);

        atr.Add(bar1);
        atr.Add(bar2);

        // TR1 = 15, TR2 = 15, Average = 15
        Assert.Equal(15.0, atr.Current);
        Assert.Equal(2, atr.Count);
    }

    [Fact]
    public void Add_MultipleValues_CalculatesAtrCorrectly()
    {
        var atr = new AtrIndicator(6);

        var bars = new[]
        {
            new Candle(DateTime.Now, 6720.25, 6721.25, 6720.25, 6721.25, 66),
            new Candle(DateTime.Now, 6721, 6721.75, 6721, 6721.5, 110),
            new Candle(DateTime.Now, 6721.75, 6723.5, 6721.75, 6722.75, 183),
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // Verify ATR values
        Assert.Equal(1.0, Math.Round(atr.Values[2], 4));
        Assert.Equal(0.875, Math.Round(atr.Values[1], 4));
        Assert.Equal(1.25, Math.Round(atr.Values[0], 4));
    }

    [Fact]
    public void Add_AccumulationPhase_SimpleAverage()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),  // TR = 15
            new Candle(DateTime.Now, 105, 115, 100, 110, 1000), // TR = 15
            new Candle(DateTime.Now, 110, 120, 105, 115, 1000), // TR = 15
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // During accumulation (bars 1-3), ATR = simple average
        // After bar 3, we should have transitioned to Wilder's smoothing
        Assert.Equal(15.0, atr.Current);
    }

    [Fact]
    public void Add_SmoothingPhase_WildersMethod()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),   // TR = 15
            new Candle(DateTime.Now, 105, 115, 100, 110, 1000),  // TR = 15
            new Candle(DateTime.Now, 110, 120, 105, 115, 1000),  // TR = 15
            new Candle(DateTime.Now, 115, 125, 110, 120, 1000),  // TR = 15
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // After period, use Wilder's smoothing: (prevATR * (period-1) + TR) / period
        // = (15 * 2 + 15) / 3 = 45 / 3 = 15
        Assert.Equal(15.0, atr.Current);
    }

    [Fact]
    public void Add_VolatileMarket_AtrIncreases()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),   // TR = 15
            new Candle(DateTime.Now, 105, 115, 100, 110, 1000),  // TR = 15
            new Candle(DateTime.Now, 110, 120, 105, 115, 1000),  // TR = 15
            new Candle(DateTime.Now, 115, 130, 110, 125, 1000),  // TR = 20 (high volatility)
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // ATR should increase due to larger true range
        Assert.True(atr.Current > 15.0);
    }

    [Fact]
    public void Add_LowVolatilityMarket_AtrDecreases()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),   // TR = 15
            new Candle(DateTime.Now, 105, 115, 100, 110, 1000),  // TR = 15
            new Candle(DateTime.Now, 110, 120, 105, 115, 1000),  // TR = 15
            new Candle(DateTime.Now, 115, 118, 114, 116, 1000),  // TR = 4 (low volatility)
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // ATR should decrease due to smaller true range
        Assert.True(atr.Current < 15.0);
    }

    [Fact]
    public void Add_GapUp_IncludesGapInTrueRange()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),   // TR = 15
            new Candle(DateTime.Now, 120, 130, 115, 125, 1000),  // Gap up: TR = max(15, |130-105|, |115-105|) = 25
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // Second bar should have larger TR due to gap
        Assert.True(atr.Current > 15.0);
    }

    [Fact]
    public void Add_GapDown_IncludesGapInTrueRange()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),   // TR = 15
            new Candle(DateTime.Now, 80, 90, 75, 85, 1000),      // Gap down: TR = max(15, |90-105|, |75-105|) = 30
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // Second bar should have larger TR due to gap
        Assert.True(atr.Current > 15.0);
    }

    [Fact]
    public void Update_UpdatesMostRecentValue()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        var bar2 = new Candle(DateTime.Now, 105, 115, 100, 110, 1000);
        var bar2Updated = new Candle(DateTime.Now, 105, 120, 100, 115, 1000);

        atr.Add(bar1);
        atr.Add(bar2);
        atr.Update(bar2Updated);

        Assert.Equal(2, atr.Count);
        Assert.True(atr.Current > 0);
    }

    [Fact]
    public void Update_BeforeAdd_ThrowsException()
    {
        var atr = new AtrIndicator(6);
        var bar = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        Assert.Throws<InvalidOperationException>(() => atr.Update(bar));
    }

    [Fact]
    public void Update_FirstValue()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        var bar1Updated = new Candle(DateTime.Now, 100, 115, 90, 110, 1000);

        atr.Add(bar1);
        atr.Update(bar1Updated);

        Assert.Equal(1, atr.Count);
        // TR = 115 - 90 = 25
        Assert.Equal(25.0, atr.Current);
    }

    [Fact]
    public void Update_MultipleTimesOnSameBar()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        var bar2 = new Candle(DateTime.Now, 105, 115, 100, 110, 1000);
        var bar2Updated1 = new Candle(DateTime.Now, 105, 120, 100, 115, 1000);
        var bar2Updated2 = new Candle(DateTime.Now, 105, 125, 100, 120, 1000);

        atr.Add(bar1);
        atr.Add(bar2);
        atr.Update(bar2Updated1);
        atr.Update(bar2Updated2);

        Assert.Equal(2, atr.Count);
        Assert.True(atr.Current > 0);
    }

    [Fact]
    public void Values_Property_ReturnsSeriesReference()
    {
        var atr = new AtrIndicator(6);
        var bar = new Candle(DateTime.Now, 100, 110, 95, 105, 1000);
        atr.Add(bar);

        var values = atr.Values;
        Assert.NotNull(values);
        Assert.Equal(1, values.Count);
    }

    [Fact]
    public void Add_WithPeriod1_AtrEqualsCurrentTrueRange()
    {
        var atr = new AtrIndicator(1);
        var bars = new[]
        {
            new Candle(DateTime.Now, 100, 110, 95, 105, 1000),
            new Candle(DateTime.Now, 105, 115, 100, 110, 1000),
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // With period 1, ATR should equal current TR
        Assert.Equal(15.0, atr.Current);
    }
}

