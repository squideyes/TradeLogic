using System;

namespace TradeLogic
{
    /// <summary>
    /// Helper methods for working with Symbol enum.
    /// </summary>
    public static class SymbolHelper
    {
        /// <summary>
        /// Parse a symbol string (e.g., from NinjaTrader Instrument.MasterInstrument.Name) to Symbol enum.
        /// </summary>
        /// <param name="symbolString">Symbol string like "ES", "NQ", "CL", etc.</param>
        /// <returns>Parsed Symbol enum value</returns>
        /// <exception cref="ArgumentException">If symbol string is not recognized</exception>
        public static Symbol Parse(string symbolString)
        {
            if (string.IsNullOrWhiteSpace(symbolString))
                throw new ArgumentException("Symbol string cannot be null or empty", nameof(symbolString));

            // Remove any whitespace and convert to uppercase
            var normalized = symbolString.Trim().ToUpperInvariant();

            // Try to parse as enum
            if (Enum.TryParse<Symbol>(normalized, ignoreCase: true, out var result))
                return result;

            throw new ArgumentException($"Unknown symbol: {symbolString}", nameof(symbolString));
        }

        /// <summary>
        /// Try to parse a symbol string to Symbol enum.
        /// </summary>
        /// <param name="symbolString">Symbol string like "ES", "NQ", "CL", etc.</param>
        /// <param name="symbol">Parsed Symbol enum value if successful</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryParse(string symbolString, out Symbol symbol)
        {
            symbol = default;

            if (string.IsNullOrWhiteSpace(symbolString))
                return false;

            var normalized = symbolString.Trim().ToUpperInvariant();
            return Enum.TryParse<Symbol>(normalized, ignoreCase: true, out symbol);
        }
    }
}

