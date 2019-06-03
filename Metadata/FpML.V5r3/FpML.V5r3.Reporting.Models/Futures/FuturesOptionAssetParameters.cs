using System;

namespace Orion.Models.Futures
{
    public class FuturesOptionAssetParameters : FuturesAssetParameters, IFuturesOptionAssetParameters
    {

        #region IFuturesOptionAssetParameters Members

        public decimal Strike { get; set; }

        /// <summary>
        /// Gets the underlying futures rate.
        /// </summary>
        /// <value>The underlying futures rate.</value>
        public decimal FuturesPrice { get; set; }

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        public Decimal OptionVolatility { get; set; }

        #endregion
    }
}