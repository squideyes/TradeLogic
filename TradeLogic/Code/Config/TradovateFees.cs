using System;
using System.Collections.Generic;

namespace TradeLogic
{
    /// <summary>
    /// Tradovate commission fees by symbol.
    /// These are the standard Tradovate fees as of 2025.
    /// </summary>
    public static class TradovateFees
    {
        private static readonly Dictionary<Symbol, decimal> FeePerContract = new Dictionary<Symbol, decimal>
        {
            // E-mini futures
            { Symbol.ES, 0.85m },      // E-mini S&P 500
            { Symbol.NQ, 0.85m },      // E-mini Nasdaq-100
            { Symbol.TY, 0.85m },      // 10-Year T-Note
            { Symbol.FV, 0.85m },      // 5-Year T-Note
            { Symbol.US, 0.85m },      // 30-Year T-Bond
            
            // Micro futures
            { Symbol.MES, 0.25m },     // Micro E-mini S&P 500
            { Symbol.MNQ, 0.25m },     // Micro E-mini Nasdaq-100
            { Symbol.MCL, 0.25m },     // Micro WTI Crude Oil
            
            // Full-size futures
            { Symbol.CL, 1.29m },      // WTI Crude Oil
            { Symbol.GC, 1.29m },      // Gold
            
            // Currencies
            { Symbol.EU, 0.85m },      // Euro FX
            { Symbol.JY, 0.85m },      // Japanese Yen
            { Symbol.BP, 0.85m }       // British Pound
        };

        /// <summary>
        /// Get the Tradovate commission fee for a symbol.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <returns>Commission fee per contract (round-turn)</returns>
        public static decimal GetFee(Symbol symbol)
        {
            if (FeePerContract.TryGetValue(symbol, out var fee))
                return fee;

            throw new ArgumentException($"No Tradovate fee defined for symbol: {symbol}", nameof(symbol));
        }

        /// <summary>
        /// Compute commission for a fill.
        /// </summary>
        /// <param name="fill">The fill</param>
        /// <param name="symbol">The symbol</param>
        /// <returns>Commission amount</returns>
        public static decimal ComputeCommission(Fill fill, Symbol symbol)
        {
            if (fill == null)
                throw new ArgumentNullException(nameof(fill));

            var feePerContract = GetFee(symbol);
            return Math.Abs(fill.Quantity) * feePerContract;
        }
    }
}

