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
using Highlander.CurveEngine.V5r3.Assets;
using Highlander.CurveEngine.V5r3.Assets.Rates.Spreads;
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
    public static class RateSpreadBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="tolerance">The tolerance for the solver</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(IEnumerable<IPriceableRateSpreadAssetController> priceableAssets, 
            IRateCurve referenceCurve, DateTime baseDate, bool extrapolationPermitted, double tolerance)
        {
            const double defaultGuess = 0.9;
            const double min = 0.000000001;
            const double max = 2;
            //only works for linear on zero.
            InterpolationMethod interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            //  Add the first element (date : discount factor) to the list
            var points = new Dictionary<DateTime, double> { { baseDate, 1d } };
            var items
                = new Dictionary<DateTime, Pair<string, decimal>> { { baseDate, new Pair<string, decimal>("", 1m) } };
            var solver = new Brent();
            bool first = true;
            foreach (var priceableAsset in priceableAssets)
            {
                DateTime assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                if (assetMaturityDate < points.Keys.Last())
                {
                    throw new InvalidOperationException("The maturity dates of the assets must be consecutive order");
                }
                if (first)
                {
                    first = false;
                    // Add the first point
                    points.Add(assetMaturityDate, defaultGuess);
                    IRateCurve curve = IsSpreadAsset(priceableAsset) ? referenceCurve : new SimpleDiscountFactorCurve(baseDate, interp, extrapolationPermitted, points);
                    points[assetMaturityDate] = (double)priceableAsset.CalculateDiscountFactorAtMaturity(curve);
                }
                else
                {
                    //This now should automatically extrapolate the required discount factor on a flat rate basis.
                    IRateCurve curve = IsSpreadAsset(priceableAsset) ? referenceCurve : new SimpleDiscountFactorCurve(baseDate, interp, extrapolationPermitted, points);
                    //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                    points.Add(assetMaturityDate, (double)priceableAsset.CalculateDiscountFactorAtMaturity(curve));
                }
                IObjectiveFunction objectiveFunction;
                bool initialValue;
                if (IsSpreadAsset(priceableAsset))
                {
                    objectiveFunction = new RateSpreadAssetQuote(priceableAsset, referenceCurve, baseDate,
                                                                 extrapolationPermitted, points, tolerance);
                    initialValue = ((RateSpreadAssetQuote)objectiveFunction).InitialValue();
                }
                else
                {
                    objectiveFunction = new RateAssetQuote(priceableAsset, baseDate, interp, extrapolationPermitted, 
                                                           points, tolerance);
                    initialValue = ((RateAssetQuote)objectiveFunction).InitialValue();
                }
                // Check whether the guess was close enough
                if (!initialValue)
                {
                    double guess = Math.Max(min, points[assetMaturityDate]);
                    points[assetMaturityDate] = solver.Solve(objectiveFunction, tolerance, guess, min, max);
                }               
                items.Add(assetMaturityDate, new Pair<string, decimal>(priceableAsset.Id, (decimal)points[assetMaturityDate]));
            }
            return TermPointsFactory.Create(items);
        }

        private static bool IsSpreadAsset(IPriceableAssetController priceableAsset)
        {
            return priceableAsset is PriceableBasisSwap || priceableAsset is PriceableSimpleRateSpreadAsset;
        }
    }
}