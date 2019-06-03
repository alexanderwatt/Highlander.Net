
namespace Orion.Models.Commodities.CommoditySwapLeg
{
    public class CommoditySwapLegInstrumentResults : ICommoditySwapLegInstrumentResults
    {
        #region Implementation of IFxLegInstrumentResults

        /// <summary>
        /// Gets the implied fx rate.
        /// </summary>
        /// <value>The implied fx rate.</value>
        public decimal ImpliedQuote { get; protected set; }

        /// <summary>
        /// Gets the market fx rate.
        /// </summary>
        /// <value>The market fx rate.</value>
        public decimal MarketQuote { get; protected set; }

        /// <summary>
        /// Gets the break even index.
        /// </summary>
        /// <value>The foreign currency NPV.</value>
        public decimal BreakEvenIndex { get; protected set; }

        #endregion
    }
}