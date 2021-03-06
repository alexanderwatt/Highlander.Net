﻿/*
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
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
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
    public static class ClearedRateBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="referenceDiscountCurve">The reference Discount Curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <param name="tolerance">Solver tolerance to use.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(IEnumerable<IPriceableClearedRateAssetController> priceableAssets,
                                            IRateCurve referenceDiscountCurve, DateTime baseDate, Boolean extrapolationPermitted,
                                            InterpolationMethod interpolationMethod, Double tolerance)
        {
            const double defaultGuess = 0.9;
            const double min = 1E-5;
            const double max = 2;
            //only works for linear on zero.
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            //  Add the first element (date : discount factor) to the list
            var points = new Dictionary<DateTime, double> { { baseDate, 1d } };
            var items
                = new Dictionary<DateTime, Pair<string, decimal>> { { baseDate, new Pair<string, decimal>("", 1m) } };
            var solver = new Brent();
            bool first = true;
            // Add the rest
            foreach (IPriceableClearedRateAssetController priceableAsset in priceableAssets)
            {
                DateTime assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                if (assetMaturityDate < points.Keys.Last())//TODO This is not correct!!!
                {
                    throw new InvalidOperationException("The maturity dates of the assets must be consecutive order");
                }
                if (first)
                {
                    first = false;
                    // Add the first point
                    points.Add(assetMaturityDate, defaultGuess);
                    var curve = new SimpleDiscountFactorCurve(baseDate, interp, extrapolationPermitted, points);
                    points[assetMaturityDate] = (double)priceableAsset.CalculateDiscountFactorAtMaturity(curve);
                }
                else
                {
                    //This now should automatically extrapolate the required discount factor on a flat rate basis.
                    var curve = new SimpleDiscountFactorCurve(baseDate, interp, extrapolationPermitted, points);
                    //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                    points.Add(assetMaturityDate, (double)priceableAsset.CalculateDiscountFactorAtMaturity(curve));
                }
                var objectiveFunction = new ClearedRateAssetQuote(priceableAsset, baseDate, interpolationMethod,
                                                        extrapolationPermitted, points, referenceDiscountCurve, tolerance);
                // Check whether the guess was close enough
                if (!objectiveFunction.InitialValue())
                {
                    double guess = Math.Max(min, points[assetMaturityDate]);
                    points[assetMaturityDate] = solver.Solve(objectiveFunction, tolerance, guess, min, max);
                }
                items.Add(assetMaturityDate, new Pair<string, decimal>(priceableAsset.Id, (decimal)points[assetMaturityDate]));
            }
            return TermPointsFactory.Create(items);
        }
    }
}