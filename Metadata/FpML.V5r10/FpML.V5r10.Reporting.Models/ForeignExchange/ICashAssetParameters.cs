using System;

namespace FpML.V5r10.Reporting.Models.ForeignExchange
{
    public interface ICashAssetParameters
    {
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }
    }
}
