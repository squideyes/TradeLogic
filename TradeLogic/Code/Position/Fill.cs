using System;

namespace TradeLogic
{
    public sealed class Fill
    {
        public string OrderIdOrClientOrderId { get; private set; }
        public string FillId { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public decimal Commission { get; private set; }
        public DateTime ETTime { get; private set; }

        public Fill(string orderIdOrClientOrderId, string fillId, decimal price, int quantity, decimal commission, DateTime etTime)
        {
            OrderIdOrClientOrderId = orderIdOrClientOrderId;
            FillId = fillId;
            Price = price;
            Quantity = quantity;
            Commission = commission;
            ETTime = etTime;
        }

        public Fill WithCommission(decimal c)
        {
            return new Fill(OrderIdOrClientOrderId, FillId, Price, Quantity, c, ETTime);
        }
    }
}
