namespace TradeLogic.Models
{
    public class TradeState
    {
        public bool HasPosition { get; set; } = false;
        public double EntryPrice { get; set; } = 0;
        public OrderAction EntryAction { get; set; } = OrderAction.Buy;
        public int PositionQuantity { get; set; } = 0;
        public int EntryBar { get; set; } = 0;

        public void InitializeTrade(OrderAction action, 
            double price, int quantity, int currentBar)
        {
            EntryAction = action;
            EntryPrice = price;
            PositionQuantity = quantity;
            EntryBar = currentBar;
            HasPosition = true;
        }

        public void Reset()
        {
            HasPosition = false;
            EntryPrice = 0;
            EntryAction = OrderAction.Buy;
            PositionQuantity = 0;
            EntryBar = 0;
        }

        public OrderAction GetExitAction()
        {
            return EntryAction == OrderAction.Buy 
                ? OrderAction.Sell : OrderAction.Buy;
        }

        public double CalculateStopPrice(int riskPoints, double tickSize)
        {
            return EntryAction == OrderAction.Buy
                ? EntryPrice - (riskPoints * tickSize)
                : EntryPrice + (riskPoints * tickSize);
        }

        public double CalculateProfitPrice(int rewardPoints, double tickSize)
        {
            return EntryAction == OrderAction.Buy
                ? EntryPrice + (rewardPoints * tickSize)
                : EntryPrice - (rewardPoints * tickSize);
        }
    }
}

