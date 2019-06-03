using System;

namespace Orion.Models.Futures
{
    public interface IFuturesOptionAssetParameters : IFuturesAssetParameters
    {
        /// <summary>
        /// Gets the strike.
        /// </summary>
        /// <value>The strike.</value>
        Decimal Strike { get; set; }

        /// <summary>
        /// Gets the underlying futures rate.
        /// </summary>
        /// <value>The underlying futures rate.</value>
        Decimal FuturesPrice { get; set; }
    }
}