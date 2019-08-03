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
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.PricingStructures;
using Orion.ModelFramework;
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
        /// The type of curve evolution to use. The default is ForwardToSpot
        ///</summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public PricingStructureData PricingStructureData => throw new NotImplementedException();

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