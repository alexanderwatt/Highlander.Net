namespace Orion.Models.Property
{
    public class PropertyAssetParameters : IPropertyAssetParameters
    {
        public string[] Metrics { get; set; }

        #region ISwapAssetParameters Members

        public decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the equity price.
        /// </summary>
        /// <value>The equity price.</value>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// The multiplier which must be set.
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal PurchaseAmount { get; set; }


        #endregion
    }
}