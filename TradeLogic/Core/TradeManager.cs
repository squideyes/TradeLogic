using System;
using TradeLogic.Models;

namespace TradeLogic.Core
{
    /// <summary>
    /// Orchestrates the complete trade lifecycle: entry submission, position management, and exit handling
    /// Encapsulates FokOrderManager and OcoExitManager to reduce strategy complexity
    /// Platform-agnostic - uses only custom enums and primitives
    /// </summary>
    public class TradeManager
    {
        private FokOrderManager fokOrderManager;
        private OcoExitManager ocoExitManager;
        private TradeState tradeState;

        public event Action<OrderAction, double, int, string> OnEntryOrder;
        public event Action<string, double, int> OnFilled;
        public event Action<string, OrderStatus, double, int, string> OnOrderStatusChanged;
        public event Action<string> OnTradeReset;
        public event Action<string> OnError;

        public TradeState State { get { return tradeState; } }
        public bool HasPosition { get { return tradeState.HasPosition; } }
        public bool HasEntryOrder { get { return fokOrderManager.HasEntryOrder; } }
        public bool HasExitOrders { get { return ocoExitManager.HasExitOrders; } }

        public TradeManager(Func<OrderAction, int, double, string, OrderKind, double, string> submitEntryCallback,
            Func<string, bool> cancelCallback,
            Func<OrderAction, int, double, double, string, string, string> submitExitCallback)
        {
            this.fokOrderManager = new FokOrderManager(submitEntryCallback, cancelCallback);
            this.ocoExitManager = new OcoExitManager(submitExitCallback, cancelCallback);
            this.tradeState = new TradeState();
        }

        /// <summary>
        /// Submits entry order with OCO exits atomically (all three orders at once)
        /// Entry can be Market, Stop, Limit, or StopLimit
        /// </summary>
        public void SubmitTrade(OrderAction action, int quantity, double entryPrice, string signalName,
            int riskPoints, int rewardPoints, double tickSize,
            OrderKind orderType = OrderKind.Limit, double entryStopPrice = 0)
        {
            try
            {
                // Pre-calculate exit prices
                tradeState.EntryPrice = entryPrice;
                tradeState.EntryAction = action;

                double stopPrice = tradeState.CalculateStopPrice(riskPoints, tickSize);
                double profitPrice = tradeState.CalculateProfitPrice(rewardPoints, tickSize);

                // Submit entry order
                fokOrderManager.SubmitFOKEntry(action, quantity, entryPrice, signalName, orderType, entryStopPrice);

                // Submit OCO exits immediately
                ocoExitManager.SubmitOCOExits(
                    action,
                    quantity,
                    entryPrice,
                    stopPrice,
                    profitPrice,
                    riskPoints,
                    rewardPoints
                );

                // Raise event with order details
                OnEntryOrder?.Invoke(action, entryPrice, quantity, signalName);
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Error submitting trade: " + ex.Message);
                throw new Exception("Error submitting trade: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if entry order has filled and updates trade state
        /// Returns true if entry just filled
        /// currentBar parameter must be provided by the strategy
        /// </summary>
        public bool CheckEntryFilled(int currentBar)
        {
            if (fokOrderManager.IsEntryFilled && !tradeState.HasPosition)
            {
                double fillPrice = fokOrderManager.GetEntryFillPrice();
                int quantity = fokOrderManager.GetFilledQuantity();

                tradeState.InitializeTrade(
                    tradeState.EntryAction,
                    fillPrice,
                    quantity,
                    currentBar
                );

                // Raise event
                OnFilled?.Invoke("Entry", fillPrice, quantity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cancels entry order if unfilled
        /// </summary>
        public bool CancelEntry()
        {
            return fokOrderManager.CancelEntryOrder();
        }

        /// <summary>
        /// Checks if stop loss has filled
        /// </summary>
        public bool IsStopFilled()
        {
            if (ocoExitManager.IsStopFilled)
            {
                OnFilled?.Invoke("StopLoss", ocoExitManager.StopOrder.AvgFillPrice, ocoExitManager.StopOrder.FilledQuantity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if take profit has filled
        /// </summary>
        public bool IsProfitFilled()
        {
            if (ocoExitManager.IsProfitFilled)
            {
                OnFilled?.Invoke("TakeProfit", ocoExitManager.ProfitOrder.AvgFillPrice, ocoExitManager.ProfitOrder.FilledQuantity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cancels all pending orders (entry and exits)
        /// </summary>
        public void CancelAllOrders()
        {
            if (fokOrderManager.HasEntryOrder && !fokOrderManager.IsEntryFilled)
            {
                fokOrderManager.CancelEntryOrder();
            }

            if (ocoExitManager.HasExitOrders)
            {
                ocoExitManager.CancelAllExits();
            }
        }

        /// <summary>
        /// Updates order status and raises OnOrderStatusChanged event
        /// Called by adapter when platform notifies of order state changes
        /// </summary>
        public void UpdateOrderStatus(string orderId, OrderStatus status, double price, int filledQty, string details = "")
        {
            // Update managers
            if (fokOrderManager.CurrentEntryOrder != null && fokOrderManager.CurrentEntryOrder.OrderId == orderId)
            {
                fokOrderManager.UpdateOrderStatus(orderId, status, price, filledQty);
            }
            else if (ocoExitManager.StopOrder != null && ocoExitManager.StopOrder.OrderId == orderId)
            {
                ocoExitManager.UpdateOrderStatus(orderId, status, price, filledQty);
            }
            else if (ocoExitManager.ProfitOrder != null && ocoExitManager.ProfitOrder.OrderId == orderId)
            {
                ocoExitManager.UpdateOrderStatus(orderId, status, price, filledQty);
            }

            // Raise event with details
            OnOrderStatusChanged?.Invoke(orderId, status, price, filledQty, details);
        }

        /// <summary>
        /// Resets all trade state and order references
        /// </summary>
        public void Reset()
        {
            fokOrderManager.ResetEntryOrder();
            ocoExitManager.ResetExits();
            tradeState.Reset();

            // Raise event
            OnTradeReset?.Invoke("Trade reset");
        }
    }
}

