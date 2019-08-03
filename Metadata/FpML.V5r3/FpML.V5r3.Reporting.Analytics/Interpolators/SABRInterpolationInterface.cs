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
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Interpolators
{
    /// <summary>
    /// A wrapper to connect ExcelAPI interpolation functions to underlying analytics interp classes
    /// </summary>
    public class SABRInterpolationInterface
    {
        #region Static Fields
        /// <summary>
        /// Singleton instance controlling the interface
        /// </summary>
        private static SABRInterpolationInterface _instance;

        #endregion

        #region Singleton Interface

        /// <summary>
        /// Protect this class from multiple instances
        /// </summary>
        private SABRInterpolationInterface()
        { }

        /// <summary>
        /// Static Instance method to access this class
        /// </summary>
        /// <returns></returns>
        public static SABRInterpolationInterface Instance()
        {
            return _instance ?? (_instance = new SABRInterpolationInterface());
        }

        #endregion

        #region Linear Interpolation
        /// <summary>
        /// Compute a one dimensional (piecewise) Linear interpolation. Extrapolation is flat-line.
        /// </summary>
        /// <param name="xValues">One dimensional array of x-values. Array is not required to be in ascending order.</param>
        /// <param name="yValues">One dimensional array of known y-values. Length of the array must be equal to the length of the XArray parameter.</param>
        /// <param name="target">Value at which to compute the interpolation.</param>
        /// <returns></returns>
        public double LinearInterpolate(Double[] xValues, Double[] yValues, double target)
        {
            if (xValues == null)
                return 0;
            if (yValues == null)
                return 0;
            var li = new LinearInterpolation();
            li.Initialize(xValues, yValues);
            return li.ValueAt(target, true);
        }

        #endregion

        #region Bilinear Interpolation
        /// <summary>
        /// Compute a Bilinear interpolation at the ordered pair (ColumnTarget, RowTarget).
        /// Extrapolation is flat-line in both the Column and Row dimensions.
        /// </summary>
        /// <param name="columnLabels">One dimensional array arranged in strict ascending order that governs HORIZONTAL interpolation.</param>
        /// <param name="rowLabels">One dimensional array arranged in strict ascending order that governs VERTICAL interpolation.</param>
        /// <param name="dataTable">Two dimensional array of known values with size equal to RowLabels x ColumnLabels.</param>
        /// <param name="columnTarget">Column (horizontal) target of the interpolation.</param>
        /// <param name="rowTarget">Row (vertical) target of the interpolation.</param>
        /// <returns></returns>
        public double BilinearInterpolate(double[] columnLabels, double[] rowLabels, double[,] dataTable,
                                          double columnTarget, double rowTarget)
        {
            if (columnLabels == null)
                return 0;
            if (rowLabels == null)
                return 0;
            if (dataTable == null)
                return 0;

            var bil = new BilinearInterpolation(ref columnLabels, ref rowLabels, ref dataTable);
            return bil.Interpolate(columnTarget, rowTarget);
        }

        #endregion

        #region CubicHermiteSpline Interpolation
        /// <summary>
        /// Compute an interpolated value from a set of known (x,y) values by Cubic Hermite Spline interpolation.
        /// Extrapolation is NOT available.
        /// </summary>
        /// <param name="xArray">Array (row/column) of x-values.</param>
        /// <param name="yArray">Array (row/column) of y-values.</param>
        /// <param name="target">Value at which to compute the interpolation.</param>
        /// <returns></returns>
        public double CubicHermiteSplineInterpolate(double[] xArray, double[] yArray, double target)
        {
            if (xArray == null)
                return 0;
            if (yArray == null)
                return 0;
            var chsi = new CubicHermiteSplineInterpolation();
            chsi.Initialize(xArray, yArray);//CubicSplineInterpolation.InterpolateAkima(xArray, yArray);
            return chsi.ValueAt(target, true);
        }

        #endregion

        #region Private Methods

        ///// <summary>
        ///// Convert a 2D decimal array to a 1D array. If I could unbox the array it would be a generic
        ///// </summary>
        ///// <param name="array"></param>
        ///// <returns></returns>
        //private static decimal[] Convert2DTo1DDecimalArray(object[,] array)
        //{
        //    var rb = array.GetUpperBound(0) + 1;
        //    var cb = array.GetUpperBound(1) + 1;

        //    var ub = (rb > cb) ? rb : cb;

        //    var retval = new decimal[ub];

        //    for (var row = 0; row < ub; row++)
        //        retval[row] = (rb > cb) ? Convert.ToDecimal(array[row, 0], CultureInfo.CurrentCulture) :
        //                                                                                                   Convert.ToDecimal(array[0, row], CultureInfo.CurrentCulture);

        //    return retval;
        //}

        ///// <summary>
        ///// Convert a 2D decimal array to a 1D array. If I could unbox the array it would be a generic
        ///// </summary>
        ///// <param name="array"></param>
        ///// <returns></returns>
        //private static double[] Convert2DTo1DDoubleArray(object[,] array)
        //{
        //    var rb = array.GetUpperBound(0) + 1;
        //    var cb = array.GetUpperBound(1) + 1;

        //    var ub = (rb > cb) ? rb : cb;

        //    var retval = new double[ub];

        //    for (var row = 0; row < ub; row++)
        //        retval[row] = (rb > cb) ? Convert.ToDouble(array[row, 0], CultureInfo.CurrentCulture) :
        //                                                                                                  Convert.ToDouble(array[0, row], CultureInfo.CurrentCulture);

        //    return retval;
        //}

        ///// <summary>
        ///// Convert a 2D array to an array of arrays
        ///// </summary>
        ///// <param name="array"></param>
        ///// <returns></returns>
        //private static double[,] Convert2DArray(object[,] array)
        //{
        //    var ubRows = array.GetUpperBound(0) + 1;
        //    var ubCols = array.GetUpperBound(1) + 1;
        //    var retval = new double[ubRows, ubCols];

        //    for (var row = 0; row < ubRows; row++)
        //    {
        //        for (var col = 0; col < ubCols; col++)
        //            retval[row, col] = Convert.ToDouble(array[row, col], CultureInfo.CurrentCulture);
        //    }
        //    return retval;
        //}

        #endregion
    }
}