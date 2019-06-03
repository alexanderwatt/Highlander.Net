using System;

namespace Orion.Models.ForeignExchange
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
