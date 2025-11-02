using System;
using TradeLogic.Models;
using static TradeLogic.Models.OrderAction;
using static TradeLogic.Models.OrderStatus;

namespace TradeLogic.Core
{
    internal class OcoExitManager
    {
        private Order stopOrder = null;
        private Order profitOrder = null;
        private string ocoGroupName = "";

        private readonly Func<OrderAction, int, double, double,
            string, string, string> submitExitCallback;

        private readonly Func<string, bool> cancelOrderCallback;

        public bool HasExitOrders => 
            stopOrder != null && profitOrder != null;

        public bool IsStopFilled => 
            stopOrder != null && stopOrder.Status == OrderStatus.Filled;

        public bool IsProfitFilled => 
            profitOrder != null && profitOrder.Status == OrderStatus.Filled;

        public Order StopOrder => stopOrder;

        public Order ProfitOrder => profitOrder;

        public OcoExitManager(Func<OrderAction, int, double, double, string, string, string> submitOrderCallback, Func<string, bool> cancelOrderCallback)
        {
            this.submitExitCallback = submitOrderCallback;
            this.cancelOrderCallback = cancelOrderCallback;
        }

        public bool SubmitOCOExits(
            OrderAction entryAction,
            int quantity,
            double entryPrice,
            double stopPrice,
            double profitPrice,
            int riskPoints,
            int rewardPoints)
        {
            try
            {
                ocoGroupName = Guid.NewGuid().ToString();

                var exitAction = (entryAction == Buy) ? Sell : Buy;

                string stopOrderId = submitExitCallback?.Invoke(
                    exitAction,
                    quantity,
                    0, 
                    stopPrice,
                    ocoGroupName,
                    entryAction == Buy ? "Stop Loss" : "Short Stop Loss"
                );

                if (string.IsNullOrEmpty(stopOrderId))
                {
                    throw new Exception(
                        "Stop loss order submission failed");
                }

                stopOrder = new Order
                {
                    OrderId = stopOrderId,
                    Action = exitAction,
                    OrderType = OrderKind.Stop,
                    Quantity = quantity,
                    EntryPrice = entryPrice,
                    StopPrice = stopPrice,
                    SignalName = (entryAction == Buy ? "Stop Loss" : "Short Stop Loss"),
                    Status = Submitted
                };

                string profitOrderId = submitExitCallback?.Invoke(
                    exitAction,
                    quantity,
                    profitPrice,
                    0,
                    ocoGroupName,
                    entryAction == Buy ? "Take Profit" : "Short Take Profit"
                );

                if (string.IsNullOrEmpty(profitOrderId))
                {
                    throw new Exception("Take profit order submission failed");
                }

                profitOrder = new Order
                {
                    OrderId = profitOrderId,
                    Action = exitAction,
                    OrderType = OrderKind.Limit,
                    Quantity = quantity,
                    EntryPrice = entryPrice,
                    StopPrice = profitPrice,
                    SignalName = (entryAction == Buy ? "Take Profit" : "Short Take Profit"),
                    Status = Submitted
                };

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error submitting OCO exits: " + ex.Message);
            }
        }


        public void UpdateOrderStatus(string orderId, OrderStatus status, double avgFillPrice = 0, int filledQty = 0)
        {
            if (stopOrder != null && stopOrder.OrderId == orderId)
            {
                stopOrder.Status = status;
                stopOrder.AvgFillPrice = avgFillPrice;
                stopOrder.FilledQuantity = filledQty;
            }
            else if (profitOrder != null && profitOrder.OrderId == orderId)
            {
                profitOrder.Status = status;
                profitOrder.AvgFillPrice = avgFillPrice;
                profitOrder.FilledQuantity = filledQty;
            }
        }

        public void CancelAllExits()
        {
            try
            {
                if (stopOrder != null &&
                    (stopOrder.Status == Working 
                        || stopOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(stopOrder.OrderId);

                    stopOrder.Status = Cancelled;
                }

                if (profitOrder != null &&
                    (profitOrder.Status == Working 
                        || profitOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(profitOrder.OrderId);
                    profitOrder.Status = Cancelled;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelling exit orders: " + ex.Message);
            }
        }

        public void CancelStop()
        {
            try
            {
                if (stopOrder != null &&
                    (stopOrder.Status == Working ||
                     stopOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(stopOrder.OrderId);
                    stopOrder.Status = Cancelled;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelling stop order: " + ex.Message);
            }
        }

        public void CancelProfit()
        {
            try
            {
                if (profitOrder != null &&
                    (profitOrder.Status == Working ||
                     profitOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(profitOrder.OrderId);
                    profitOrder.Status = Cancelled;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelling profit order: " + ex.Message);
            }
        }

        public string GetExitStatus()
        {
            string status = "";

            if (stopOrder != null)
                status += string.Format("Stop: {0} @ {1} | ", stopOrder.Status, stopOrder.StopPrice);

            if (profitOrder != null)
                status += string.Format("Profit: {0} @ {1}", profitOrder.Status, profitOrder.StopPrice);

            return status;
        }

        public string GetFilledExitType()
        {
            if (IsStopFilled) 
                return "Stop Loss";

            if (IsProfitFilled) 
                return "Take Profit";
            
            return "None";
        }

        public void ResetExits()
        {
            CancelAllExits();

            stopOrder = null;
            profitOrder = null;
            ocoGroupName = "";
        }
    }
}

