using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public class RateSpreadAssetResults : IRateSpreadAssetResults
    {
        /// <summary>
        /// Gets the forward delta the fixed rate.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        #region IRateAssetResults Members

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the accrual factor
        /// </summary>
        public decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity { get; set; }

        /// <summary>
        /// Gets the discount factor at start.
        /// </summary>
        /// <value>The discount factor at start.</value>
        public Decimal DiscountFactorAtStart { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote { get; set; }

        #endregion
    }
}