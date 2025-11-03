using System;

namespace TradeLogic.UnitTests.Fixtures
{
    public class MockClock : IClock
    {
        private DateTime _currentET;

        public DateTime ETNow => _currentET;

        public MockClock(DateTime initialET)
        {
            _currentET = initialET;
        }

        public void SetTime(DateTime etTime)
        {
            _currentET = etTime;
        }

        public void AdvanceSeconds(int seconds)
        {
            _currentET = _currentET.AddSeconds(seconds);
        }

        public void AdvanceMinutes(int minutes)
        {
            _currentET = _currentET.AddMinutes(minutes);
        }

        public void AdvanceHours(int hours)
        {
            _currentET = _currentET.AddHours(hours);
        }
    }
}

