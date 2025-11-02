namespace TradeLogic.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public OrderAction Action { get; set; }
        public OrderKind OrderType { get; set; }
        public int Quantity { get; set; }
        public double EntryPrice { get; set; }
        public double StopPrice { get; set; }
        public string SignalName { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public double AvgFillPrice { get; set; } = 0;
        public int FilledQuantity { get; set; } = 0;
    }
}

