/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Utilities.Helpers;
using Highlander.Reporting.Analytics.V5r3.Solvers;
using Math = System.Math;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Bootstrappers
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
            const double solveRateGap = 0.015d;//should we need more precising perhaps???
            const decimal dfamMinThreshold = 0.0m;
            const decimal defaultGuess = 0.9m;
            const double accuracy = 0.000001d;
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LinearInterpolation"); //only works for linear on zero.
            priceableAssets = priceableAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            var dates = new List<DateTime>();
            var discountFactors = new List<double>();
            var items = new Dictionary<DateTime, Pair<string, decimal>>();
            bool firstTime = true;
            foreach (var asset in priceableAssets)
            {
                DateTime assetMaturityDate = asset.GetRiskMaturityDate();
                //check if the maturity date is already in the list. If not continue.
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