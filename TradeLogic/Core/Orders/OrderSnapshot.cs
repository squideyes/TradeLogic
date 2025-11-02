namespace TradeLogic
{
    internal sealed class OrderSnapshot
    {
        public OrderSpec Spec { get; private set; }
        public OrderStatus Status { get; private set; }
        public int FilledQuantity { get; private set; }
        public decimal? AvgFillPrice { get; private set; }
        public string RejectOrCancelReason { get; private set; }

        public OrderSnapshot(OrderSpec spec, OrderStatus status, int filledQty, decimal? avgFillPrice, string reason)
        {
            Spec = spec;
            Status = status;
            FilledQuantity = filledQty;
            AvgFillPrice = avgFillPrice;
            RejectOrCancelReason = reason;
        }

        public OrderSnapshot With(
            OrderStatus status,
            int? filledQty = null,
            decimal? avgFillPrice = null,
            string reason = null)
        {
            return new OrderSnapshot(
                Spec,
                status,
                filledQty ?? FilledQuantity,
                avgFillPrice ?? AvgFillPrice,
                reason ?? RejectOrCancelReason);
        }
    }
}
