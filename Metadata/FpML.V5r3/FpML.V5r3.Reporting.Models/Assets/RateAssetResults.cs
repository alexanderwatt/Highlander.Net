using System;

namespace Orion.Models.Assets
{
    public class RateAssetResults : IRateAssetResults
    {
        #region IRateAssetResults Members

        /// <summary>
        /// Gets the forward delta the fixed rate.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal NPV { get; set; }

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public Decimal NPVChange { get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the delta wrt the fixed rate.
        /// </summary>
        public decimal DeltaR { get; set; }

        /// <summary>
        /// Gets the accrual factor
        /// </summary>
        public decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public decimal ConvexityAdjustment { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote { get; set; }

        #endregion
    }
}