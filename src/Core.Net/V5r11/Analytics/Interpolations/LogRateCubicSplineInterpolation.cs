﻿/*
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

#region Usigns

using System;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations
{
    /// <summary>
    /// Class that encapsulates functionality to perform one dimensional
    /// interpolation with piecewise cubic Hermite splines.
    /// The class does not support extrapolation.
    /// </summary>
    public class LogRateCubicSplineInterpolation : CubicSplineInterpolation
    {
        public LogRateCubicSplineInterpolation()
        {}

        ///<summary>
        ///</summary>
        ///<param name="times"></param>
        /// <param name="rates">Natural logarithm of the sample values (N) at the corresponding points</param>
        ///<exception cref="NotImplementedException"></exception>
        public LogRateCubicSplineInterpolation(double[] times, double[] rates)
        {
            Initialize(times, rates);
        }

        /// <summary>
        /// Create a piecewise log-linear interpolation from a set of (x,y) value pairs, sorted ascending by x.
        /// </summary>
        public static LogRateCubicSplineInterpolation Interpolate(double[] x, double[] dfs)
        {
            if (x.Length != dfs.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (x.Length < 2)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(x));
            }
            var rates = new double[dfs.Length];
            CommonParallel.For(0, dfs.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    if (Math.Abs(x[i]) > 0)
                    {
                        rates[i] = -Math.Log(dfs[i]) / x[i];
                    }
                    else
                    {
                        rates[i] = dfs[i + 1];
                    }
                }
            });
            return new LogRateCubicSplineInterpolation(x, rates);
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <param name="extrapolation">extrapolation flag</param>
        /// <returns>Interpolated value x(t).</returns>
        public override double ValueAt(double t, bool extrapolation)
        {
            if (!extrapolation) return ValueAt(t);
            if (t <= _x[0])
            {
                return ValueAt(_x[0]);
            }
            if (t >= _x[_x.Length - 1])
            {
                return ValueAt(_x[_x.Length - 1]);
            }
            return ValueAt(t);
        }

        /// <summary>
        /// Interpolated value.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The interpolated value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when extrapolation has not been allowed and the passed value
        /// is outside the allowed range.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the class was not properly initialized.
        /// </exception>
        public override double ValueAt(double x)
        {
            return Math.Abs(x) > 0 ? Math.Exp(base.ValueAt(x) * -x) : 1.0;
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            return new LogRateCubicSplineInterpolation();
        }
    }
}