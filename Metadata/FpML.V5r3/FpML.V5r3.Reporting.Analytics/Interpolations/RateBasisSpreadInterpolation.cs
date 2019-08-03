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

#region Using Directives

using System;
using System.Collections.Generic;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    ///Class that encapsulates functionality to perform piecewise Linear
    /// interpolation with flat-line extrapolation.
    /// </summary>
    public class RateBasisSpreadInterpolation : LinearInterpolation
    {
        ///<summary>
        /// The base curve interpolated space. This must be one dimensional.
        /// Also, the curve itself is assumed to be a discount factor curve.
        ///</summary>
        public IRateCurve BaseCurve { get; set; }

        #region Constructors

        /// <summary>
        /// Constructor for the <see cref="LinearInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        public RateBasisSpreadInterpolation(IRateCurve baseCurve)
        {
            BaseCurve = baseCurve;
        }

        #endregion Constructors

        #region Overrides

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
        public override void Initialize(double[] x, double[] discountFactors)
        {
            var zeroes = new List<double>();
            var index = 0;
            foreach (var df in discountFactors)
            {
                var time = x[index];
                var baseDf = BaseCurve.GetDiscountFactor(time);
                var ratio = df/baseDf;
                var zero = 0.0;
                if (ratio != 1 && Math.Abs(time) > 0)
                {
                    zero = -Math.Log(ratio)/time;
                }
                zeroes.Add(zero);
                index++;
            }
            X = x;
            Y = zeroes.ToArray();
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
                if (Math.Abs(interval) > 0)
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
            var baseDf = BaseCurve.GetDiscountFactor(x);
            var zero = base.ValueAt(x);
            var spreadDf = Math.Exp(-zero * x);
            var df = baseDf * spreadDf;
            return df;
        }

        #endregion Overrides

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            return new RateBasisSpreadInterpolation(BaseCurve);
        }
    }
}
