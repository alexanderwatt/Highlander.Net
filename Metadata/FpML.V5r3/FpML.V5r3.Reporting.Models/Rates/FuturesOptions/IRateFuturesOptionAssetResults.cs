using System;
using Orion.Models.Futures;

namespace Orion.Models.Rates.FuturesOptions
{
    public enum RateFuturesOptionMetrics
    {
        ForwardDelta,
        NPV,  
        ImpliedQuote, 
        MarketQuote,
        IndexAtMaturity,
        PandL,
        InitialMargin,
        VariationMargin,
        OptionVolatility,
        VolatilityAtExpiry,
        AccrualFactor,
        ExpectedValue,
        RawValue,
        ForwardRate,
        ImpliedStrike,
        SpotDelta,
        DeltaR,
        Delta0,
        Delta1,
        Gamma0,
        Gamma1,
        Vega0,
        Theta0
    }

    public interface IRateFuturesOptionAssetResults : IFuturesOptionAssetResults
    {
        #region IRateFuturesOptionAssetResults Members

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal AccrualFactor { get; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        Decimal ExpectedValue { get; }

        /// <summary>
        /// Gets the forward rate.
        /// </summary>
        /// <value>The forward rate.</value>
        Decimal ForwardRate { get; }

        /// <summary>
        /// Gets the $ derivative with respect to the Rate.
        /// </summary>
        /// <value>The $ delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal Delta0 { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal Delta1 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        Decimal Gamma0 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        Decimal Gamma1 { get; }

        /// <summary>
        /// Gets the first derivative with respect to the Vol.
        /// </summary>
        /// <value>The vega wrt the forward rate.</value>
        Decimal Vega0 { get; }

        /// <summary>
        /// Gets the second derivative with respect to the Time.
        /// </summary>
        /// <value>The theta wrt the forward rate.</value>
        Decimal Theta0 { get; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal VolatilityAtExpiry { get; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        Decimal ConvexityAdjustment { get; }

        #endregion
    }
}