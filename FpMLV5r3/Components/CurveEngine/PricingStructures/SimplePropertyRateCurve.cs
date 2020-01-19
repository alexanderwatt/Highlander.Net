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
using Highlander.Utilities.Helpers;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures
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
        /// The type of curve evolution to use. The default is ForwardToSpot
        ///</summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public PricingStructureData PricingStructureData => throw new NotImplementedException();

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
        /// If there is no defined spot then return the base date
        /// </summary>
        /// <returns></returns>
        public DateTime GetSpotDate()
        {
            var spot = _wrapped.Items1[0].spotDate?.Value ?? _wrapped.Items1[0].baseDate.Value;
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
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolatedSpace Interpolator
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
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