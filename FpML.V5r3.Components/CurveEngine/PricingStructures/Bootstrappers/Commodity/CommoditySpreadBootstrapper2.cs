#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Interpolations;
using Orion.CurveEngine.Assets;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public static class CommoditySpreadBootstrapper2
    {
        /// <summary>
        /// Bootstraps the specified priceable assets, where the assets 
        /// are simple commodity asset with single cash flows on a 
        /// single index observation.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="tolerance">The tolerance for the solver</param>
        /// <param name="spreadXArray">THe spread interpolator produced in the bootstrapper</param>
        /// <param name="spreadYArray">THe spread interpolator produced in the bootstrapper</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(IEnumerable<IPriceableCommoditySpreadAssetController> priceableAssets,
            ICommodityCurve referenceCurve, DateTime baseDate, bool extrapolationPermitted, double tolerance, ref IList<double> spreadXArray, ref IList<double> spreadYArray)
        {
            //only works for linear on zero.
            //InterpolationMethod interp = InterpolationMethodHelper.Parse("LinearInterpolation");
            //  Add the first element (date : discount factor) to the list
            var points = new Dictionary<DateTime, double>();
            var items
                = new SortedDictionary<DateTime, Pair<string, decimal>>();
            var dayCounter = new Actual365();
            foreach (var priceableAsset in priceableAssets)
            {
                DateTime assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                //This now should automatically extrapolate the required discount factor on a flat rate basis.
                if (IsSpreadAsset(priceableAsset))
                {
                    //These are simple assts so the solver is unnecessary.
                    var value = (double) priceableAsset.CalculateImpliedQuoteWithSpread(referenceCurve);
                    var dayCount = dayCounter.YearFraction(baseDate, assetMaturityDate);
                    spreadXArray.Add(dayCount);
                    spreadYArray.Add((double) priceableAsset.MarketQuote.value); //TODO Get the marketquote
                    points.Add(assetMaturityDate, value);
                    items.Add(assetMaturityDate,
                              new Pair<string, decimal>(priceableAsset.Id, (decimal) points[assetMaturityDate]));
                }
            }
            if (spreadXArray.Count > 2)
            {
                var spreadCurveInterpolator = new LinearInterpolation(spreadXArray.ToArray(),
                                                                        spreadYArray.ToArray());
                var index = 0;
                foreach (var assetMaturityDate in referenceCurve.GetTermCurve().GetListTermDates())
                {
                    if (points.Keys.Contains(assetMaturityDate)) continue;
                    var dayCount = dayCounter.YearFraction(baseDate, assetMaturityDate);
                    double spreadValue = spreadCurveInterpolator.ValueAt(dayCount, true);
                    var value = referenceCurve.GetForward(baseDate, assetMaturityDate);
                    points.Add(assetMaturityDate, value + spreadValue);
                    items.Add(assetMaturityDate,
                              new Pair<string, decimal>("RefCurvePillar_" + index, (decimal) points[assetMaturityDate]));
                    index++;
                }
                return TermPointsFactory.Create(items);
            }
            return null;
        }

        private static bool IsSpreadAsset(IPriceableAssetController priceableAsset)
        {
            return priceableAsset is PriceableSimpleCommoditySpreadAsset;
        }
    }
}