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

namespace Highlander.Reporting.Analytics.V5r3.Interpolations
{
    /// <summary>
    /// Class that encapsulates functionality for two dimensional linear
    /// interpolation.
    /// The convention used throughout the class is that Column Labels are
    /// oriented horizontally and Row Labels are oriented vertically, as shown
    /// in the following diagram:
    ///                 +---Column Labels---+
    ///                 +
    ///                 |
    ///                 Row Labels
    ///                 |
    ///                 +
    /// </summary>
    public class BilinearInterpolation : ITwoDimensionalInterpolation
    {
        #region Constructor

        /// <summary>
        /// Constructor for <see cref="BilinearInterpolation"/> class that
        /// validates inputs and then maps these arguments into appropriate
        /// private fields.
        /// </summary>
        /// <param name="columnLabels">Data structure used to store the column 
        /// labels, which govern the interpolation in the horizontal direction.
        /// Precondition: Column labels must be arranged in strict ascending
        /// order.</param>
        /// <param name="rowLabels">Data structure used to store the row
        /// labels, which govern the interpolation in the vertical direction.
        /// Precondition: Row labels must be arranged in strict ascending
        ///  order.</param>
        /// <param name="dataTable">Data structure used to store the table
        /// of known values, which are arranged at the intersection of a
        /// particular row and column.
        /// Precondition: Number of columns in the data table must equal
        /// the length of the column labels array, and the number of rows
        /// in the data table must equal the length of the row labels array.
        /// </param>
        public BilinearInterpolation(ref double[] columnLabels,
                                     ref double[] rowLabels,
                                     ref double[,] dataTable)
        {
            // Validate data.
            string errorMessage = "";
            bool isDataValid = CheckDataQuality(ref columnLabels,
                                                ref rowLabels,
                                                ref dataTable,
                                                ref errorMessage);
            if (!isDataValid)
            {
                throw new Exception(errorMessage);
            }
            // Map arguments to their equivalent private field.
            int numColumns = columnLabels.Length;
            _columnLabels = new double[numColumns];
            Array.Copy(columnLabels, _columnLabels, numColumns);
            int numRows = rowLabels.Length;
            _rowLabels = new double[numRows];
            Array.Copy(rowLabels, _rowLabels, numRows);
            _dataTable = new double[numRows, numColumns];
            for(int i = 0; i < numRows; ++i)
            {
                for(int j = 0; j < numColumns; ++j)
                {
                    _dataTable[i, j] = dataTable[i, j];
                }
            }
        }

        #endregion Constructor

        #region Public Business Logic Methods

        /// <summary>
        /// Encapsulates the logic to perform bilinear interpolation at the
        /// ordered pair (columnTarget, rowTarget).
        /// Flat-line extrapolation is applied if the target values fall outside
        /// the range of [min label, max label] in either the row or column
        /// dimensions.
        /// </summary>
        /// <param name="columnTarget">The column (horizontal) target.</param>
        /// <param name="rowTarget">The row (vertical) target.</param>
        /// <returns>
        /// Interpolated value at the desired two dimensional target.
        /// </returns>
        public double Interpolate(double columnTarget, double rowTarget)
        {
            // Locate the indices of the bounding grid.
            int columnLBound = LowerBound(ref _columnLabels, columnTarget);
            int columnUBound = UpperBound(ref _columnLabels, columnTarget);
            int rowLBound = LowerBound(ref _rowLabels, rowTarget);
            int rowUBound = UpperBound(ref _rowLabels, rowTarget);

            // Determine the values at the grid point that bound the
            // two dimensional target.
            double point0 = _dataTable[rowLBound, columnLBound];
            double point1 = _dataTable[rowLBound, columnUBound];
            double point2 = _dataTable[rowUBound, columnUBound];
            double point3 = _dataTable[rowUBound, columnLBound];

            // Compute weights - these should be in the range [0,1).
            double d1 =
                _columnLabels[columnUBound] - _columnLabels[columnLBound];
            double d2 = _rowLabels[rowUBound] - _rowLabels[rowLBound];
            double t = (columnLBound == columnUBound) ?
                                                          0.0 : (columnTarget - _columnLabels[columnLBound])/d1;
            double u = (rowLBound == rowUBound) ?
                                                    0.0 : (rowTarget - _rowLabels[rowLBound])/d2;
          
            // Compute and return the interpolated value.
            double interpolationValue = (1.0 - t)*(1.0 - u)*point0 +
                                        t*(1.0 - u)*point1 + t*u*point2 + (1.0 - t)*u*point3;

            return interpolationValue;
        }

        #endregion Public Business Logic Methods

        #region Private Business Logic Methods

        /// <summary>
        /// Finds the index of the largest element in an array that is 
        /// less than or equal to a user specified value. 
        /// </summary>
        /// <param name="array">Array in which to search for the lower 
        /// bound.</param>
        /// Post conditions:
        /// 1) Array is not empty;
        /// 2) Array is sorted into strict ascending order.
        /// <param name="target">Target value.</param>
        /// <returns>
        /// Zero based array index for the lower bound.
        /// </returns>
        private static int LowerBound(ref double[] array, double target)
        {
            // Record minimum and maximum values in the array.
            int numElements = array.Length;
            double min = array[0];
            double max = array[numElements - 1];
            // Set the position of the search location.
            int pos = numElements - 1;
            // Find and return the lower bound.
            if(target <= min)
            {
                // Lower bound at the left end.
                pos = 0;
            }
            else if(target >= max)
            {
                // Lower bound at the right end.
                pos = numElements - 1; 
            }
            else
            {
                // Generic case.
                while(pos >= 0 && target < array[pos])
                {
                    // Move left to the next array element.
                    --pos;
                }
            }
            return pos;
        }

