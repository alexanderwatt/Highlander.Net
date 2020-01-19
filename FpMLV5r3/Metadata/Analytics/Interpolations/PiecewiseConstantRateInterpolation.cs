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
    public class PiecewiseConstantRateInterpolation : LinearInterpolation
    {
        public PiecewiseConstantRateInterpolation()
        {}

        /// <param name="times">Sample time points (N), sorted ascending</param>
        /// <param name="rates">Sample rates values (N) of each segment starting at the corresponding sample point.</param>
        protected PiecewiseConstantRateInterpolation(double[] times, double[] rates)
        {
            Y = rates;
            if (times.Length != rates.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (times.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(times));
            }
            X = times;
            for (int i = 0; i < times.Length; i++)
            {
                Y[i] = rates[i] * times[i];
            }
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
        public override void Initialize(double[] times, double[] rates)
        {
            Y = rates;
            if (times.Length != rates.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (times.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(times));
            }
            X = times;
            for (int i = 0; i < times.Length; i++)
            {
                Y[i] = rates[i] * times[i];
            }
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            var interpolation = new PiecewiseConstantRateInterpolation();
            return interpolation;
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <param name="extrapolation">This is not currently implemented.</param>
        /// <returns></returns>
        public override double ValueAt(double time, bool extrapolation)
        {
            if (time <= X[0])
            {
                return extrapolation ? ValueAt(X[0]) : 0.0;
            }
            if (time >= X[X.Length - 1])
            {
                return extrapolation ? ValueAt(X[X.Length - 1]) : 0.0;
            }
            return ValueAt(time);
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="time">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public override double ValueAt(double time)
        {
            if (Math.Abs(time) > 0)
            {
                return base.ValueAt(time) / time;
            }
            return base.ValueAt(time) / 0.000001;
        }
    }
}