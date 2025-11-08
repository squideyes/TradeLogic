using System.Collections.Generic;
using WickScalper.Common;

namespace WickScalper.Indicators
{
    public class Series<T>
    {
        private readonly List<T> values = new List<T>();

        public int Count => values.Count;

        
        public T this[int index]
        {
            get
            {
                index.Should().BeBetween(0, values.Count - 1);

                return values[values.Count - 1 - index];
            }
        }

        public void Add(T value) => values.Add(value);

        public void Update(T value)
        {
            values.Count.Should().BeGreaterThan(0);

            values[values.Count - 1] = value;
        }
    }
}