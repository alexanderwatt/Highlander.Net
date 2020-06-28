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
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Utilities.Helpers;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures
{
    /// <summary>
    /// SimpleCommodityCurve
    /// </summary>
    public class SimpleBondCurve : SimpleBaseCurve, IBondCurve
    {
        /// <summary>
        /// SimpleCommodityCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dfs"></param>
        public SimpleBondCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                         IDictionary<DateTime, double> dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dfs.Keys), dfs.Values.ToList()), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleCommodityCurve
        /// </summary>
        public SimpleBondCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                    IEnumerable<DateTime> dates, double[] dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dates), dfs), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleCommodityCurve
        /// </summary>
        public SimpleBondCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                    double[] times, double[] dfs)
            : base(new DiscreteCurve(times, dfs), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetForward(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);           
            return Value(point);
        }

        public double GetForward(double targetTime)
        {
            var point = new Point1D(targetTime);
            return Value(point);
        }

        public List<IPriceableCommodityAssetController> PriceableCommodityAssets { get; private set; }

        #region IPricingStructure implementation

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

        #endregion

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public List<IPriceableBondAssetController> PriceableBondAssets { get; private set; }
        public double GetYieldToMaturity(DateTime baseDate, DateTime targetDate)
        {
            throw new NotImplementedException();
        }

        public double GetYieldToMaturity(double time)
        {
            throw new NotImplementedException();
        }

        public double GetYieldToMaturity(DateTime targetDate)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, decimal> GetAssetQuotations()
        {
            throw new NotImplementedException();
        }

        public Asset[] GetAssets()
        {
            throw new NotImplementedException();
        }
    }
}