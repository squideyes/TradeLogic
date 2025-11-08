using System.Collections.Generic;
using System.Linq;

namespace WickScalper.Common
{
    public static class KnownFutures
    {
        private static readonly Dictionary<Symbol, Future> futures =
            GetFutures();

        static Dictionary<Symbol, Future> GetFutures()
        {
            var futures = new List<Future>()
            {
                new Future
                {
                    Symbol = Symbol.BP,
                    Name = "British Pound",
                    TickSize = 0.0001m,
                    TickValue = 6.25m,
                    TicksPerPoint = 10000,
                    DecimalPlaces = 4,
                    PriceFormat = "0.0000"
                },
                new Future
                {
                    Symbol = Symbol.CL,
                    Name = "Crude Oil",
                    TickSize = 0.01m,
                    TickValue = 10.00m,
                    TicksPerPoint = 100,
                    DecimalPlaces = 2,
                    PriceFormat = "0.00"
                }, 
                new Future
                {
                    Symbol = Symbol.ES,
                    Name = "E-mini S&P 500",
                    TickSize = 0.25m,
                    TickValue = 12.50m,
                    TicksPerPoint = 4,
                    DecimalPlaces = 2,
                    PriceFormat = "0.00"
                },
                new Future
                {
                    Symbol = Symbol.EU,
                    Name = "Euro FX",
                    TickSize = 0.00005m,
                    TickValue = 6.25m,
                    TicksPerPoint = 20000,
                    DecimalPlaces = 5,
                    PriceFormat = "0.00000"
                },
                new Future
                {
                    Symbol = Symbol.FV,
                    Name = "5-Year Treasury Note",
                    TickSize = 0.03125m,
                    TickValue = 7.8125m,
                    TicksPerPoint = 32,
                    DecimalPlaces = 5,
                    PriceFormat = "0.00000"
                },
                new Future
                {
                    Symbol = Symbol.GC,
                    Name = "Gold",
                    TickSize = 0.10m,
                    TickValue = 10.00m,
                    TicksPerPoint = 10,
                    DecimalPlaces = 1,
                    PriceFormat = "0.0"
                },
                new Future
                {
                    Symbol = Symbol.JY,
                    Name = "Japanese Yen",
                    TickSize = 0.0000005m,
                    TickValue = 6.25m,
                    TicksPerPoint = 2000000,
                    DecimalPlaces = 7,
                    PriceFormat = "0.0000000"
                },
                new Future
                {
                    Symbol = Symbol.MES,
                    Name = "Micro E-mini S&P 500",
                    TickSize = 0.25m,
                    TickValue = 1.25m,
                    TicksPerPoint = 4,
                    DecimalPlaces = 2,
                    PriceFormat = "0.00"
                },
                new Future
                {
                    Symbol = Symbol.MNQ,
                    Name = "Micro E-mini Nasdaq-100",
                    TickSize = 0.25m,
                    TickValue = 0.50m,
                    TicksPerPoint = 4,
                    DecimalPlaces = 2,
                    PriceFormat = "0.00"
                },
                new Future
                {
                    Symbol = Symbol.MCL,
                    Name = "Micro Crude Oil",
                    TickSize = 0.01m,
                    TickValue = 1.00m,
                    TicksPerPoint = 100,
                    DecimalPlaces = 2,
                    PriceFormat = "0.00"
                },
                new Future
                {
                    Symbol = Symbol.NQ,
                    Name = "E-mini Nasdaq-100",
                    TickSize = 0.25m,
                    TickValue = 5.00m,
                    TicksPerPoint = 4,
                    DecimalPlaces = 2,
                    PriceFormat = "0.00"
                },
                new Future
                {
                    Symbol = Symbol.TY,
                    Name = "10-Year Treasury Note",
                    TickSize = 0.015625m,
                    TickValue = 15.625m,
                    TicksPerPoint = 64,
                    DecimalPlaces = 6,
                    PriceFormat = "0.000000"
                },
                new Future
                {
                    Symbol = Symbol.US,
                    Name = "30-Year Treasury Bond",
                    TickSize = 0.03125m,
                    TickValue = 31.25m,
                    TicksPerPoint = 32,
                    DecimalPlaces = 5,
                    PriceFormat = "0.00000"
                }
            };

            return futures.ToDictionary(f => f.Symbol);
        }

        public static Future GetFuture(Symbol symbol) => futures[symbol];
    }
}
