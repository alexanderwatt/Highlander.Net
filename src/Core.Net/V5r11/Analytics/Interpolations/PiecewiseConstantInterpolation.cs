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

using System;
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.Reporting.Analytics.V5r3.Interpolations

{
    /// <summary>
    /// Piece-wise Linear Interpolation.
    /// </summary>
    public class PiecewiseConstantInterpolation : IInterpolation
    {
        protected double[] Times;
        protected double[] Rates;

        public PiecewiseConstantInterpolation()
        {}

        /// <param name="times">Sample time values (N), sorted ascending</param>
        /// <param name="rates">Sample rates values (N) of each segment starting at the corresponding sample point.</param>
        public PiecewiseConstantInterpolation(double[] times, double[] rates)
        {
            if (times.Length != rates.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (times.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(times));
            }
            Times = times;
            Rates = rates;
        }

        /// <summary>
        /// Create a step interpolation from a set of (time,rate) value pairs, sorted ascending by time.
        /// </summary>
        public static PiecewiseConstantInterpolation Interpolate(double[] time, double[] rates)
        {
            return new PiecewiseConstantInterpolation(time, rates);
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
        public virtual void Initialize(double[] times, double[] rates)
        {
            if (times.Length != rates.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (times.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(times));
            }
            Times = times;
            Rates = rates;
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public virtual IInterpolation Clone()
        {
            return new PiecewiseConstantInterpolation(Times, Rates);
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time value</param>
        /// <param name="extrapolation">This is not currently implemented.</param>
        /// <returns></returns>
        public virtual double ValueAt(double time, bool extrapolation)
        {
            if (time <= Times[0])
            {
                return extrapolation ? Rates[0] : 0.0;
            }
            int k = LeftBracketIndex(time);
            return Rates[k];
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="time">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public virtual double ValueAt(double time)
        {
            return ValueAt(time, true);
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t.
        /// </summary>
        protected int LeftBracketIndex(double t)
        {
            int index = Array.BinarySearch(Times, t);
            return index >= 0 ? index : ~index - 1;
        }
    }
}