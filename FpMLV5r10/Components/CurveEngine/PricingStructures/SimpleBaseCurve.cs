using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SimpleBaseCurve : InterpolatedCurve, ICurve
    {
        /// <summary>
        /// 
        /// </summary>
        protected DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discreteCurve"></param>
        /// <param name="interpolation"></param>
        /// <param name="allowExtrapolation"></param>
        protected SimpleBaseCurve(IDiscreteSpace discreteCurve, IInterpolation interpolation, bool allowExtrapolation) 
            : base(discreteCurve, interpolation, allowExtrapolation)
        {
        }

        protected static double[] PointCurveHelper(DateTime baseDate, IEnumerable<DateTime> dates)
        {
            return dates.Select(t => new DateTimePoint1D(baseDate, t).GetX()).ToArray();
        }

        protected static IList<double> Converter(IEnumerable<Decimal> dfs)
        {
            var result = new List<double>();
            foreach (var element in dfs)
            {
                result.Add((double)element);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract IList<IValue> GetClosestValues(IPoint pt);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Pair<PricingStructure, PricingStructureValuation> GetFpMLData();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetBaseDate()
        {
            return BaseDate;
        }

        public PricingStructureData PricingStructureData => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public IValue GetValue(IPoint pt)
        {
            var value = new DoubleValue("dummy", Value(pt), pt);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType { get; set; }

        #region Implementation of IPricingStructure

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetSpotDate()
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
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IIdentifier GetPricingStructureId()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IInterpolatedSpace GetInterpolatedSpace()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of ICurve

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="measureType"></param>
        /// <returns></returns>
        public bool PerturbCurve(decimal[] values, string measureType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QuotedAssetSet GetQuotedAssetSet()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TermCurve GetTermCurve()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, decimal> GetInputs()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