        /// <summary>
        /// Finds the index of the smallest element in an array that is 
        /// greater than or equal to a user specified value. 
        /// </summary>
        /// <param name="array">Array in which to search for the upper 
        /// bound.</param>
        /// Post conditions:
        /// 1) Array is not empty;
        /// 2) Array is sorted into strict ascending order.
        /// <param name="target">Target value.</param>
        /// <returns>
        /// Zero based array index for the upper bound.
        /// </returns>
        private static int UpperBound(ref double[] array, double target)
        {
            // Record minimum and maximum values in the array.
            int numElements = array.Length;
            double min = array[0];
            double max = array[numElements - 1];
            // Set the position of the search location.
            int pos = 0;
            // Find and return the upper bound.
            if (target <= min)
            {
                // Upper bound at the left end.
                pos = 0;
            }
            else if (target >= max)
            {
                // Upper bound at the right end.
                pos = numElements - 1; 
            }
            else
            {
                // Generic case.
                while (pos < numElements && target > array[pos])
                {
                    // Move right to the next array element.
                    ++pos;
                }
            }
            return pos;
        }

        #endregion Private Business Logic Methods

        #region Private Validation Methods

        /// <summary>
        /// Checks for invalid input data passed to the constructor.
        /// </summary>
        /// <param name="columnLabels">One dimensional array of column labels 
        /// to be tested.</param>
        /// <param name="rowLabels">One dimensional array of row labels to be
        /// tested.</param>
        /// <param name="dataTable">Two dimensional array of known data.</param>
        /// <param name="errorMessage">Storage for the description of the 
        /// error message that is used when invalid data is detected.</param>
        /// <returns>True if all data valid, otherwise false.</returns>
        private static bool CheckDataQuality(ref double[] columnLabels,
                                             ref double[] rowLabels,
                                             ref double[,] dataTable,
                                             ref string errorMessage)
        {
            // Return variable that indicates whether invalid data found.
            bool isDataValid = false;
            errorMessage = "";
            // Check for an empty array.
            bool isArrayEmpty = (columnLabels.Length == 0 ||
                                 rowLabels.Length == 0 ||
                                 dataTable.Length == 0);
            if (isArrayEmpty)
            {
                errorMessage = "Bilinear interpolation found an empty array.";
                return isDataValid;
            }
            // Check that column and row labels are sorted into ascending order.
            bool areLabelsSorted =
                IsArraySorted(ref columnLabels) && IsArraySorted(ref rowLabels);
            if (!areLabelsSorted)
            {
                errorMessage =
                    "Bilinear interpolation requires column/row labels in ascending order";
                return isDataValid;
            }
            // Check array dimensions.
            bool areColumnSizesCorrect =
                (dataTable.GetUpperBound(1) + 1 == columnLabels.Length);
            bool areRowSizesCorrect =
                (dataTable.GetUpperBound(0) + 1 == rowLabels.Length);
            if(!areColumnSizesCorrect || !areRowSizesCorrect)
            {
                errorMessage =
                    "Bilinear interpolation found incorrect array dimensions.";

                return isDataValid;
            }
            // All checks passed.
            isDataValid = true;
            return isDataValid;    
        }

        /// <summary>
        /// Checks whether an array is sorted into strict ascending order.
        /// </summary>
        /// <param name="array">Array to test.</param>
        /// <returns>
        /// 	<c>true</c> if array sorted into strict ascending order,
        ///     otherwise <c>false</c>.
        /// </returns>
        private static bool IsArraySorted(ref double[] array)
        {
            // Declare and initialise the return variable.
            bool isArraySorted = true;
            // Loop through the array and check that the contents
            // are in strict ascending order.
            int numElements = array.Length;
            for (int i = 1; i < numElements; ++i)
            {
                if(array[i] <= array[i - 1])
                {
                    isArraySorted = false;
                    break;
                }
            }
            return isArraySorted;
        }


        #endregion Private Validation Methods

        #region Private Fields

        /// <summary>
        /// Data structure used to store the column labels, which govern the
        /// interpolation in the horizontal direction.
        /// </summary>
        private double[] _columnLabels;

        /// <summary>
        /// Data structure used to store the table of known values, which are
        /// arranged at the intersection of a particular row and column. 
        /// </summary>
        private readonly double[,] _dataTable;
        
        /// <summary>
        /// Data structure used to store the row labels, which govern the
        /// interpolation in the vertical direction.
        /// </summary>
        private double[] _rowLabels;

        #endregion Private Fields

        /// <summary>
        /// Encapsulates the particular methodology, for example Linear or
        /// Cubic Spline, used to compute a two dimensional interpolation.
        /// Concrete derived classes must implement this method.
        /// </summary>
        /// <param name="columnTarget">The column (horizontal) target.</param>
        /// <param name="rowTarget">The row (vertical) target.</param>
        /// <returns>Interpolated value at the desired two dimensional target.
        /// </returns>
        public double ValueAt(double columnTarget, double rowTarget)
        {
            throw new NotImplementedException();
        }
    }
}