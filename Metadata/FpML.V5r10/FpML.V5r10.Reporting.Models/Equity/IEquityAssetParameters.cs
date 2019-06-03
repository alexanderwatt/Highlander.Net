using System;

namespace FpML.V5r10.Reporting.Models.Equity
{
    public interface IEquityAssetParameters
    {
        /// <summary>
        /// Flag that sets whether the forst coupon is ex div.
        /// </summary>
        Boolean IsExDiv { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        Decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the equity price.
        /// </summary>
        /// <value>The equity price.</value>
        Decimal EquityPrice { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }
    }
}