namespace TradeLogic
{
    public sealed class PositionConfig
    {
        public Symbol Symbol { get; set; }
        public decimal TickSize { get; set; }
        public decimal PointValue { get; set; }
        public string IdPrefix { get; set; }
        public int SlippageToleranceTicks { get; set; }

        public PositionConfig()
        {
            Symbol = TradeLogic.Symbol.ES;
            TickSize = 0.01m;
            PointValue = 1m;
            IdPrefix = "PM";
            SlippageToleranceTicks = 1;
        }
    }
}
