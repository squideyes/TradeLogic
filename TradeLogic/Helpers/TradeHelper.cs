using TradeLogic.Models;

namespace TradeLogic.Utilities
{
    internal static class TradeHelper
    {
        public static double CalculatePnL(double entryPrice, 
            double exitPrice, OrderAction entryAction,
            int positionQuantity, double tickSize, double tickValue)
        {
            double ticks;

            if (entryAction == OrderAction.Buy)
                ticks = (exitPrice - entryPrice) / tickSize;
            else
                ticks = (entryPrice - exitPrice) / tickSize;

            return ticks * tickValue * positionQuantity;
        }

        public static string FormatPrice(double price)=>
            price.ToString("F2");

        public static string FormatCurrency(double amount)=>
            amount.ToString("F2");

        public static string FormatDollar(double amount)=>
            "$" + amount.ToString("F2");

        public static double CalculateStopPrice(double entryPrice, 
            int riskPoints, OrderAction entryAction, double tickSize)
        {
            if (entryAction == OrderAction.Buy)
                return entryPrice - (riskPoints * tickSize);
            else
                return entryPrice + (riskPoints * tickSize);
        }

        public static double CalculateProfitPrice(double entryPrice, 
            int rewardPoints, OrderAction entryAction, double tickSize)
        {
            if (entryAction == OrderAction.Buy)
                return entryPrice + (rewardPoints * tickSize);
            else
                return entryPrice - (rewardPoints * tickSize);
        }
    }
}

