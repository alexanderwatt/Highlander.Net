using System;

namespace Orion.Models.Property
{
    public interface IPropertyAssetParameters
    {
        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        Decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        Decimal PurchaseAmount { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }
    }
}