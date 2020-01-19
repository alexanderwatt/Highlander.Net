/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Interpolations
{
    /// <summary>
    /// Linear Interpolator base class
    /// </summary>
    public class SpreadInterpolation : IInterpolation
    {
        private LinearRateInterpolation BaseCurve{ get; }

        private LinearInterpolation SpreadCurve { get; }

        /// <summary>
        /// Perform a linear interpolation on a sorted array of times, where the value
        /// is relative to a base value that has been transformed to a gog linear zero rate interpolated base value/>
        /// </summary>
        /// <param name="baseTimes">The base times array of values</param>
        /// <param name="baseValues">The base values array to interpolate</param>
        /// <param name="spreadTimes">The spread times array of values</param>
        /// <param name="spreadValues">The spread values array to interpolate</param>
        protected SpreadInterpolation(double[] baseTimes, double[] baseValues, double[] spreadTimes, double[] spreadValues)
        {
            BaseCurve = LinearRateInterpolation.Interpolate(baseTimes, baseValues);
            SpreadCurve = LinearInterpolation.Interpolate(spreadTimes, spreadValues);
        }

        /// <summary>
        /// Perform a linear interpolation on a sorted array of times, where the value
        /// is relative to a base value that has been transformed to a gog linear zero rate interpolated base value/>
        /// </summary>
        /// <param name="baseTimes">The base times array of values</param>
        /// <param name="baseValues">The base values array to interpolate</param>
        /// <param name="spreadTimes">The spread times array of values</param>
        /// <param name="spreadValues">The spread values array to interpolate</param>
        /// <returns></returns>
        public static SpreadInterpolation Interpolate(double[] baseTimes, double[] baseValues, double[] spreadTimes, double[] spreadValues)
        {
            if (spreadTimes.Length != spreadValues.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            return new SpreadInterpolation(baseTimes, baseValues, spreadTimes, spreadValues);
        }

        /// <summary>
        /// Perform a model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time value</param>
        /// <param name="extrapolation">This is not currently implemented.</param>
        /// <returns></returns>
        public double ValueAt(double time, bool extrapolation)
        {
            var result = BaseCurve.ValueAt(time, extrapolation) + SpreadCurve.ValueAt(time, extrapolation);
            return result;
        }

        /// <summary>
        /// Initialize a class constructed by the default constructor.
        /// </summary>
        /// <remarks>
        /// The sequence of values for x must have been sorted.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the passed lists do not have the same size
        /// or do not provide enough points to interpolate.
        /// </exception>
        public void Initialize(double[] x, double[] y)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Perform an interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <returns></returns>
        public double ValueAt(double time)
        {
            return ValueAt(time, true);
        }
    }
}