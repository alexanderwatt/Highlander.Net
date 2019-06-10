/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;

#endregion

//TODO: make the Interpolation method classes into generec classes 
//(That will improve performance and code readability)

namespace Orion.ModelFramework
{
	/// <summary>
	/// Abstract base class for point interpolations.
	/// </summary>
	/// <remarks>
	/// Classes derived from this class
	/// provide interpolated values from two sequences of equal length,
	/// representing discrete values of a variable and a function of
	/// the former, respectively.
	/// </remarks>
	public interface IInterpolation 
	{
		/// <summary>
		/// Interpolated value.
		/// </summary>
		/// <remarks>
		/// This method must be overridden to provide an implementation
		/// of the actual interpolation.
		/// </remarks>
        /// <param name="point"><c>IPoint</c> This can be a one or many dimensional point.</param>
		/// <param name="allowExtrapolation"></param>
		/// <returns><c>decimal</c> The interpolated value as a decimal for FpML compatibility.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when extrapolation has not been allowed and the passed value
		/// is outside the allowed range.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the class was not properly initialized.
		/// </exception>
        double ValueAt(double point, bool allowExtrapolation);

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
        void Initialize(double[] x, double[] y);

	    ///<summary>
	    /// The clone method makes a shallow copy of the current interpolation class.
	    ///</summary>
	    ///<returns></returns>
	    IInterpolation Clone();    
	}
}
