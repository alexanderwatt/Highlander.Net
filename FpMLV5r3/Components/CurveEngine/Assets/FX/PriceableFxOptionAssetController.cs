﻿// Model Analytics

using System;
using Highlander.Reporting.ModelFramework.V5r3.Assets;

namespace Highlander.CurveEngine.V5r3.Assets.FX
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