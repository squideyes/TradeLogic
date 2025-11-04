using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class OrderLogEntry : LogEntryBase
    {
        public string ClientOrderId { get; set; }
        public string VenueOrderId { get; set; }
        public string OrderType { get; set; }
        public string Side { get; set; }
        public int Quantity { get; set; }
        public decimal? LimitPrice { get; set; }
        public decimal? StopPrice { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public OrderLogEntry(
            string clientOrderId,
            string venueOrderId,
            string orderType,
            string side,
            int quantity,
            decimal? limitPrice,
            decimal? stopPrice,
            string status,
            string message,
            LogLevel level = LogLevel.Info) : base(level)
        {
            ClientOrderId = clientOrderId;
            VenueOrderId = venueOrderId;
            OrderType = orderType;
            Side = side;
            Quantity = quantity;
            LimitPrice = limitPrice;
            StopPrice = stopPrice;
            Status = status;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            var data = new Dictionary<string, object>
            {
                { "clientOrderId", ClientOrderId },
                { "orderType", OrderType },
                { "side", Side },
                { "quantity", Quantity },
                { "status", Status },
                { "message", Message }
            };

            if (!string.IsNullOrEmpty(VenueOrderId))
                data["venueOrderId"] = VenueOrderId;

            if (LimitPrice.HasValue)
                data["limitPrice"] = LimitPrice.Value;

            if (StopPrice.HasValue)
                data["stopPrice"] = StopPrice.Value;

            return data;
        }
    }
}

