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

namespace Highlander.Numerics.Differentiation
{
    /// <summary>
    /// Concrete implementation of the interface IFiniteDifferenceDerivative.
    /// Class encapsulates the functionality to compute numerical derivatives
    /// by a Centered Finite Difference scheme.
    /// </summary>
    public class CenteredFiniteDifferenceDerivative : IFiniteDifferenceDerivative
    {
        #region Constructor

        /// <summary>
        /// Constructor for the <see cref="CenteredFiniteDifferenceDerivative"/>
        /// class. Internally, the constructor maps each array into its
        /// appropriate private field.
        /// No data validation is performed by the class because it is assumed
        /// that the client has performed all necessary data validation.
        /// </summary>
        /// <param name="xArray">One dimensional array of x values.</param>
        /// <param name="yArray">One dimensional array of y values</param>
        /// Precondition: length of the x and y arrays are identical, and is
        /// at least two.
        /// Precondition: x array in strict ascending order.
        /// Precondition: x and y arrays form ordered (x,y) pairs.
        public CenteredFiniteDifferenceDerivative(decimal[] xArray,
                                                 decimal[] yArray)
        {
            // Map arguments to their private fields.
            _xArray = new decimal[xArray.Length];
            xArray.CopyTo(_xArray, 0);
            _yArray = new decimal[yArray.Length];
            yArray.CopyTo(_yArray, 0);
        }

        /// <summary>
        /// Initializes a NumericalDerivative class with the default 3 point center difference method.
        /// </summary>
        public CenteredFiniteDifferenceDerivative() //: this(3, 1)
        {}

        #endregion Constructor

        #region Public Business Logic Methods

        /// <summary>
        /// Encapsulates the methodology to compute numerical first derivatives
        /// by a Centered Finite Difference scheme. If the derivative requested
        /// coincides with one of the ends of the grid of knot points, then a
        /// one sided Finite Difference is used to compute the derivative.
        /// </summary>
        /// <param name="index">Zero based index in the array of x and y
        /// values at which the derivative is required.</param>
        /// <returns>Numerical first derivative.</returns>
        public decimal ComputeFirstDerivative(int index)
        {
            decimal deltaX;
            decimal deltaY;
            decimal firstDerivative; // return variable
            // Compute the first derivative based on the index location.
            if(index == 0)
            {
                // Derivative at extreme LEFT.
                deltaX = _xArray[1] - _xArray[0];
                deltaY = _yArray[1] - _yArray[0];
                firstDerivative = deltaY/deltaX;
            }
            else if(index == _xArray.Length - 1)
            {
                // Derivative at extreme RIGHT.
                var maxIndex = _xArray.Length - 1;
                deltaX = _xArray[maxIndex] - _xArray[maxIndex - 1];
                deltaY = _yArray[maxIndex] - _yArray[maxIndex - 1];
                firstDerivative = deltaY/deltaX;
            }
            else
            {
                // Derivative at an interior point.
                deltaX = _xArray[index + 1] - _xArray[index];
                deltaY = _yArray[index + 1] - _yArray[index];
                var forwardDerivative = deltaY/deltaX;
                deltaX = _xArray[index] - _xArray[index - 1];
                deltaY = _yArray[index] - _yArray[index - 1];
                var backwardDerivative = deltaY/deltaX;
                firstDerivative =
                    (forwardDerivative + backwardDerivative)/2m;
            }
            return firstDerivative;
        }

        #endregion Public Business Logic Methods

        #region Private Fields

        /// <summary>
        /// Array of x values.
        /// Convention is that values used in the computation of derivatives
        /// are arranged into the from (x, y).
        /// </summary>
        private readonly decimal[] _xArray;

        /// <summary>
        /// Array of y values.
        /// Convention is that values used in the computation of derivatives
        /// are arranged into the from (x, y).
        /// </summary>
        private readonly decimal[] _yArray;

        #endregion Private Fields
    }
}