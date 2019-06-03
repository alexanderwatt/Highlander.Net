using System;

namespace Orion.Models.Commodities.CommoditySwapLeg
{
    public interface ICommoditySwapLegParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        /// <value>The multiplier.</value>
        Decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; set; }
    }
}