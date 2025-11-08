using System;
using System.Collections.Generic;
using WickScalper.Common;

namespace WickScalper.Indicators
{
    public class EmaIndicator
    {
        private readonly Series<decimal> values = new Series<decimal>();
        private readonly List<decimal> closePrices = new List<decimal>();

        private readonly decimal multiplier;

        public EmaIndicator(int period)
        {
            period.Should().BeGreaterThan(0);

            multiplier = 2.0m / (period + 1);
        }

        public Series<decimal> Values => values;

        public int Count => values.Count;

        public decimal Current => values[0];

        public void Add(decimal close)
        {
            closePrices.Add(close);

            if (Count == 0)
                values.Add(close);
            else
                values.Add(CalcEma(0, close));
        }

        public void Update(decimal close)
        {
            if (Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot update before adding initial value.");
            }

            closePrices[closePrices.Count - 1] = close;

            if (values.Count == 1)
                values.Update(close);
            else
                values.Update(CalcEma(1, close));
        }

        private decimal CalcEma(int index, decimal closePrice) =>
            (closePrice * multiplier) + (values[index] * (1 - multiplier));
    }
}