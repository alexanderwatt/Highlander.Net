#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.Identifiers;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Constants;
using Orion.Util.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// SimpleDiscountFactorCurve
    /// </summary>
    public class SimpleDiscountFactorCurve : SimpleBaseCurve, IInflationCurve
    {
        /// <summary>
        /// SimpleDiscountFactorCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dates"></param>
        /// <param name="dfs"></param>
        public SimpleDiscountFactorCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
            DateTime[] dates, Decimal[] dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dates), Converter(dfs)), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleDiscountFactorCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dfs"></param>
        public SimpleDiscountFactorCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                         IDictionary<DateTime, double> dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dfs.Keys), dfs.Values.ToList()), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleDiscountFactorCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dfs"></param>
        public SimpleDiscountFactorCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
            SortedList<DateTime, double> dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dfs.Keys), dfs.Values.ToList()), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleDiscountFactorCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="dates"></param>
        /// <param name="dfs"></param>
        public SimpleDiscountFactorCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation,
                                         IEnumerable<DateTime> dates, IList<double> dfs)
            : base(new DiscreteCurve(PointCurveHelper(baseDate, dates), dfs), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            BaseDate = baseDate;
        }

        /// <summary>
        /// SimpleDiscountFactorCurve
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="times"></param>
        /// <param name="dfs"></param>
        public SimpleDiscountFactorCurve(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation, //TODO add a curve id.
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

        public List<IPriceableRateAssetController> PriceableRateAssets => throw new NotImplementedException();

        /// <summary>
        /// GetDiscountFactor
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
        /// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of pertubed rate curves</returns>
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
            return new Pair<PricingStructure, PricingStructureValuation>(null, GetYieldCurveValuation());
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