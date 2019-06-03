#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.ModelFramework.Assets;
using FpML.V5r3.Reporting;
using Orion.Util.Helpers;
using TermPointsFactory = Orion.CurveEngine.Helpers.TermPointsFactory;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class BondBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(List<IPriceableBondAssetController> priceableAssets, DateTime baseDate, Boolean extrapolationPermitted, InterpolationMethod interpolationMethod)
        {
            return Bootstrap(priceableAssets, baseDate, extrapolationPermitted, interpolationMethod, 0.000001d);
        }

        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <param name="tolerance">Solver tolerance to use.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(List<IPriceableBondAssetController> priceableAssets, 
                                            DateTime baseDate, Boolean extrapolationPermitted, 
                                            InterpolationMethod interpolationMethod, Double tolerance)
        {
            priceableAssets.Sort
                (
                (priceableAssetController1, priceableAssetController2) =>
                priceableAssetController1.GetRiskMaturityDate().CompareTo(priceableAssetController2.GetRiskMaturityDate())
                );
            var points = new Dictionary<DateTime, double>() ;
            var items
                = new Dictionary<DateTime, Pair<string, decimal>>() ;
            foreach (var priceableAsset in priceableAssets)
            {
                var assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                decimal guess = priceableAsset.QuoteValue;
                //decimal dfam;
                //only works for linear on zero.
                //var interp = InterpolationMethodHelper.Parse("LinearInterpolation");
                //var curve = new SimpleBondCurve(baseDate, interp, extrapolationPermitted, points);
                //var dfam = priceableAsset.CalculateImpliedQuote(curve);
                points.Add(assetMaturityDate, (double)guess);//dfam
                items.Add(assetMaturityDate, new Pair<string, decimal>(priceableAsset.Id, (decimal)points[assetMaturityDate]));
            }
            return TermPointsFactory.Create(items);
        }
    }
}