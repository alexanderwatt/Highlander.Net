using System;

namespace Orion.Models.ForeignExchange
{
    public enum FxOptionMetrics { VolatilityAtExpiry, NPV, AccrualFactor,
        ImpliedQuote, Delta0, Delta1, Gamma0, Gamma1, MarketQuote
    }

    public interface IFxOptionAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal AccrualFactor { get; }

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
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal VolatilityAtExpiry { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }
    }
}