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
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework.Assets;
using Orion.Analytics.DayCounters;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Math = System.Math;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class CreditBootstrapper
    {
        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolationPermitted">The extrapolationPermitted flag.</param>
        /// <param name="interpolationMethod">The interpolationMethod.</param>
        /// <returns></returns>
        public static TermPoint[] Bootstrap(List<IPriceableCreditAssetController> priceableAssets, DateTime baseDate, Boolean extrapolationPermitted, InterpolationMethod interpolationMethod)
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
        public static TermPoint[] Bootstrap(List<IPriceableCreditAssetController> priceableAssets, 
                                            DateTime baseDate, Boolean extrapolationPermitted, 
                                            InterpolationMethod interpolationMethod, Double tolerance)
        {
            const Double cSolveRateGap = 0.015d;//should be need more precise perhaps???
            const Decimal cDfamMinThreshold = 0.0m;
            const Decimal cDefaultGuess = 0.9m;
            const Double accuracy = 0.000001d;
            priceableAssets.Sort
                (
                (priceableAssetController1, priceableAssetController2) =>
                priceableAssetController1.GetRiskMaturityDate().CompareTo(priceableAssetController2.GetRiskMaturityDate())
                );
            //  Add the first element (date : discount factor) to the list
            //
            IList<DateTime> dates = new List<DateTime> { baseDate };
            IList<double> discountFactors = new List<double> { 1.0 };
            var index = 0;
            foreach (var priceableAsset in priceableAssets)
            {
                //TODO check if the maturity date is already in the list. If not contimue.
                var assetMaturityDate = priceableAsset.GetRiskMaturityDate();
                if (dates.Contains(assetMaturityDate)) continue;
                // do we really need that guess step??? I don't think so...
                //
                var guess = priceableAsset.SurvivalProbabilityAtMaturity;
                if (guess <= cDfamMinThreshold)
                {
                    guess = cDefaultGuess;
                }
                //dates.Add(priceableAsset.GetRiskMaturityDate());
                decimal dfam;
                double[] values;
                //var position = dates.IndexOf(priceableAsset.GetRiskMaturityDate());
                //discountFactors.CopyTo(values, 0);
                //only works for linear on zero.
                var interp = InterpolationMethodHelper.Parse("LogLinearInterpolation");
                if (index == 0)
                {
                    dates.Add(priceableAsset.GetRiskMaturityDate());
                    values = new double[dates.Count];
                    var position = dates.IndexOf(priceableAsset.GetRiskMaturityDate());
                    discountFactors.CopyTo(values, 0);
                    values[position] = Convert.ToDouble(guess);
                    dfam = priceableAsset.CalculateImpliedQuote(new SimpleDiscountFactorCurve(baseDate, interp, extrapolationPermitted, dates, values));
                    values[position] = (double)dfam;
                    index++;
                }
                else
                {
                    //The first guess, which should be correct for all priceable assets with analytical solutions that have been implemented.
                    //So far this is only wrt Deposits and Futures...This now should automatically extrapolate the required discount factor on a flat rate basis.
                    var tempvalues = new List<double>(discountFactors);
                    dfam = priceableAsset.CalculateImpliedQuote(new SimpleDiscountFactorCurve(baseDate, interp, extrapolationPermitted, dates, tempvalues.ToArray()));
                    tempvalues.Add((double)dfam);
                    dates.Add(priceableAsset.GetRiskMaturityDate());
                    values = new double[dates.Count];
                    tempvalues.CopyTo(values, 0);
                    index++;
                }
                //Add a check on the dfam so that the solver is only called if outside the tolerance.
                var objectiveFunction = new CreditAssetQuote(priceableAsset, baseDate, interpolationMethod,
                                                           extrapolationPermitted, dates, values, tolerance);
                if (objectiveFunction.InitialValue())
                {
                    discountFactors.Add((double)dfam);
                }
                else
                {
                    var timeInterval = Actual365.Instance.YearFraction(baseDate, assetMaturityDate);
                    var cSolveInterval = Math.Exp(-cSolveRateGap * timeInterval);
                    var min = Math.Max(0,(double)dfam * cSolveInterval);
                    var max = (double)dfam / cSolveInterval;
                    var solver2 = new Brent();
                    var solvedFiscountFactor = solver2.Solve(objectiveFunction, accuracy, (double)dfam, min, max);
                    //TODO check that this is the correct usage of the optimizer.
                    discountFactors.Add(solvedFiscountFactor);
                }
            }
            return TermPointsFactory.Create(dates, discountFactors);
        }
    }
}