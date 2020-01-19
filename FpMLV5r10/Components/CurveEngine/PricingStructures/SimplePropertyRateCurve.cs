#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// This is an extension to the Market structure used by CurveGen
    /// </summary>
    public class SimplePropertyRateCurve : IPricingStructure//TODO remove linearinterpolation and daycount.
    {
        private readonly Market _wrapped;

        #region Properties

        /// <summary>
        /// The CalculationDate for the YieldCurve
        /// </summary>
        public DateTime BaseDate
        { get { return _wrapped.Items1[0].baseDate.Value; } }

        #endregion

        /// <summary>
        /// Default constructor for Serialization purposes
        /// </summary>
        public SimplePropertyRateCurve(Market mkt)
        {
            _wrapped = mkt;
        }

        #region Public Methods

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

        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IValue GetValue(IPoint pt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the spot date for this yield curve
        /// If there is no defined spot then return the basedate
        /// </summary>
        /// <returns></returns>
        public DateTime GetSpotDate()
        {
            var spot = _wrapped.Items1[0].spotDate != null ? _wrapped.Items1[0].spotDate.Value : _wrapped.Items1[0].baseDate.Value;
            return spot;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IIdentifier GetPricingStructureId()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IList<IValue> GetClosestValues(IPoint pt)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolatedSpace GetInterpolatedSpace()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Market GetMarket()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IIdentifier PricingStructureIdentifier
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolatedSpace Interpolator
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Get the base date for this yield curve
        /// </summary>
        /// <returns></returns>
        public DateTime GetBaseDate()
        {
            return _wrapped.Items1[0].baseDate.Value;
        }

        #endregion

        ///<summary>
        ///</summary>
        ///<param name="point"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public double Value(IPoint point)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolation GetInterpolatingFunction()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IDiscreteSpace GetDiscreteSpace()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public bool AllowExtrapolation()
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
    }
}