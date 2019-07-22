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

#region Usings

using Orion.ModelFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Numerics.Differentiation;
using Highlander.Numerics.Helpers;
using Highlander.Numerics.Utilities;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// Class that encapsulates functionality to perform one dimensional
    /// interpolation with piecewise cubic Hermite splines.
    /// The class does not support extrapolation.
    /// The interpolation method implemented in the class is documented in 
    /// Appendix A of the document "Caplet Bootstrap and Interpolation
    /// Methodologies," Ref. GS-14022008/1.
    /// </summary>
    public class CubicHermiteSplineInterpolation : IInterpolation
    {
        #region Private Constants

        /// <summary>
        /// Constant that defines the minimum number of (x,y) pairs required
        /// to perform Cubic Hermite Spline interpolation.
        /// </summary>
        private const int MinNumberXyPairs = 2;

        #endregion Private Constants

        #region Accessor Methods

        /// <summary>
        /// Public accessor method for the data table of (x,y) values.
        /// </summary>
        /// <value>Sorted list in which the keys are the x values and the
        /// values are the y values.</value>
        public SortedList<decimal, decimal> DataTable { get; private set; }

        /// <summary>
        /// Public accessor method for the index at the LEFT of the bounding
        /// interval.
        /// </summary>
        /// <value>Left (zero based) index of the interval that bounds the 
        /// target of the interpolation.</value>
        public int LeftIndex { get; private set; }

        /// <summary>
        /// Public accessor method for the normalised target.
        /// </summary>
        /// <value>Interpolation target transformed to the interval (0,1).
        /// </value>
        public decimal NormalisedTarget { get; private set; }

        /// <summary>
        /// Public accessor method for the index at the RIGHT of the bounding
        /// interval.
        /// </summary>
        /// <value>Right (zero based) index of the interval that bounds the 
        /// target of the interpolation.</value>
        public int RightIndex { get; private set; }

        #endregion Accessor Methods

        #region Private Business Logic Methods

        /// <summary>
        /// Computes the first derivatives at the knot points that bound the
        /// target of the interpolation.
        /// Post condition: private field _m0 is set.
        /// Post condition: private field _m1 is set.
        /// </summary>
        /// <param name="leftIndex">Zero based index at the left knot point
        /// of the bounding interval.</param>
        /// <param name="rightIndex">Zero based index at the right knot point
        /// of the bounding interval.</param>
        private void ComputeFirstDerivatives(int leftIndex, int rightIndex)
        {
            // Instantiate the object that will access the functionality to
            // compute the necessary derivatives.
            var numElements = DataTable.Keys.Count;
            var xArray = new decimal[numElements];
            var yArray = new decimal[numElements];
            for (var i = 0; i < numElements; ++i )
            {
                xArray[i] = DataTable.Keys[i];
                yArray[i] = DataTable.Values[i];
            }
            var derivObj =
                new CenteredFiniteDifferenceDerivative(xArray,
                                                      yArray);
            // Compute derivatives.
            _m0 = derivObj.ComputeFirstDerivative(leftIndex);
            _m1 = derivObj.ComputeFirstDerivative(rightIndex);
        }

        /// <summary>
        /// Computes the value of the Hermite Spline basis functions at the
        /// normalised target.
        /// Post condition: private field _h00 is set.
        /// Post condition: private field _h10 is set.
        /// Post condition: private field _h01 is set.
        /// Post condition: private field _h11 is set.
        /// </summary>
        /// <param name="t">Normalised target.</param>
        private void ComputeHermiteSplineBasisFunctions(decimal t)
        {
            // Compute the Hermite Spline basis functions.
            _h00 = HermiteSplineBasisFunctions.H00(t);
            _h10 = HermiteSplineBasisFunctions.H10(t);
            _h01 = HermiteSplineBasisFunctions.H01(t);
            _h11 = HermiteSplineBasisFunctions.H11(t);
        }

        /// <summary>
        /// Computes the normalised target. The end result will reside in the 
        /// interval (0,1).
        /// </summary>
        /// <param name="target">Value at which to compute the
        /// interpolation.</param>
        /// Post-condition: private field _t is set.
        private void ComputeNormalisedTarget(decimal target)
        {
            decimal xK = DataTable.Keys[LeftIndex];
            decimal xKPlusOne = DataTable.Keys[RightIndex];
            NormalisedTarget = (target - xK)/(xKPlusOne - xK);
        }

        /// <summary>
        /// Helper function used by the constructor to initialise the private
        /// fields.
        /// </summary>
        /// <param name="xArray">One dimensional array of x values.</param>
        /// <param name="yArray">One dimensional array of y values that is
        /// associated with each x value.</param>
        /// Precondition: ValidateConstructorArguments method has been called.
        private void InitialisePrivateFields(IEnumerable<decimal> xArray, decimal[] yArray)
        {
            DataTable = new SortedList<decimal, decimal>();
            // Map the method arguments into ordered (x,y) pairs.
            int i = 0; // index into the array of y values
            foreach (decimal x in xArray)
            {
                DataTable.Add(x, yArray[i]);
                ++i; // move to next y value
            }
            // Initialise the remaining private fields to their default values.
            _h00 = 0m;
            _h10 = 0m;
            _h01 = 0m;
            _h11 = 0m;            
            LeftIndex = 0;
            _m0 = 0m;
            _m1 = 0m;
            RightIndex = 0;
            NormalisedTarget = 0m;
        }

        /// <summary>
        /// Sets the interval of knot points that bound the target point of the
        /// interpolation.
        /// </summary>
        /// <param name="allowExtrapolation">Currently not implemented</param>
        /// <param name="target">Value at which to compute the
        /// interpolation. The target cannot be outside of the interval
        /// that bounds the knot points.</param>
        /// Post condition: private field _leftIndex is set.
        /// Post condition: private field _rightIndex is set.
        /// Post condition: private field _isTargetAtExtremeLeft is set.
        /// Post condition: private field _isTargetAtExtremeRight is set.
        /// Exception: ArgumentException
        private void FindBoundingInterval(decimal target, bool allowExtrapolation)
        {
            // Check that target point is not outside the interval of knot
            // points.
            int maxIndex = DataTable.Keys.Count - 1;
            bool isExtrapolationRequested =
                target < DataTable.Keys[0] || target > DataTable.Keys[maxIndex];
            if (!allowExtrapolation && isExtrapolationRequested)
            {
                const string errorMessage =
                    "Cubic Hermite Spline does not support extrapolation";
                throw new ArgumentException(errorMessage);
            }
            //if (allowExtrapolation)
            //{
                //if (decimal.Compare(DataTable.Keys[0], target) >= 0)
                //{
                //    // Target is at the extreme left.
                //    LeftIndex = 0;
                //    RightIndex = 1;
                //}
                //else if (decimal.Compare(DataTable.Keys[maxIndex], target) <= 0)
                //{
                //    // Target is at the extreme right.
                //    RightIndex = maxIndex;
                //    LeftIndex = maxIndex - 1;
                //}
                //else
                //{
                //    // Generic case.
                //    int i = 0;
                //    while (i <= maxIndex && DataTable.Keys[i] <= target)
                //    {
                //        ++i;
                //    }
                //    RightIndex = i;
                //    LeftIndex = i - 1;
                //}
            //}
            //else
            //{
            // Find the bounding interval.
            if (decimal.Compare(DataTable.Keys[0], target) == 0)
            {
                // Target is at the extreme left.
                LeftIndex = 0;
                RightIndex = 1;
            }
            else if (decimal.Compare(DataTable.Keys[maxIndex], target) == 0)
            {
                // Target is at the extreme right.
                RightIndex = maxIndex;
                LeftIndex = maxIndex - 1;
            }
            else
            {
                // Generic case.
                int i = 0;
                while (i <= maxIndex && DataTable.Keys[i] <= target)
                {
                    ++i;
                }
                RightIndex = i;
                LeftIndex = i - 1;
            }
            //}
        }

        /// <summary>
        /// Helper function used by the constructor to validate its arguments.
        /// </summary>
        /// <param name="xArray">One dimensional array of x values.</param>
        /// <param name="yArray">One dimensional array of y values that is
        /// associated with each x value.</param>
        /// Exception: ArgumentException.
        private static void ValidateConstructorArguments(decimal[] xArray,
                                                  ICollection<decimal> yArray)
        {
            // Check that array lengths are equal.
            if(xArray.Length != yArray.Count)
            {
                const string errorMessage =
                    "Cubic Hermite Spline interpolation needs equal length arrays";
                throw new ArgumentException(errorMessage);
            }
            // Check for sufficient data.
            if(xArray.Length < MinNumberXyPairs)
            {
                const string errorMessage =
                    "Cubic Hermite Spline interpolation needs at least two (x,y)";
                throw new ArgumentException(errorMessage);
            }
            // Check that the array of x values are in strict ascending order.
            int numElements = xArray.Length;
            for(int i = 1; i < numElements; ++i)
            {
                if(xArray[i] <= xArray[i-1])
                {
                    const string errorMessage =
                        "Cubic Hermite Spline interpolation needs sorted x values";
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        #endregion Private Business Logic Methods

        #region Private Fields

        /// <summary>
        /// Value of the Hermite Spline basis function h00.
        /// [Default: 0]
        /// </summary>
        private decimal _h00;

        /// <summary>
        /// Value of the Hermite Spline basis function h10.
        /// [Default: 0]
        /// </summary>
        private decimal _h10;

        /// <summary>
        /// Value of the Hermite Spline basis function h01.
        /// [Default: 0]
        /// </summary>
        private decimal _h01;

        /// <summary>
        /// Value of the Hermite Spline basis function h11.
        /// [Default: 0]
        /// </summary>
        private decimal _h11;

        /// <summary>
        /// Derivative at the left index.
        /// [Default: 0]
        /// </summary>
        private decimal _m0;

        /// <summary>
        /// Derivative at the right index.
        /// [Default: 0]
        /// </summary>
        private decimal _m1;

        #endregion Private Fields

        ///<summary>
        ///</summary>
        ///<param name="point"></param>
        ///<param name="allowExtrapolation"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public double ValueAt(double point, bool allowExtrapolation)
        {
            var left = Convert.ToDouble(DataTable.Keys[0]);
            int maxIndex = DataTable.Keys.Count - 1;
            var right = Convert.ToDouble(DataTable.Keys[maxIndex]);
            if (allowExtrapolation && point < left) return ValueAt(left);
            if (allowExtrapolation && point > right) return ValueAt(right);
            return ValueAt(point);
        }

        /// <summary>
        /// Implements one dimensional Cubic Spline Interpolation.
        /// </summary>
        /// <param name="point">Value at which to compute the interpolation.
        /// </param>
        /// <returns>
        /// Interpolated value at the desired target.
        /// </returns>
        public double ValueAt(double point)
        {
            FindBoundingInterval(Convert.ToDecimal(point), true);
            ComputeNormalisedTarget(Convert.ToDecimal(point));
            ComputeHermiteSplineBasisFunctions(NormalisedTarget);
            ComputeFirstDerivatives(LeftIndex, RightIndex);
            // Compute and return the interpolation value.
            var p0 = DataTable.Values[LeftIndex];
            var p1 = DataTable.Values[RightIndex];
            var h =
                DataTable.Keys[RightIndex] - DataTable.Keys[LeftIndex];
            var interpolationValue =
                _h00 * p0 + _h10 * h * _m0 + _h01 * p1 + _h11 * h * _m1;
            return (double)interpolationValue;
            //return ValueAt(point, true);
        }

        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<param name="y"></param>
        ///<exception cref="NotImplementedException"></exception>
        public void Initialize(double[] x, double[] y)
        {
            Initialize(ArrayUtilities.ArrayToDecimal(x), ArrayUtilities.ArrayToDecimal(y));
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            var copy = new CubicHermiteSplineInterpolation();
            copy.Initialize(DataTable.Keys.ToArray(), DataTable.Values.ToArray());
            return copy;
        }

        /// <summary>
        /// Constructor for the <see cref="CubicHermiteSplineInterpolation"/>
        /// class that transfers the array of x and y values into the data 
        /// structure that stores (x,y) points into ascending order based  
        /// on the x value of each point.
        /// </summary>
        /// <param name="xArray">One dimensional array of x values. The array of
        /// x values is not required to be in ascending order.</param>
        /// <param name="yArray">One dimensional array of y values that is
        /// associated with each x value.</param>
        /// Precondition: Lengths of the two input arrays are equal.
        /// Precondition: Length of the array of x values is >= 2.
        /// Precondition: Array of x values is in strict ascending order.
        public void Initialize(decimal[] xArray, decimal[] yArray)
        {
            ValidateConstructorArguments(xArray, yArray);
            InitialisePrivateFields(xArray, yArray);
        }
    }
}