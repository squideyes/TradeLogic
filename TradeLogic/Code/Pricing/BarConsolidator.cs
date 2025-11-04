using System;

namespace TradeLogic
{
    public sealed class BarConsolidator
    {
        private readonly TimeSpan _barPeriod;
        private readonly Action<Bar> _onBarClosed;

        private DateTime? _currentBarStartET;
        private DateTime? _sessionStartET;
        private decimal _open;
        private decimal _high;
        private decimal _low;
        private decimal _close;
        private long _volume;

        public BarConsolidator(TimeSpan barPeriod, Action<Bar> onBarClosed)
        {
            if (barPeriod <= TimeSpan.Zero)
                throw new ArgumentException("Bar period must be positive", nameof(barPeriod));
            if (onBarClosed == null)
                throw new ArgumentNullException(nameof(onBarClosed));

            _barPeriod = barPeriod;
            _onBarClosed = onBarClosed;
        }

        public void ProcessTick(Tick tick)
        {
            if (tick == null)
                throw new ArgumentNullException(nameof(tick));

            // Determine which bar this tick belongs to
            var barStartET = GetBarStartTime(tick.OnET);

            // If this is a new bar, close the previous one
            if (_currentBarStartET.HasValue && barStartET != _currentBarStartET.Value)
            {
                CloseCurrentBar();
            }

            // Initialize or update current bar
            if (!_currentBarStartET.HasValue)
            {
                // Start new bar
                _currentBarStartET = barStartET;
                _sessionStartET = barStartET;  // Session start is the start of the first bar
                _open = tick.Last;
                _high = tick.Last;
                _low = tick.Last;
                _close = tick.Last;
                _volume = tick.Volume;
            }
            else
            {
                // Update current bar
                _high = Math.Max(_high, tick.Last);
                _low = Math.Min(_low, tick.Last);
                _close = tick.Last;
                _volume += tick.Volume;
            }
        }

        private DateTime GetBarStartTime(DateTime tickTime)
        {
            // If session not started yet, calculate as if it starts at this tick's bar boundary
            var sessionStart = _sessionStartET ?? GetBarStartTimeFromEpoch(tickTime);
            var timeSinceSessionStart = tickTime - sessionStart;
            var barsElapsed = (long)(timeSinceSessionStart.Ticks / _barPeriod.Ticks);
            return sessionStart.AddTicks(barsElapsed * _barPeriod.Ticks);
        }

        private DateTime GetBarStartTimeFromEpoch(DateTime tickTime)
        {
            // Round down to nearest bar period from epoch
            long ticks = tickTime.Ticks;
            long periodTicks = _barPeriod.Ticks;
            long barStartTicks = (ticks / periodTicks) * periodTicks;
            return new DateTime(barStartTicks, tickTime.Kind);
        }

        private void CloseCurrentBar()
        {
            if (!_currentBarStartET.HasValue)
                return;

            var bar = new Bar(_currentBarStartET.Value, _open, _high, _low, _close, _volume);

            _onBarClosed(bar);

            // Reset for next bar
            _currentBarStartET = null;
        }
    }
}

