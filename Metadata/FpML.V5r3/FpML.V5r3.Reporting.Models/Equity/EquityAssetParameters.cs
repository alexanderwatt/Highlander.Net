namespace Orion.Models.Equity
{
    public class EquityAssetParameters : IEquityAssetParameters
    {
        public string[] Metrics { get; set; }

        #region ISwapAssetParameters Members

        /// <summary>
        /// Flag that sets whether the forst coupon is ex div.
        /// </summary>
        public bool IsExDiv { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        public decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the equity price.
        /// </summary>
        /// <value>The equity price.</value>
        public decimal EquityPrice { get; set; }

        /// <summary>
        /// The multiplier which must be set.
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount{ get; set; }


        #endregion
    }
}