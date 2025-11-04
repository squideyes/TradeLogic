namespace TradeLogic
{
    public enum OrderStatus
    {
        New = 1,
        Accepted,
        Rejected,
        Canceled,
        Working,
        Filled,
        PartiallyFilled,
        Expired
    }
}
