// Model Analytics

using System;
using Orion.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets.FX
{
    ///<summary>
    ///</summary>
    public abstract class PriceableFxOptionAssetController : AssetControllerBase, IPriceableRateOptionAssetController
    {
        #region IPriceableAssetController Members

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public abstract decimal VolatilityAtRiskMaturity { get; }

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        public abstract DateTime OptionsExpiryDate { get; }

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        public DateTime GetExpiryDate() => OptionsExpiryDate;

        /// <summary>
        /// The underlying asset reference
        /// </summary>
        public abstract string UnderlyingAssetRef { get; }

        #endregion
    }
}