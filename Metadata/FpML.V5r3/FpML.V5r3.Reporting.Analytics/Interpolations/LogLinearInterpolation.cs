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

#region Usings

using System;
using Orion.Analytics.Utilities;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations

{
    /// <summary>
    /// Piece-wise Linear Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class LogLinearInterpolation : LinearInterpolation
    {
        public LogLinearInterpolation()
        {}

        /// <param name="x">Sample points (N), sorted ascending</param>
        /// <param name="logy">Natural logarithm of the sample values (N) at the corresponding points</param>
        protected LogLinearInterpolation(double[] x, double[] logy) : base(x, logy)
        {}

        /// <param name="x">Sample points (N+1), sorted ascending</param>
        /// <param name="y">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        public override void Initialize(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (x.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(x));
            }
            X = x;
            var logy = new double[y.Length];
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    logy[i] = -Math.Log(y[i]);
                }
            });
            Y = logy;
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            return new LogLinearInterpolation();
        }

        /// <summary>
        /// Create a piecewise log-linear interpolation from a set of (x,y) value pairs, sorted ascending by x.
        /// </summary>
        public new static LogLinearInterpolation Interpolate(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (x.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(x));
            }
            var logy = new double[y.Length];
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    logy[i] = -Math.Log(y[i]);
                }
            });
            return new LogLinearInterpolation(x, logy);
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="axisValue">The axis value</param>
        /// <param name="extrapolation">This is implemented as a linear extrapolation out, but flat in.</param>
        /// <returns></returns>
        public override double ValueAt(double axisValue, bool extrapolation)
        {
            if (!extrapolation) return ValueAt(axisValue);
            if (axisValue <= X[0])
            {
                return ValueAt(X[0]);
            }
            var lastNode = X[X.Length - 1];
            if (axisValue > lastNode)
            {
                var penultimateNode = X[X.Length - 2];
                var extension = axisValue - lastNode;
                var interval = lastNode - penultimateNode;
                var height = Rate(lastNode) - Rate(penultimateNode);
                if (Math.Abs(interval) > 0)
                {
                    var gradient = height / interval;
                    var lastValue = Rate(lastNode);
                    var extrapolatedValue = lastValue + gradient * extension;
                    return Math.Exp(-1 * extrapolatedValue * axisValue);
                }
                return ValueAt(lastNode);
            }
            return ValueAt(axisValue);
        }
       
        /// <summary>
        /// Perform a log linear interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <returns></returns>
        public override double ValueAt(double time)
        {
            var value = base.ValueAt(time);
            return Math.Exp(-1 * value);
        }

        /// <summary>
        /// Perform a log linear interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <returns></returns>
        private double Rate(double time)
        {
            var value = base.ValueAt(time);
            var t = .000001;
            if (Math.Abs(time) > 0)
            {
                t = time;
            }
            return value / t;
        }
    }
}