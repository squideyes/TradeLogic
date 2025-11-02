using System;
using TradeLogic.Models;
using static TradeLogic.Models.OrderAction;
using static TradeLogic.Models.OrderStatus;

namespace TradeLogic.Core
{
    internal class OcoExitManager
    {
        private Order stopLossOrder = null;
        private Order takeProfitOrder = null;
        private string ocoId = "";

        private readonly Func<OrderAction, int, double, double,
            string, string, string> submitExitCallback;

        private readonly Func<string, bool> cancelOrderCallback;

        public bool HasExitOrders => 
            stopLossOrder != null && takeProfitOrder != null;

        public bool IsStopFilled => 
            stopLossOrder != null && stopLossOrder.Status == Filled;

        public bool IsProfitFilled => 
            takeProfitOrder != null && takeProfitOrder.Status == Filled;

        public Order StopOrder => stopLossOrder;

        public Order ProfitOrder => takeProfitOrder;

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
                ocoId = Guid.NewGuid().ToString();

                var exitAction = (entryAction == Buy) ? Sell : Buy;

                var stopOrderId = submitExitCallback?.Invoke(
                    exitAction,
                    quantity,
                    0, 
                    stopPrice,
                    ocoId,
                    entryAction == Buy ? "Stop Loss" : "Short Stop Loss");

                if (string.IsNullOrEmpty(stopOrderId))
                {
                    throw new Exception(
                        "Stop loss order submission failed");
                }

                stopLossOrder = new Order
                {
                    OrderId = stopOrderId,
                    Action = exitAction,
                    OrderType = OrderKind.Stop,
                    Quantity = quantity,
                    EntryPrice = entryPrice,
                    StopPrice = stopPrice,
                    SignalName = (entryAction == Buy ? "Stop Loss" : "Short Stop Loss"),
                    Status = Submitted};

                string profitOrderId = submitExitCallback?.Invoke(
                    exitAction,
                    quantity,
                    profitPrice,
                    0,
                    ocoId,
                    entryAction == Buy ? "Take Profit" : "Short Take Profit");

                if (string.IsNullOrEmpty(profitOrderId))
                {
                    throw new Exception(
                        "Take profit order submission failed");
                }

                takeProfitOrder = new Order
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
            if (stopLossOrder != null && stopLossOrder.OrderId == orderId)
            {
                stopLossOrder.Status = status;
                stopLossOrder.AvgFillPrice = avgFillPrice;
                stopLossOrder.FilledQuantity = filledQty;
            }
            else if (takeProfitOrder != null && takeProfitOrder.OrderId == orderId)
            {
                takeProfitOrder.Status = status;
                takeProfitOrder.AvgFillPrice = avgFillPrice;
                takeProfitOrder.FilledQuantity = filledQty;
            }
        }

        public void CancelAllExits()
        {
            try
            {
                if (stopLossOrder != null &&
                    (stopLossOrder.Status == Working 
                        || stopLossOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(stopLossOrder.OrderId);

                    stopLossOrder.Status = Cancelled;
                }

                if (takeProfitOrder != null &&
                    (takeProfitOrder.Status == Working 
                        || takeProfitOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(takeProfitOrder.OrderId);
                    takeProfitOrder.Status = Cancelled;
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
                if (stopLossOrder != null &&
                    (stopLossOrder.Status == Working ||
                     stopLossOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(stopLossOrder.OrderId);
                    stopLossOrder.Status = Cancelled;
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
                if (takeProfitOrder != null &&
                    (takeProfitOrder.Status == Working ||
                     takeProfitOrder.Status == Submitted))
                {
                    cancelOrderCallback?.Invoke(takeProfitOrder.OrderId);
                    takeProfitOrder.Status = Cancelled;
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

            if (stopLossOrder != null)
                status += string.Format("Stop: {0} @ {1} | ", stopLossOrder.Status, stopLossOrder.StopPrice);

            if (takeProfitOrder != null)
                status += string.Format("Profit: {0} @ {1}", takeProfitOrder.Status, takeProfitOrder.StopPrice);

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

            stopLossOrder = null;
            takeProfitOrder = null;
            ocoId = "";
        }
    }
}

