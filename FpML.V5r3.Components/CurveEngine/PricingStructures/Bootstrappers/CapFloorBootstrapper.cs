/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Dates;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.Assets.Rates.CapFloorLet;
using Orion.CurveEngine.Assets.Rates.CapsFloors;
using Orion.CurveEngine.Assets.Rates.Futures;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.NamedValues;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public static class CapFloorBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <param name="tolerance">Solver tolerance to use.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        /// <param name="curveProperties">The properties.</param>
        /// <returns></returns>
        public static SortedList<DateTime, Decimal> Bootstrap(IEnumerable<IPriceableOptionAssetController> priceableAssets,
            NamedValueSet curveProperties, IRateCurve discountCurve, IRateCurve forecastCurve, DateTime baseDate, Boolean extrapolationPermitted,
            InterpolationMethod interpolationMethod, Double tolerance)
        {
            const double min = 0.000000001;
            const double max = 2;
            if (interpolationMethod == null)
            {
                interpolationMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            }
            var points = new SortedList<DateTime, Decimal>();
            var solver = new Brent();
            bool first = true;
            // Add the rest
            foreach (var priceableOptionAssetController in priceableAssets)
            {
                var priceableAsset = (IPriceableRateOptionAssetController) priceableOptionAssetController;
                DateTime assetMaturityDate = priceableAsset.GetExpiryDate();
                if (points.Keys.Contains(assetMaturityDate)) continue;
                if (priceableAsset is PriceableRateFuturesOptionAsset || priceableAsset is PriceableSimpleRateOptionAsset)
                {
                    points[assetMaturityDate] = priceableAsset.VolatilityAtRiskMaturity;
                    first = false;
                }
                if (priceableAsset is PriceableCapRateAsset asset)
                {
                    //Some new code which creates flat volatilities out to the first expiry when no ETOs are supplied.
                    if (first)
                    {
                        var maturityDate = priceableAsset.GetRiskMaturityDate();
                        int weeks = DateHelper.GetWeeks(baseDate, maturityDate);
                        for (var i = 1; i <= weeks; i++)
                        {
                            var shortEndDate = baseDate.AddDays(7 * i);
                            points[shortEndDate] = priceableAsset.VolatilityAtRiskMaturity;
                        }
                        points[assetMaturityDate] = priceableAsset.VolatilityAtRiskMaturity;
                    }
                    else
                    {
                        //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                        points[assetMaturityDate] = priceableAsset.VolatilityAtRiskMaturity;
                        //Set the ATM strike
                        asset.DiscountCurve = discountCurve;
                        asset.ForecastCurve = forecastCurve;
                        asset.Strike = asset.CalculateImpliedParRate(baseDate);
                        var objectiveFunction = new CapFloorAssetQuote(asset, discountCurve, forecastCurve, baseDate, interpolationMethod,
                            extrapolationPermitted, points, tolerance);
                        // Check whether the guess was close enough
                        var initialValue = objectiveFunction.InitialValue();
                        if (initialValue) continue;
                        var tempMax = (double) points[assetMaturityDate];
                        double guess = Math.Max(min, tempMax);
                        var result = solver.Solve(objectiveFunction, tolerance, guess, min, max);
                        points[assetMaturityDate] = (decimal)result;
                    }
                }
            }
            return points;
        }
    }
}