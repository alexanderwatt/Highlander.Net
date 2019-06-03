using System;

namespace Orion.Models.Futures
{
    public enum FuturesOptionMetrics
    {
        ForwardDelta,
        NPV,
        NPVChange,
        ImpliedQuote,
        MarketQuote,
        IndexAtMaturity,
        PandL,
        InitialMargin,
        VariationMargin,
        ImpliedStrike,
        OptionVolatility,
        SpotDelta
    }

    public interface IFuturesOptionAssetResults : IFuturesAssetResults
    {
        /// <summary>
        /// Gets the strike.
        /// </summary>
        Decimal ImpliedStrike { get; }

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        Decimal OptionVolatility { get; }

        /// <summary>
        /// Gets the spot delta.
        /// </summary>
        /// <value>The spot delta.</value>
        Decimal SpotDelta { get; }
    }
}