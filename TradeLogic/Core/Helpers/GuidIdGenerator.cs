using System;
using System.Threading;

namespace TradeLogic
{
    public sealed class GuidIdGenerator : IIdGenerator
    {
        private int _seq = 0;
        public string NewId(string prefix)
        {
            var n = Interlocked.Increment(ref _seq);
            return string.IsNullOrEmpty(prefix)
                ? $"{n}-{Guid.NewGuid():N}"
                : $"{prefix}-{n}-{Guid.NewGuid():N}";
        }
    }
}
