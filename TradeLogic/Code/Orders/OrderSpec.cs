using System;

namespace TradeLogic
{
    #region Config

    #endregion


    public sealed class OrderSpec
    {
        public string ClientOrderId { get; private set; }
        public string VenueOrderId { get; private set; }
        public Side Side { get; private set; }
        public OrderType OrderType { get; private set; }
        public int Quantity { get; private set; }
        public TimeInForce TimeInForce { get; private set; }
        public decimal? LimitPrice { get; private set; }
        public decimal? StopPrice { get; private set; }
        public DateTime? GoodTillTimeUtc { get; private set; }
        public bool IsEntry { get; private set; }
        public bool IsExit { get; private set; }
        public string OcoGroupId { get; private set; }

        public OrderSpec(
            string clientOrderId,
            Side side,
            OrderType orderType,
            int quantity,
            TimeInForce tif,
            decimal? limitPrice,
            decimal? stopPrice,
            DateTime? gttUtc,
            bool isEntry,
            bool isExit,
            string ocoGroupId)
        {
            ClientOrderId = clientOrderId;
            Side = side;
            OrderType = orderType;
            Quantity = quantity;
            TimeInForce = tif;
            LimitPrice = limitPrice;
            StopPrice = stopPrice;
            GoodTillTimeUtc = gttUtc;
            IsEntry = isEntry;
            IsExit = isExit;
            OcoGroupId = ocoGroupId;
        }

        public OrderSpec WithVenueOrderId(string venueOrderId)
        {
            var clone = Clone();
            clone.VenueOrderId = venueOrderId;
            return clone;
        }

        public OrderSpec Clone()
        {
            var c = new OrderSpec(
                ClientOrderId, Side, OrderType, Quantity, TimeInForce,
                LimitPrice, StopPrice, GoodTillTimeUtc, IsEntry, IsExit, OcoGroupId);
            c.VenueOrderId = VenueOrderId;
            return c;
        }
    }
}
