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
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Utilities.Helpers;
using Highlander.Constants;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Identifiers;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures
{
    /// <summary>
    /// SimpleDiscountFactorCurve
    /// </summary>
    public class SimpleCapletCurve : SimpleBaseCurve
    {
        /// <summary>
        /// SimpleCapletCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="vols"></param>
        public SimpleCapletCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
            IDictionary<DateTime, decimal> vols)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, vols.Keys), Converter(vols.Values)), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleCapletCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="vols"></param>
        public SimpleCapletCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                         IDictionary<DateTime, double> vols)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, vols.Keys), vols.Values.ToList()), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleCapletCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dates"></param>
        /// <param name="vols"></param>
        public SimpleCapletCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                         IEnumerable<DateTime> dates, IList<double> vols)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dates), vols), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleCapletCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="times"></param>
        /// <param name="dfs"></param>
        public SimpleCapletCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation, //TODO add a curve id.
                                         IList<double> times, IList<double> dfs)
            : base(new DiscreteCurve(times, dfs), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// GetYieldCurveValuation
        /// </summary>
        /// <returns></returns>
        public VolatilityMatrix GetVolatilityMatrix()
        {
            var volatilityMatrix = new VolatilityMatrix
            {
                                              baseDate = IdentifiedDateHelper.Create(BaseDate),
                                              dataPoints = null
                                          };
            return volatilityMatrix;
        }

        public List<IPriceableRateOptionAssetController> PriceableRateOptionAssets => throw new NotImplementedException();

        /// <summary>
        /// Get the volatility
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetVolatility(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);         
            return Value(point);
        }

        /// <summary>
        /// Get the volatility.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public double GetVolatility(double time)
        {
            IPoint point = new Point1D(time);
            return Value(point);
        }

        /// <summary>
        /// Get the volatility.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetVolatility(DateTime date)
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

        #region IPricingStructure implementation
        
        /// <summary>
        /// GetPricingStructureId
        /// </summary>
        /// <returns></returns>
        public override IIdentifier GetPricingStructureId()
        {
            IPricingStructureIdentifier id = new PricingStructureIdentifier(PricingStructureTypeEnum.RateCurve, "Simple", new DateTime());
            return id;
        }

        /// <summary>
        /// GetClosestValues
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
        /// GetFpMLData
        /// </summary>
        /// <returns></returns>
        public override Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            return new Pair<PricingStructure, PricingStructureValuation>(null, GetVolatilityMatrix());
        }

        /// <summary>
        /// GetIndexValue
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public double GetIndexValue(DateTime baseDate, DateTime targetDate)
        {
            throw new NotImplementedException();
        }

        #endregion

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