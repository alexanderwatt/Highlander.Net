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
using System.Linq;
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.Reporting.Analytics.V5r3.Interpolations

{
    /// <summary>
    /// Piece-wise Linear Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class LinearInterpolation : IInterpolation
    {
        protected double[] X;
        protected double[] Y;

        public LinearInterpolation()
        {}

        /// <param name="xArray">Sample points (N+1), sorted ascending</param>
        /// <param name="yArray">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        public LinearInterpolation(double[] xArray, double[] yArray)
        {
            if (xArray.Length != yArray.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (xArray.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(xArray));
            }
            X = xArray;
            Y = yArray;
        }

        /// <param name="xArray">Sample points (N+1), sorted ascending</param>
        /// <param name="yArray">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        public LinearInterpolation(decimal[] xArray, decimal[] yArray)
        {
            // Convert the arrays to the Decimal data type.
            double[] dXArray = xArray.Select(a => (double)a).ToArray();
            double[] dYArray = yArray.Select(a => (double)a).ToArray();
            if (dXArray.Length != dYArray.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (dXArray.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(dXArray));
            }
            X = dXArray;
            Y = dYArray;
        }

        /// <param name="x">Sample points (N+1), sorted ascending</param>
        /// <param name="y">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        public virtual void Initialize(double[] x, double[] y)
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
            Y = y;
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public virtual IInterpolation Clone()
        {
            return new LinearInterpolation();
        }

        /// <summary>
        /// Create a linear interpolation from a set of (x,y) value pairs, sorted ascending by x.
        /// </summary>
        public static LinearInterpolation Interpolate(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (x.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(x));
            }
            return new LinearInterpolation(x, y);
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="axisValue">The axis value</param>
        /// <param name="extrapolation">This is implemented as a linear extrapolation out, but flat in.</param>
        /// <returns></returns>
        public virtual double ValueAt(double axisValue, bool extrapolation)
        {
            if (!extrapolation) return ValueAt(axisValue);
            if (X.Length == 1)
            {
                return Y[0];
            }
            var firstNode = X[0];
            if (axisValue < firstNode)
            {
                var penultimateNode = X[1];
                var extension = firstNode - axisValue;
                var interval = penultimateNode - firstNode;
                var height = ValueAt(firstNode) - ValueAt(penultimateNode);
                if (Math.Abs(interval) > 0)//Math.Abs(height) > 0 && 
                {
                    var gradient = height / interval;
                    var firstValue = ValueAt(firstNode);
                    var extrapolatedValue = firstValue + gradient * extension;
                    return extrapolatedValue;
                }
                return ValueAt(firstNode);
            }
            var lastNode = X[X.Length - 1];
            if (axisValue > lastNode)
            {
                var penultimateNode = X[X.Length - 2];
                var extension = axisValue - lastNode;
                var interval = lastNode - penultimateNode;                    
                var height = ValueAt(lastNode) - ValueAt(penultimateNode);
                if (Math.Abs(interval) > 0)//Math.Abs(height) > 0 && 
                {
                    var gradient = height / interval;
                    var lastValue = ValueAt(lastNode);
                    var extrapolatedValue = lastValue + gradient * extension;
                    return extrapolatedValue;
                }
                return ValueAt(lastNode);
            }
            return ValueAt(axisValue);
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public virtual double ValueAt(double t)
        {
            int k = LeftSegmentIndex(t);
            var interval = X[k + 1] - X[k];
            var weight1 = (t - X[k]) / interval;
            var weight2 = (X[k + 1] - t) / interval;
            return weight2 * Y[k] + weight1 * Y[k + 1];
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public decimal ValueAt(decimal t)
        {
            var dt = (double)t;
            return (decimal)ValueAt(dt);
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t,
        /// or the left index of the closest segment for extrapolation.
        /// </summary>
        int LeftSegmentIndex(double t)
        {
            if (X.Length > 1)
            {
                int index = Array.BinarySearch(X, t);
                if (index < 0)
                {
                    index = ~index - 1;
                }
                return Math.Min(Math.Max(index, 0), X.Length - 2);
            }
            return 0;
        }
    }
}