﻿using System;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets.Options
{
    ///<summary>
    ///</summary>
    public abstract class PriceableOptionAssetController : AssetControllerBase, IPriceableOptionAssetController
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
        public abstract DateTime GetExpiryDate();

        /// <summary>
        /// The underlying asset reference
        /// </summary>
        public string UnderlyingAssetRef { get; set; }

        #endregion
    }
}