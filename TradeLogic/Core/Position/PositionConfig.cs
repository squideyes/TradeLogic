namespace TradeLogic
{
    internal sealed class PositionConfig
    {
        public string Symbol { get; set; }
        public decimal TickSize { get; set; }
        public decimal PointValue { get; set; }
        public int MinQty { get; set; }
        public SessionConfig Session { get; set; }
        public string IdPrefix { get; set; }
        public ExitAtSessionEndMode ExitAtSessionEndMode { get; set; }
        public int MarketableLimitOffsetTicks { get; set; }
        public bool UseStopLimitForSL { get; set; }
        public bool UseMarketForManualFlat { get; set; }
        public int SlippageToleranceTicks { get; set; }

        public PositionConfig()
        {
            Symbol = "SYM";
            TickSize = 0.01m;
            PointValue = 1m;
            MinQty = 1;
            Session = new SessionConfig();
            IdPrefix = "PM";
            ExitAtSessionEndMode = ExitAtSessionEndMode.GTDMarket;
            MarketableLimitOffsetTicks = 1;
            UseStopLimitForSL = false;
            UseMarketForManualFlat = true;
            SlippageToleranceTicks = 4;
        }
    }
}
