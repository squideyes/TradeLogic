namespace WickScalper.Common
{
    public class Future
    {
        public Symbol Symbol { get; set; }
        public string Name { get; set; }
        public decimal TickSize { get; set; }
        public decimal TickValue { get; set; }
        public decimal TicksPerPoint { get; set; }
        public int DecimalPlaces { get; set; }
        public string PriceFormat { get; set; }

        public bool IsPrice(decimal price) => (price % TickSize) == 0.0m;
    }
}
