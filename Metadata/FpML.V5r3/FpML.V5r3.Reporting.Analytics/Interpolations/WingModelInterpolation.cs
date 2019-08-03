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
using Orion.Analytics.Helpers;
using Orion.Analytics.Options;
using Orion.Analytics.Solvers;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// Linear Interpolator base class
    /// </summary>
    public class WingModelInterpolation : IInterpolation
    {
        protected double[] X;
        protected double[] Y;

        /// <summary>
        /// Gets or sets the wing parameters.
        /// </summary>
        /// <value>The wing parameters.</value>
        public OrcWingParameters WingParameters { get; private set; }

        /// <summary>
        /// Gets or sets the forward.
        /// </summary>
        /// <value>The forward.</value>
        public double Forward { get; set; }

        /// <summary>
        /// Gets or sets the spot.
        /// </summary>
        /// <value>The spot.</value>
        public double Spot { get; set; }

        public WingModelInterpolation()
        {}

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="axisVector">The axis vector of values</param>
        /// <param name="valueVector">The vector of values to interpolate</param>
        /// <returns></returns>
        public WingModelInterpolation(double[] axisVector, double[] valueVector)
        {
            if (axisVector.Length != valueVector.Length) return;
            //Process the axis array and the value array.
            //Run the calibration function, which returns the OrcWingParameters for the input vectors.
            X = axisVector;
            Y = valueVector;
            var orcParameters = WingModelFitter.FitWing(axisVector, valueVector,  Forward, Spot);
            WingParameters = orcParameters;
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="axisVector">The axis vector of values</param>
        /// <param name="valueVector">The vector of values to interpolate</param>
        /// <returns></returns>
        public static WingModelInterpolation Interpolate(double[] axisVector, double[] valueVector)
        {
            return axisVector.Length != valueVector.Length ? null : new WingModelInterpolation(axisVector, valueVector);
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="axisValue">The axis value</param>
        /// <returns></returns>
        public double ValueAt(double axisValue)
        {
            var result = ValueAt(axisValue, true);
            return result;
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="axisValue">The axis value</param>
        /// <param name="extrapolation">This is not currently implemented.</param>
        /// <returns></returns>
        public double ValueAt(double axisValue, bool extrapolation)
        {
            var result = OrcWingVol.Value(axisValue, WingParameters);
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
            if (x.Length == y.Length)
            {
                var orcParameters = WingModelFitter.FitWing(x, y, Forward, Spot);
                WingParameters = orcParameters;
            }
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            var copy = new WingModelInterpolation(X, Y)
            {
                Forward = Forward,
                Spot = Spot
            };
            return copy;
        }
    }
}