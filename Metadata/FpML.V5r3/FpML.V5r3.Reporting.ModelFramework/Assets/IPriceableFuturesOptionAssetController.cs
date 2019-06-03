using System;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableFuturesOptionAssetController : IPriceableFuturesAssetController
    {
        /// <summary>
        /// Gets the volatility at expiry.
        /// </summary>
        /// <value>The volatility at expiry.</value>
        Decimal VolatilityAtRiskMaturity { get; }

        /// <summary>
        /// The is a call flag.
        /// </summary>
        bool IsCall { get; }

        /// <summary>
        /// The commodity identifier.
        /// </summary>
        string CommodityIdentifier { get; }
    }
}