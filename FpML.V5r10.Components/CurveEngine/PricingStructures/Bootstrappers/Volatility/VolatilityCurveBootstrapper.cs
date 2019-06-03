#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Assets;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers.Volatility
{
    ///<summary>
    ///</summary>
    public static class VolatilityCurveBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public static SortedList<DateTime, Decimal> Bootstrap(IEnumerable<IPriceableOptionAssetController> priceableAssets)
        {
            var points = new SortedList<DateTime, Decimal>();
            // Add the rest
            foreach (var priceableAsset in priceableAssets)
            {
                //TODO Replace with the expiry date.
                DateTime assetMaturityDate = priceableAsset.GetExpiryDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                points[assetMaturityDate] = priceableAsset.VolatilityAtRiskMaturity;
            }
            return points;
        }
    }
}