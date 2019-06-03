namespace Orion.Models.Property
{
    public class PropertyAssetResults : IPropertyAssetResults
    {
        #region Implementation of IEquityAssetResults

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal NPV{ get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public decimal PandL { get; set; }

        #endregion
    }
}