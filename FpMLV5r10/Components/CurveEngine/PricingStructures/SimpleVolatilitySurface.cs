#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Analytics.LinearAlgebra;

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// SimpleVolatilitySurface
    /// </summary>
    public class SimpleVolatilitySurface : InterpolatedSurface, IVolatilitySurface
    {
        private readonly DateTime _baseDate;

        /// <summary>
        /// SimpleVolatilitySurface
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="interpolation"></param>
        /// <param name="extrapolation"></param>
        /// <param name="times"></param>
        /// <param name="strikes"></param>
        /// <param name="vols"></param>
        public SimpleVolatilitySurface(DateTime baseDate, InterpolationMethod interpolation, bool extrapolation, //TODO add a curve id.
                                       double[] times, double[] strikes, double[,] vols  )
            : base(times, strikes, new Matrix(vols), InterpolationFactory.Create(interpolation.Value), extrapolation)
        {
            _baseDate = baseDate;
        }

        /// <summary>
        /// GetYieldCurveValuation
        /// </summary>
        /// <returns></returns>
        public YieldCurveValuation GetYieldCurveValuation()
        {
            var yieldCurveValuation = new YieldCurveValuation
                                          {
                                              baseDate = IdentifiedDateHelper.Create(_baseDate),
                                              discountFactorCurve = null
                                          };

            return yieldCurveValuation;
        }

        /// <summary>
        /// GetBaseDate
        /// </summary>
        /// <returns></returns>
        public DateTime GetBaseDate()
        {
            return _baseDate;
        }

        #region IPricingStructure implementation

        ///<summary>
        /// The type of curve evolution to use. The defualt is ForwardToSpot
        ///</summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public PricingStructureData PricingStructureData
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public IValue GetValue(IPoint pt)
        {
            var value = new DoubleValue("dummy", Value(pt), pt);

            return value;
        }

        /// <summary>
        /// GetSpotDate
        /// </summary>
        /// <returns></returns>
        public DateTime GetSpotDate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetPricingStructureId
        /// </summary>
        /// <returns></returns>
        public IIdentifier GetPricingStructureId()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetClosestValues
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public IList<IValue> GetClosestValues(IPoint pt)
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
        /// GetInterpolatedSpace
        /// </summary>
        /// <returns></returns>
        public IInterpolatedSpace GetInterpolatedSpace()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        public Pair<NamedValueSet, Market> GetPerturbedCopy(Decimal[] values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetFpMLData
        /// </summary>
        /// <returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            return new Pair<PricingStructure, PricingStructureValuation>(null, GetYieldCurveValuation());
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="dimension1"></param>
        /// <param name="dimension2"></param>
        /// <returns></returns>
        public double GetValue(double dimension1, double dimension2)
        {
            IPoint point = new Point2D(dimension1, dimension2);
            return Value(point);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object[,] GetSurface()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}