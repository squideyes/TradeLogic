namespace TradeLogic.Models
{
    public enum OrderStatus
    {
        Pending = 1,
        Submitted,
        Working,
        PartiallyFilled,
        Filled,
        Cancelled,
        Rejected
    }
}

