
namespace Orion.Models.Commodities.CommoditySwapLeg
{
    public class CommoditySwapLegParameters : ICommoditySwapLegParameters
    {
        #region Implementation of IFxLegParameters

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        /// <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote { get; set; }

        #endregion
    }
}