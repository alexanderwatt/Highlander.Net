using System;

namespace Orion.Models.Assets
{
    public enum RateMetrics { DiscountFactorAtMaturity, NPV, AccrualFactor, 
        ImpliedQuote, ConvexityAdjustment, DeltaR, MarketQuote, NPVChange}

    public interface IRateAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        Decimal NPVChange { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        Decimal AccrualFactor { get; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        /// <value>The convexity adjustment.</value>
        Decimal ConvexityAdjustment { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }
    }
}