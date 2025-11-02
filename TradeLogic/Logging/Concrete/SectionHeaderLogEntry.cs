using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class SectionHeaderLogEntry : LogEntryBase
    {
        public string Title { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "title", Title }
            };
        }
    }
}

