#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// SimpleFxCurve
    /// </summary>
    public class SimpleFxCurve : SimpleBaseCurve, IFxCurve
    {
        /// <summary>
        /// GetSpotRate
        /// Added to match interface to make build
        /// </summary>
        /// <returns></returns>
        public decimal GetSpotRate()
        {
            //TODO: Implement something here
            return 0m;
        }

        /// <summary>
        /// GetSpotDate
        /// </summary>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public DateTime GetSpotDate(DateTime baseDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="isBaseCurve"></param>
        /// <param name="derivedCurveProperties"></param>
        /// <returns></returns>
        public IRateCurve GenerateRateCurve(IRateCurve referenceCurve, bool isBaseCurve, NamedValueSet derivedCurveProperties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SimpleFxCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dates"></param>
        /// <param name="dfs"></param>
        public SimpleFxCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation, //TODO add a curve id.
                             IEnumerable<DateTime> dates, IList<double> dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dates), dfs), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleFxCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="times"></param>
        /// <param name="dfs"></param>
        public SimpleFxCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation, //TODO add a curve id.
                             IList<double> times, IList<double> dfs)
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
        /// <param name="outputQuote"></param>
        public void SetValueDirection(QuotedCurrencyPair outputQuote)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetForward
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetForward(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);
            
            return Value(point);
        }

        #region IPricingStructure implementation

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
            return new Pair<PricingStructure, PricingStructureValuation>(null, GetYieldCurveValuation());
        }

        #endregion

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}