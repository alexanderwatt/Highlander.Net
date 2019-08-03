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
    public class LinearRateInterpolation : LinearInterpolation
    {
        /// <summary>
        /// The empty constructor
        /// </summary>
        public LinearRateInterpolation()
        { }

        /// <param name="x">Sample points (N+1), sorted ascending</param>
        /// <param name="y">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        protected LinearRateInterpolation(double[] x, double[] y) : base(x, y)
        {}

        /// <summary>
        /// Create a linear interpolation from a set of (x,y) value pairs, sorted ascending by x.
        /// </summary>
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
            var rates = new double[y.Length];
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    if (Math.Abs(x[i]) > 0)
                    {
                        rates[i] = -Math.Log(y[i]) / x[i];
                    }
                    else
                    {
                        rates[i] = y[i + 1];
                    }
                }
            });
            Y = rates;
        }

        /// <summary>
        /// Create a linear interpolation from a set of (x,y) value pairs, sorted ascending by x.
        /// </summary>
        public new static LinearRateInterpolation Interpolate(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (x.Length < 1)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(x));
            }
            var rates = new double[y.Length];
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    if (Math.Abs(x[i]) > 0)
                    {
                        rates[i] = -Math.Log(y[i]) / x[i];
                    }
                    else
                    {
                        rates[i] = y[i + 1];
                    }
                }
            });
            return new LinearRateInterpolation(x, rates);
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            return new LinearRateInterpolation();
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
        /// Perform a model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <returns></returns>
        public override double ValueAt(double time)
        {
            var value =  Math.Exp(-base.ValueAt(time) * time);
            return value;
        }
    }
}