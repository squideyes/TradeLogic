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

        /// <summary>
        /// Validates the configuration for correctness.
        /// Throws ArgumentException if any property is invalid.
        /// </summary>
        public void Validate()
        {
            if (TickSize <= 0)
                throw new System.ArgumentException("TickSize must be greater than 0", nameof(TickSize));

            if (PointValue <= 0)
                throw new System.ArgumentException("PointValue must be greater than 0", nameof(PointValue));

            if (string.IsNullOrWhiteSpace(IdPrefix))
                throw new System.ArgumentException("IdPrefix cannot be null or empty", nameof(IdPrefix));

            if (SlippageToleranceTicks < 0)
                throw new System.ArgumentException("SlippageToleranceTicks cannot be negative", nameof(SlippageToleranceTicks));
        }
    }
}
