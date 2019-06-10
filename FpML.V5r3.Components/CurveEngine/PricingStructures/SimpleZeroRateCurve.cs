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

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// SimpleZeroRateCurve
    /// </summary>
    public class SimpleZeroRateCurve : SimpleBaseCurve, IRateCurve
    {
        //private readonly DateTime _baseDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleZeroRateCurve"/> class.
        /// </summary>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="times">The times.</param>
        /// <param name="rates">The rates.</param>
        public SimpleZeroRateCurve(InterpolationMethod interpolation, bool extrapolation, double[] times, double[] rates)
            : base(new DiscreteCurve(times, rates), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
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
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));
            IList<IValue> result = new List<IValue> {prevValue, nextValue};
            return result;
        }

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public YieldCurveValuation GetYieldCurveValuation()
        {
            var yieldCurveValuation = new YieldCurveValuation
                                          {
                                              baseDate = IdentifiedDateHelper.Create(DateTime.MinValue)
                                          };
            return yieldCurveValuation;
        }

        /// <summary>
        /// Gets the fpML data.
        /// </summary>
        /// <returns></returns>
        public override Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            return new Pair<PricingStructure, PricingStructureValuation>(null, GetYieldCurveValuation());
        }

        public List<IPriceableRateAssetController> PriceableRateAssets => throw new NotImplementedException();

        /// <summary>
        /// GetDiscountFactor
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public double GetDiscountFactor(DateTime baseDate, DateTime targetDate)
        {
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
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
        /// <returns></returns>
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}