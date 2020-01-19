#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using Orion.Util.Helpers;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public static class SimpleExchangeBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <param name="tolerance">Solver tolerance to use.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(IEnumerable<IPriceableFuturesAssetController> priceableAssets,
                                            DateTime baseDate, Boolean extrapolationPermitted,
                                            InterpolationMethod interpolationMethod, Double tolerance)
        {
            //  Add the first element (date : discount factor) to the list
            var points = new Dictionary<DateTime, double>();
            var items = new Dictionary<DateTime, Pair<string, decimal>>();
            // Add the rest
            foreach (IPriceableFuturesAssetController priceableAsset in priceableAssets)
            {
                DateTime assetMaturityDate = priceableAsset.LastTradeDate;
                if (points.Keys.Contains(assetMaturityDate)) continue;
                points.Add(assetMaturityDate, (double)priceableAsset.IndexAtMaturity);
                items.Add(assetMaturityDate, new Pair<string, decimal>(priceableAsset.Id, (decimal)points[assetMaturityDate]));
            }
            return TermPointsFactory.Create(items);
        }
    }
}