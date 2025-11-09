using System;
using NUnit.Framework;
using WickScalper.Indicators;
using WickScalper.Common;

namespace TradeLogic.UnitTests;

[TestFixture]
public class AtrIndicatorTests
{
    [Test]
    public void Constructor_InvalidPeriod_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new AtrIndicator(0));
        Assert.Throws<ArgumentException>(() => new AtrIndicator(-1));
    }

    [Test]
    public void Constructor_ValidPeriod_Succeeds()
    {
        var atr = new AtrIndicator(1);
        Assert.That(atr, Is.Not.Null);

        var atr6 = new AtrIndicator(6);
        Assert.That(atr6, Is.Not.Null);

        var atr100 = new AtrIndicator(100);
        Assert.That(atr100, Is.Not.Null);
    }

    [Test]
    public void Add_FirstBar_AtrEqualsTrueRange()
    {
        var atr = new AtrIndicator(6);
        var bar = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        atr.Add(bar);

        Assert.That(atr.Current, Is.EqualTo(15m));
        Assert.That(atr.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_SecondBar_CalculatesSimpleAverage()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        var bar2 = new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000);

        atr.Add(bar1);
        atr.Add(bar2);

        // TR1 = 15, TR2 = 15, Average = 15
        Assert.That(atr.Current, Is.EqualTo(15m));
        Assert.That(atr.Count, Is.EqualTo(2));
    }

    [Test]
    public void Add_MultipleValues_CalculatesAtrCorrectly()
    {
        var atr = new AtrIndicator(6);

        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 6720.25m, 6721.25m, 6720.25m, 6721.25m, 66),
            new WickScalper.Common.Bar(DateTime.Now, 6721m, 6721.75m, 6721m, 6721.5m, 110),
            new WickScalper.Common.Bar(DateTime.Now, 6721.75m, 6723.5m, 6721.75m, 6722.75m, 183),
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // Verify ATR values
        Assert.That(Math.Round(atr.Values[2], 4), Is.EqualTo(1.0m));
        Assert.That(Math.Round(atr.Values[1], 4), Is.EqualTo(0.875m));
        Assert.That(Math.Round(atr.Values[0], 4), Is.EqualTo(1.25m));
    }

    [Test]
    public void Add_AccumulationPhase_SimpleAverage()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000), // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 110m, 120m, 105m, 115m, 1000), // TR = 15
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // During accumulation (bars 1-3), ATR = simple average
        // After bar 3, we should have transitioned to Wilder's smoothing
        Assert.That(atr.Current, Is.EqualTo(15m));
    }

    [Test]
    public void Add_SmoothingPhase_WildersMethod()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),   // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 110m, 120m, 105m, 115m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 115m, 125m, 110m, 120m, 1000),  // TR = 15
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // After period, use Wilder's smoothing: (prevATR * (period-1) + TR) / period
        // = (15 * 2 + 15) / 3 = 45 / 3 = 15
        Assert.That(atr.Current, Is.EqualTo(15m));
    }

    [Test]
    public void Add_VolatileMarket_AtrIncreases()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),   // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 110m, 120m, 105m, 115m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 115m, 130m, 110m, 125m, 1000),  // TR = 20 (high volatility)
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // ATR should increase due to larger true range
        Assert.That(atr.Current, Is.GreaterThan(15m));
    }

    [Test]
    public void Add_LowVolatilityMarket_AtrDecreases()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),   // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 110m, 120m, 105m, 115m, 1000),  // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 115m, 118m, 114m, 116m, 1000),  // TR = 4 (low volatility)
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // ATR should decrease due to smaller true range
        Assert.That(atr.Current, Is.LessThan(15m));
    }

    [Test]
    public void Add_GapUp_IncludesGapInTrueRange()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),   // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 120m, 130m, 115m, 125m, 1000),  // Gap up: TR = max(15, |130-105|, |115-105|) = 25
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // Second bar should have larger TR due to gap
        Assert.That(atr.Current, Is.GreaterThan(15m));
    }

    [Test]
    public void Add_GapDown_IncludesGapInTrueRange()
    {
        var atr = new AtrIndicator(3);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),   // TR = 15
            new WickScalper.Common.Bar(DateTime.Now, 80m, 90m, 75m, 85m, 1000),      // Gap down: TR = max(15, |90-105|, |75-105|) = 30
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // Second bar should have larger TR due to gap
        Assert.That(atr.Current, Is.GreaterThan(15m));
    }

    [Test]
    public void Update_UpdatesMostRecentValue()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        var bar2 = new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000);
        var bar2Updated = new WickScalper.Common.Bar(DateTime.Now, 105m, 120m, 100m, 115m, 1000);

        atr.Add(bar1);
        atr.Add(bar2);
        atr.Update(bar2Updated);

        Assert.That(atr.Count, Is.EqualTo(2));
        Assert.That(atr.Current, Is.GreaterThan(0m));
    }

    [Test]
    public void Update_BeforeAdd_ThrowsException()
    {
        var atr = new AtrIndicator(6);
        var bar = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        Assert.Throws<InvalidOperationException>(() => atr.Update(bar));
    }

    [Test]
    public void Update_FirstValue()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        var bar1Updated = new WickScalper.Common.Bar(DateTime.Now, 100m, 115m, 90m, 110m, 1000);

        atr.Add(bar1);
        atr.Update(bar1Updated);

        Assert.That(atr.Count, Is.EqualTo(1));
        // TR = 115 - 90 = 25
        Assert.That(atr.Current, Is.EqualTo(25m));
    }

    [Test]
    public void Update_MultipleTimesOnSameBar()
    {
        var atr = new AtrIndicator(6);
        var bar1 = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        var bar2 = new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000);
        var bar2Updated1 = new WickScalper.Common.Bar(DateTime.Now, 105m, 120m, 100m, 115m, 1000);
        var bar2Updated2 = new WickScalper.Common.Bar(DateTime.Now, 105m, 125m, 100m, 120m, 1000);

        atr.Add(bar1);
        atr.Add(bar2);
        atr.Update(bar2Updated1);
        atr.Update(bar2Updated2);

        Assert.That(atr.Count, Is.EqualTo(2));
        Assert.That(atr.Current, Is.GreaterThan(0m));
    }

    [Test]
    public void Values_Property_ReturnsSeriesReference()
    {
        var atr = new AtrIndicator(6);
        var bar = new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000);
        atr.Add(bar);

        var values = atr.Values;
        Assert.That(values, Is.Not.Null);
        Assert.That(values.Count, Is.EqualTo(1));
    }

    [Test]
    public void Add_WithPeriod1_AtrEqualsCurrentTrueRange()
    {
        var atr = new AtrIndicator(1);
        var bars = new[]
        {
            new WickScalper.Common.Bar(DateTime.Now, 100m, 110m, 95m, 105m, 1000),
            new WickScalper.Common.Bar(DateTime.Now, 105m, 115m, 100m, 110m, 1000),
        };

        foreach (var bar in bars)
        {
            atr.Add(bar);
        }

        // With period 1, ATR should equal current TR
        Assert.That(atr.Current, Is.EqualTo(15m));
    }
}

