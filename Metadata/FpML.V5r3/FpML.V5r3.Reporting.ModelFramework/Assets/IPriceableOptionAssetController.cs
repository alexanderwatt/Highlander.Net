using System;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableOptionAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the volatility at expiry.
        /// </summary>
        /// <value>The volatility at expiry.</value>
        Decimal VolatilityAtRiskMaturity { get; }

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        DateTime GetExpiryDate();

        /// <summary>
        /// The underlying asset reference
        /// </summary>
        string UnderlyingAssetRef { get; }
    }
}