using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public class OrderStatusLogEntry : LogEntryBase
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public double Price { get; set; }
        public int FilledQuantity { get; set; }
        public string Details { get; set; }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "orderId", OrderId },
                { "status", Status },
                { "price", Price },
                { "filledQuantity", FilledQuantity },
                { "details", Details }
            };
        }
    }
}

