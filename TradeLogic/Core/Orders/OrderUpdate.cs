namespace TradeLogic
{
    public sealed class OrderUpdate
    {
        public string ClientOrderId { get; private set; }
        public string VenueOrderId { get; private set; }
        public OrderStatus Status { get; private set; }
        public string Reason { get; private set; }

        public OrderUpdate(string clientOrderId, string venueOrderId, OrderStatus status, string reason)
        {
            ClientOrderId = clientOrderId;
            VenueOrderId = venueOrderId;
            Status = status;
            Reason = reason;
        }
    }
}
