using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public enum RateSpreadMetrics
    {
        DiscountFactorAtMaturity, DiscountFactorAtStart, AccrualFactor, 
        ImpliedQuote, MarketQuote }

    public interface IRateSpreadAssetResults
    {
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
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }

        /// <summary>
        /// Gets the discount factor at start.
        /// </summary>
        /// <value>The discount factor at start.</value>
        Decimal DiscountFactorAtStart { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }
    }
}