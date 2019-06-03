using System;

namespace Orion.Models.Commodities.CommoditySwapLeg
{
    public enum CommoditySwapLegInstrumentMetrics
    {
        ImpliedQuote
        , MarketQuote
        , BreakEvenIndex
    }

    public interface ICommoditySwapLegInstrumentResults
    {
        /// <summary>
        /// Gets the implied fx rate.
        /// </summary>
        /// <value>The implied fx rate.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market fx rate.
        /// </summary>
        /// <value>The market fx rate.</value>
        Decimal MarketQuote { get; }
        
        /// <summary>
        /// Gets the break even index.
        /// </summary>
        /// <value>The foreign currency NPV.</value>
        Decimal BreakEvenIndex { get; }
    }
}