using System;
using TradeLogic.Models;

namespace TradeLogic.Core
{
    internal class OrderManager
    {
        private Order currentEntryOrder = null;

        private readonly Func<OrderAction, int, double, string,
            OrderKind, double, string> submitOrderCallback;

        private readonly Func<string, bool> cancelOrderCallback;

        public Order CurrentEntryOrder => currentEntryOrder;
        public bool HasEntryOrder => currentEntryOrder != null;
        public bool IsEntryFilled => currentEntryOrder != null
            && currentEntryOrder.Status == OrderStatus.Filled;

        public OrderManager(Func<OrderAction, int, double,
            string, OrderKind, double, string> submitOrderCallback,
                Func<string, bool> cancelOrderCallback)
        {
            this.submitOrderCallback = submitOrderCallback;
            this.cancelOrderCallback = cancelOrderCallback;
        }

        public string SubmitFOKEntry(
            OrderAction action,
            int quantity,
            double entryPrice,
            string signalName,
            OrderKind orderType = OrderKind.Limit,
            double stopPrice = 0)
        {
            try
            {
                var orderId = submitOrderCallback?.Invoke(action,
                    quantity, entryPrice, signalName, orderType, stopPrice);

                if (string.IsNullOrEmpty(orderId))
                {
                    throw new Exception(
                        "Order submission failed - no order ID returned");
                }

                currentEntryOrder = new Order
                {
                    OrderId = orderId,
                    Action = action,
                    OrderType = orderType,
                    Quantity = quantity,
                    EntryPrice = entryPrice,
                    StopPrice = stopPrice,
                    SignalName = signalName,
                    Status = OrderStatus.Submitted
                };

                return orderId;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error submitting FOK entry: " + ex.Message);
            }
        }

        public bool CancelEntryOrder()
        {
            try
            {
                if (currentEntryOrder == null)
                    return false;

                if (currentEntryOrder.Status == OrderStatus.Working
                    || currentEntryOrder.Status == OrderStatus.Submitted)
                {
                    bool cancelled = cancelOrderCallback
                        ?.Invoke(currentEntryOrder.OrderId) ?? false;

                    if (cancelled)
                        currentEntryOrder.Status = OrderStatus.Cancelled;

                    return cancelled;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error cancelling entry order: " + ex.Message);
            }
        }

        public void UpdateOrderStatus(string orderId,
            OrderStatus status, double avgFillPrice = 0, int filledQty = 0)
        {
            if (currentEntryOrder != null
                && currentEntryOrder.OrderId == orderId)
            {
                currentEntryOrder.Status = status;
                currentEntryOrder.AvgFillPrice = avgFillPrice;
                currentEntryOrder.FilledQuantity = filledQty;
            }
        }

        public double GetEntryFillPrice() =>
            currentEntryOrder?.AvgFillPrice ?? 0;

        public int GetFilledQuantity() =>
            currentEntryOrder?.FilledQuantity ?? 0;

        public void ResetEntryOrder() =>
            currentEntryOrder = null;

        public string GetOrderStatus()
        {
            if (currentEntryOrder == null)
                return "No entry order";

            return string.Format(
                "Entry Order - Status: {0}, Filled: {1}, Price: {2}",
                    currentEntryOrder.Status,
                    currentEntryOrder.FilledQuantity,
                    currentEntryOrder.AvgFillPrice
            );
        }
    }
}

