using System;
using System.Linq;
using System.Collections.Generic;
using WickScalper.Common;

namespace WickScalper.Indicators
{
    public class AtrIndicator
    {
        private readonly Series<decimal> values = new Series<decimal>();
        private readonly List<decimal> trueRanges = new List<decimal>();

        private readonly int period;

        private decimal? prevClose;

        public Series<decimal> Values => values;

        public int Count => values.Count;

        public AtrIndicator(int period)
        {
            if (period <= 0)
            {
                throw new ArgumentException(
                    "Period must be greater than 0", nameof(period));
            }

            this.period = period;
        }

        public void Add(Bar bar)
        {
            decimal trueRange = bar.GetTrueRange(prevClose);

            trueRanges.Add(trueRange);

            prevClose = bar.Close;

            if (Count == 0)
            {
                values.Add(trueRange);
            }
            else if (values.Count < period)
            {
                values.Add(trueRanges.Sum() / (values.Count + 1));
            }
            else
            {
                var prevAtr = values[0];

                var atr = (prevAtr * (period - 1) + trueRange) / period;

                values.Add(atr);
            }
        }

        public void Update(Bar bar)
        {
            if (values.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot update before adding initial value");
            }

            trueRanges.RemoveAt(trueRanges.Count - 1);

            var trueRange = bar.GetTrueRange(prevClose);

            trueRanges.Add(trueRange);

            prevClose = bar.Close;

            if (values.Count == 1)
            {
                values.Update(trueRange);
            }
            else if (values.Count <= period)
            {
                var sum = trueRanges.Sum();

                var atr = sum / values.Count;

                values.Update(atr);
            }
            else
            {
                var prevAtr = values[1];

                var atr = (prevAtr * (period - 1) + trueRange) / period;

                values.Update(atr);
            }
        }

        public decimal Current => values[0];
    }
}