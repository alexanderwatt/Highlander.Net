#region Using directives

using System;
using System.Collections.Generic;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public static class RateXccySpreadBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable spread rate assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="termCurve">The term Curve with pre-existing points.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(IList<IPriceableRateSpreadAssetController> priceableAssets,
            RateCurve referenceCurve, DateTime baseDate, TermCurve termCurve)
        {
            return Bootstrap(priceableAssets, referenceCurve, baseDate, termCurve, referenceCurve.Tolerance);
        }

        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="termCurve">The term Curve with pre-existing points. This will not work if there are no points.</param>
        /// <param name="tolerance">Solver tolerance to use.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(IList<IPriceableRateSpreadAssetController> priceableAssets,
                                            IRateCurve referenceCurve, DateTime baseDate, TermCurve termCurve, 
                                            Double tolerance)
        {
            const double min = 0.000000001;
            const double max = 1;
            Dictionary<DateTime, Pair<string, decimal>> items
                = new Dictionary<DateTime, Pair<string, decimal>> ();
            //  Add the elements (date : discount factor) to the list
            IDictionary<DateTime, double> dfs = new Dictionary<DateTime, double>();
            foreach (var point in termCurve.point)
            {
                dfs.Add((DateTime)point.term.Items[0], (double)point.mid);
                items.Add((DateTime)point.term.Items[0], new Pair<string, decimal>(point.id, point.mid));
            }
            var solver = new Brent();
            foreach (var priceableAsset in priceableAssets)
            {
                var assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (dfs.ContainsKey(assetMaturityDate)) continue;
                //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                //So far this is only wrt Depos and Futures...This now should automatically extrapolate the required discount factor on a flat rate basis.
                dfs.Add(assetMaturityDate, (double)priceableAsset.CalculateDiscountFactorAtMaturity(referenceCurve));
                var objectiveFunction = new RateSpreadAssetQuote(priceableAsset, referenceCurve, baseDate,
                                                           termCurve.extrapolationPermitted, dfs, tolerance);
                // check accuracy so that solver is only called if outside the tolerance.
                if (!objectiveFunction.InitialValue())
                {
                    dfs[assetMaturityDate] = solver.Solve(objectiveFunction, tolerance, dfs[assetMaturityDate], min, max);
                }
                items.Add(assetMaturityDate, new Pair<string, decimal>(priceableAsset.Id, (decimal)dfs[assetMaturityDate]));
            }
            return TermPointsFactory.Create(items);
        }
    }
}