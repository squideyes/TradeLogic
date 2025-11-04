using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public abstract class LogEntryBase
    {
        public DateTime LoggedOn { get; set; }
        public LogLevel Level { get; set; }

        protected LogEntryBase(LogLevel level = LogLevel.Info)
        {
            LoggedOn = DateTime.UtcNow.ToET();
            Level = level;
        }

        public string Serialize()
        {
            var data = GetData();

            data["loggedOn"] = LoggedOn;
            data["level"] = Level.ToString();

            return JsonSerializer.Serialize(data);
        }

        protected abstract Dictionary<string, object> GetData();
    }
}

