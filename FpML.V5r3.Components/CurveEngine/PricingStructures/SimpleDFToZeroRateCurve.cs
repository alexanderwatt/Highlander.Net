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
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework.Assets;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// SimpleDFToZeroRateCurve
    /// </summary>
    public class SimpleDFToZeroRateCurve : SimpleBaseCurve, IRateCurve
    {
        /// <summary>
        /// SimpleDFToZeroRateCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dates"></param>
        /// <param name="dfs"></param>
        public SimpleDFToZeroRateCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                       IList<DateTime> dates, double[] dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dates), DiscountFactorHelper(baseDate, dates, dfs)), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleDFToZeroRateCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="times"></param>
        /// <param name="dfs"></param>
        public SimpleDFToZeroRateCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                       double[] times, double[] dfs)
            : base(new DiscreteCurve(times, DiscountFactorHelper(times, dfs)), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        private static double[] DiscountFactorHelper(DateTime baseDate, IEnumerable<DateTime> dates, double[] dfs)
        {
            var zeroes = new List<double>();
            var dfsLength = dfs.Length;
            var times = PointCurveHelper(baseDate, dates);
            if (times.Length == dfsLength)
            {
                var zero = 0.0;
                for (var i = dfsLength - 1; i >= 0; i--)
                {
                    if (times[i] != 0)
                    {
                        zero = -Math.Log(dfs[i]) / times[i];
                    }
                    zeroes.Add(zero);
                }
            }
            return zeroes.ToArray();
        }

        private static double[] DiscountFactorHelper(double[] times, double[] dfs)
        {
            var zeroes = new List<double>();
            var dfsLength = dfs.Length; 
            if (times.Length == dfsLength)
            {
                var zero = 0.0;
                for(var i = dfsLength-1; i >=0; i--)
                {
                    if (times[i] != 0)
                    {
                        zero = -Math.Log(dfs[i]) / times[i];
                    }
                    zeroes.Add(zero);
                }
            }
            return zeroes.ToArray();
        }

        /// <summary>
        /// GetYieldCurveValuation
        /// </summary>
        /// <returns></returns>
        public YieldCurveValuation GetYieldCurveValuation()
        {
            var yieldCurveValuation = new YieldCurveValuation
                                          {
                                              baseDate = IdentifiedDateHelper.Create(BaseDate),
                                              discountFactorCurve = null
                                          };

            return yieldCurveValuation;
        }

        public List<IPriceableRateAssetController> PriceableRateAssets => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetDiscountFactor(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);
            return Value(point);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public double GetDiscountFactor(double time)
        {
            IPoint point = new Point1D(time);
            return Value(point);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetDiscountFactor(DateTime date)
        {
            IPoint point = new DateTimePoint1D(GetBaseDate(), date);
            return Value(point);
        }

        /// <summary>
        /// Gets the asset quotations.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, decimal> GetAssetQuotations()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <returns></returns>
        public Asset[] GetAssets()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the days and zero rates.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="compounding">The compounding.</param>
        /// <returns></returns>
        public IDictionary<int, double> GetDaysAndZeroRates(DateTime baseDate, string compounding)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
            GetDiscreteSpace().GetFunctionValueArray();
            var times = GetDiscreteSpace().GetCoordinateArray(1);
            var values = GetDiscreteSpace().GetFunctionValueArray();
            var index = Array.BinarySearch(times, pt.Coords[1]);//TODO check this...

            if (index >= 0)
            {
                return null;
            }
            var nextIndex = ~index;
            var prevIndex = nextIndex - 1;
            //TODO check for DateTime1D point and return the date.
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));
            IList<IValue> result = new List<IValue> {prevValue, nextValue};
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            return new Pair<PricingStructure, PricingStructureValuation>(null, GetYieldCurveValuation());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double Value(IPoint point)
        {
            var time = (double)point.Coords[0];
            var value = base.Value(point);
            return Math.Exp(-time * value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}