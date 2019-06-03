using System;

namespace Orion.Models.Commodities
{
    public enum CommoditySpreadMetrics
    {
        IndexAtMaturity, ImpliedQuote, MarketQuote }

    public interface ICommoditySpreadAssetResults
    {
        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        Decimal IndexAtMaturity { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }
    }
}