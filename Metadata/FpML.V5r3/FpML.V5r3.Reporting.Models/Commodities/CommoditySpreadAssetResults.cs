using System;

namespace Orion.Models.Commodities
{
    public class CommoditySpreadAssetResults : ICommoditySpreadAssetResults
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
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        public Decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote { get; set; }

        #endregion
    }
}