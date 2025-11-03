using System;
using System.Collections.Generic;

namespace TradeLogic.Logging
{
    public sealed class FillLogEntry : LogEntryBase
    {
        public string ClientOrderId { get; set; }
        public string FillId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Commission { get; set; }
        public DateTime FillTimeET { get; set; }
        public bool IsPartial { get; set; }
        public string Message { get; set; }

        public FillLogEntry(
            string clientOrderId,
            string fillId,
            decimal price,
            int quantity,
            decimal commission,
            DateTime fillTimeET,
            bool isPartial,
            string message,
            LogLevel level = LogLevel.Info) : base(level)
        {
            ClientOrderId = clientOrderId;
            FillId = fillId;
            Price = price;
            Quantity = quantity;
            Commission = commission;
            FillTimeET = fillTimeET;
            IsPartial = isPartial;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "clientOrderId", ClientOrderId },
                { "fillId", FillId },
                { "price", Price },
                { "quantity", Quantity },
                { "commission", Commission },
                { "fillTimeET", FillTimeET },
                { "isPartial", IsPartial },
                { "message", Message }
            };
        }
    }
}

