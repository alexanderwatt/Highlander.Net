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
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using TermPointsFactory = Highlander.CurveEngine.V5r3.Helpers.TermPointsFactory;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class CommodityBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(List<IPriceableCommodityAssetController> priceableAssets, DateTime baseDate, Boolean extrapolationPermitted, InterpolationMethod interpolationMethod)
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
        public static TermPoint[] Bootstrap(List<IPriceableCommodityAssetController> priceableAssets, 
                                            DateTime baseDate, Boolean extrapolationPermitted, 
                                            InterpolationMethod interpolationMethod, Double tolerance)
        {
            //const Double cSolveRateGap = 0.015d;//should we need more precising perhaps???
            //const double Min = 0.000000001;
            //const double max = 2;
            const double defaultGuess = 0.9;
            //const Double accuracy = 0.000001d;
            priceableAssets.Sort
                (
                (priceableAssetController1, priceableAssetController2) =>
                priceableAssetController1.GetRiskMaturityDate().CompareTo(priceableAssetController2.GetRiskMaturityDate())
                );
            //  Add the first element (date : discount factor) to the list
            //
            //  Add the first element (date : discount factor) to the list
            var points = new Dictionary<DateTime, double>() ;//{ { baseDate, 1d } }
            var items
                = new Dictionary<DateTime, Pair<string, decimal>>() ;//{ { baseDate, new Pair<string, decimal>("", 1m) } }
            bool first = true;
            foreach (var priceableAsset in priceableAssets)
            {
                //TODO check if the maturity date is already in the list. If not contimue.
                var assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                // do we really need that guess step??? I don't think so...
                //
                //if (assetMaturityDate < points.Keys.Last())
                //{
                //    throw new InvalidOperationException("The maturity dates of the assets must be consecutive order");
                //}
                //only works for linear on zero.
                var interp = InterpolationMethodHelper.Parse("LinearInterpolation");
                decimal dfam;
                if (first)
                {
                    first = false;
                    // Add the first point
                    points.Add(assetMaturityDate, defaultGuess);
                    var curve = new SimpleCommodityCurve(baseDate, interp, extrapolationPermitted, points);
                    dfam = priceableAsset.CalculateImpliedQuote(curve);
                    points[assetMaturityDate] = (double)dfam;
                }
                else
                {
                    //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                    //So far this is only wrt Deposits and Futures...This now should automatically extrapolate the required discount factor on a flat rate basis.
                    var curve = new SimpleCommodityCurve(baseDate, interp, extrapolationPermitted, points);
                    //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                    dfam = priceableAsset.CalculateImpliedQuote(curve);
                    points.Add(assetMaturityDate, (double)dfam);
                }
                //Add a check on the dfam so that the solver is only called if outside the tolerance.
                //var objectiveFunction = new CommodityAssetQuote(priceableAsset, baseDate, interpolationMethod,
                //                                         extrapolationPermitted, points, tolerance);
                //// Check whether the guess was close enough
                //if (!objectiveFunction.InitialValue())
                //{
                //    var timeInterval = Actual365.Instance.YearFraction(baseDate, assetMaturityDate);
                //    var cSolveInterval = Math.Exp(-cSolveRateGap * timeInterval);
                //    var min = Math.Max(0,(double)dfam * cSolveInterval);
                //    var max = (double)dfam / cSolveInterval;
                //    double guess = Math.Max(min, points[assetMaturityDate]);
                //    var solver = new Brent();
                //    points[assetMaturityDate] = solver.Solve(objectiveFunction, tolerance, guess, min, max);
                //}
                items.Add(assetMaturityDate, new Pair<string, decimal>(priceableAsset.Id, (decimal)points[assetMaturityDate]));
            }
            return TermPointsFactory.Create(items);
        }
    }
}