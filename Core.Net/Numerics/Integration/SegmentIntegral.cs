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
using Highlander.Numerics.Solvers;

#endregion

namespace Highlander.Numerics.Integration
{
    /// <summary>
    /// Integral of a one-dimensional function.
    /// </summary>
    /// <latex>
    /// Given a number \f$ N \f$ of intervals, the integral of
    /// a function \f$ f \f$ between \f$ a \f$ and \f$ b \f$ is 
    /// calculated by means of the trapezoid formula
    /// \f[
    /// 	\int_{a}^{b} f \mathrm{d}x = 
    /// 	\frac{1}{2} f(x_{0}) + f(x_{1}) + f(x_{2}) + \dots 
    /// 	+ f(x_{N-1}) + \frac{1}{2} f(x_{N})
    /// \f]
    /// where \f$ x_0 = a \f$, \f$ x_N = b \f$, and 
    /// \f$ x_i = a+i \Delta x \f$ with \f$ \Delta x = (b-a)/N \f$.
    /// </latex>
    public class SegmentIntegral
    {
        /// <summary>
        /// Initialize a new SegmentIntegral with the given intervals.
        /// </summary>
        /// <param name="intervals"></param>
        public SegmentIntegral(int intervals)
        {
            if (intervals <= 0)
                // At least 1 interval needed, {0} given.
                throw new ArgumentOutOfRangeException("SegmInter");
            _intervals = intervals;
        }

        private readonly int _intervals;

        /// <summary>
        /// Computes the integral value of the given one-dimensional function.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double Value(UnaryFunction f, double a, double b)
        {
            if (a >= b)
                // To compute an integral on [a,b] it must be a<b
                throw new ArgumentException("SegmParams");

            double dx = (b - a) / _intervals;
            double sum = 0.5 * (f(a) + f(b));
            double end = b - 0.5 * dx;
            for (double x = a + dx; x < end; x += dx)
                sum += f(x);
            return sum * dx;
        }
    }
}