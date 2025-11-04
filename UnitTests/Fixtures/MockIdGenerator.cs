using System.Collections.Generic;
using TradeLogic;

namespace TradeLogic.UnitTests
{
    public class MockIdGenerator : IIdGenerator
    {
        private int _counter = 0;
        public List<string> GeneratedIds { get; } = new List<string>();

        public string NewId(string prefix)
        {
            _counter++;
            var id = string.IsNullOrEmpty(prefix)
                ? $"ID-{_counter}"
                : $"{prefix}-{_counter}";
            GeneratedIds.Add(id);
            return id;
        }

        public void Reset()
        {
            _counter = 0;
            GeneratedIds.Clear();
        }
    }
}

