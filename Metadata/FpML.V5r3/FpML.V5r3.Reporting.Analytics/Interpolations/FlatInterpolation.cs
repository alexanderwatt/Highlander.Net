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
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations
{
	/// <summary>
	/// Linear interpolation between discrete points.
	/// </summary>
	public class FlatInterpolation : IInterpolation
    {
	    private double _value;

        /// <param name="y">Sample points (N+1), sorted ascending</param>
	    protected FlatInterpolation(double y)
	    {
	        _value = y;
	    }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public FlatInterpolation()
        {}

        /// <summary>
        /// Create a constant flat interpolation from a value y.
        /// </summary>
        public static FlatInterpolation Interpolate(double y)
	    {
	        return new FlatInterpolation(y);
	    }

	    /// <summary>
	    /// Perform a model interpolation on a vector of values
	    /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
	    /// </summary>
	    /// <param name="axisValue">The axis value</param>
	    /// <param name="extrapolation">This is not currently implemented.</param>
	    /// <returns></returns>
	    public double ValueAt(double axisValue, bool extrapolation)
	    {
	        return _value;
	    }

        /// <summary>
        /// Initialize a class constructed by the default constructor.
        /// </summary>
        /// <remarks>
        /// The sequence of values for x are ignored.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the passed lists do not have the same size
        /// or do not provide enough points to interpolate.
        /// </exception>
        public void Initialize(double y)
        {
            _value = y;
        }

        /// <summary>
        /// Initialize a class constructed by the default constructor.
        /// </summary>
        /// <remarks>
        /// The sequence of values for x are ignored.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the passed lists do not have the same size
        /// or do not provide enough points to interpolate.
        /// </exception>
        public void Initialize(double[] x, double[] y)
        {
            _value = y[0];
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new FlatInterpolation();
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
        public double ValueAt(double x)
		{
            return _value;
		}
	}
}
