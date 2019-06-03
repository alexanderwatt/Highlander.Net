#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework.Assets;
using Orion.Analytics.DayCounters;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Math = System.Math;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public static class EquityBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(List<IPriceableEquityAssetController> priceableAssets, DateTime baseDate, Boolean extrapolationPermitted, InterpolationMethod interpolationMethod)
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
        public static TermPoint[] Bootstrap(List<IPriceableEquityAssetController> priceableAssets,
                                            DateTime baseDate, Boolean extrapolationPermitted,
                                            InterpolationMethod interpolationMethod, Double tolerance)
        {
            const Double solveRateGap = 0.015d;//should we need more precising perhaps???
            const Decimal dfamMinThreshold = 0.0m;
            const Decimal defaultGuess = 0.9m;
            const Double accuracy = 0.000001d;
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LinearInterpolation"); //only works for linear on zero.
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            var dates = new List<DateTime>();
            var discountFactors = new List<double>();
            var items = new Dictionary<DateTime, Pair<string, decimal>>();
            bool firstTime = true;
            foreach (var asset in priceableAssets)
            {
                DateTime assetMaturityDate = asset.GetRiskMaturityDate();
                //check if the maturity date is already in the list. If not contimue.
                if (items.ContainsKey(assetMaturityDate)) continue;
                decimal guess = asset.IndexAtMaturity > dfamMinThreshold ? asset.IndexAtMaturity : defaultGuess;
                decimal dfam;
                if (firstTime)
                {
                    firstTime = false;
                    dates.Add(assetMaturityDate);
                    discountFactors.Add(Convert.ToDouble(guess));
                    dfam = asset.CalculateImpliedQuote(new SimpleEquityCurve(baseDate, interp, extrapolationPermitted, dates, discountFactors));
                    discountFactors[0] = (double)dfam;
                }
                else
                {
                    //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                    //So far this is only wrt Depos and Futures...This now should automatically extrapolate the required discount factor on a flat rate basis.
                    dfam = asset.CalculateImpliedQuote(new SimpleEquityCurve(baseDate, interp, extrapolationPermitted, dates, discountFactors));
                    discountFactors.Add((double)dfam);
                    dates.Add(assetMaturityDate);
                }
                //Add a check on the dfam so that the solver is only called if outside the tolerance.
                var objectiveFunction = new EquityAssetQuote(asset, baseDate, interpolationMethod,
                                                         extrapolationPermitted, dates, discountFactors, tolerance);
                if (!objectiveFunction.InitialValue())
                {
                    var timeInterval = Actual365.Instance.YearFraction(baseDate, assetMaturityDate);
                    var solveInterval = Math.Exp(-solveRateGap * timeInterval);
                    var min = Math.Max(0, (double)dfam * solveInterval);
                    var max = (double)dfam / solveInterval;
                    var solver = new Brent();
                    dfam = (decimal)solver.Solve(objectiveFunction, accuracy, (double)dfam, min, max);
                }
                items.Add(assetMaturityDate, new Pair<string, decimal>(asset.Id, dfam));
            }
            return TermPointsFactory.Create(items);
        }
    }
}