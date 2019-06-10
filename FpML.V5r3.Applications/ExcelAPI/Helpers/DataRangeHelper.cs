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

#region Usings

using System;
using System.Collections.Generic;
using Orion.Analytics.Helpers;
using Orion.Analytics.LinearAlgebra;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Helpers
{
    /// <summary>
    /// This is a lightweight wrapper that interfaces between the SABRInterface class and the ExcelAPI.
    /// The wrapper acts as a layer that will map Excel specific data/operations to Interface expected types
    /// </summary>
    public static class DataRangeHelper
    {
        #region Equities

        [Serializable]
        public class ZeroCurveRange
        {
            public DateTime RateDate;
            public double Rate;
        }

        [Serializable]
        public class DividendRange
        {
            public DateTime DivDate;
            public double DivAmt;
        }

        [Serializable]
        public class WingParamsRange
        {
            public double CurrentVolatility;
            public double SlopeReference;
            public double PutCurvature;
            public double CallCurvature;
            public double DownCutOff;
            public double UpCutOff;
            public double VolChangeRate;
            public double SlopeChangeRate;
            public double SkewSwimmingnessRate;
            public double DownSmoothingRange;
            public double UpSmoothingRange;
            public double ReferenceForward;

        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="rawTable"></param>
        /// <param name="convertPercentage">This is defaulted to be true. If false assume no conversion</param>
        /// <returns>0 based 2D array</returns>
        public static object[,] RedimensionVolatilityGrid(object[,] rawTable, bool convertPercentage)
        {
            // Set the upper/lower bounds of the converted array and adjust the array we write to accordingly
            int hi1D = rawTable.GetUpperBound(0);
            int hi2D = rawTable.GetUpperBound(1);
            //// We will also strip out the additional column that has no purpose (if it still exists)
            //// This requires a cell by cell copy (it will be slow)
            //if (rawTable[0, 1] == null || rawTable[0, 1].ToString() == "Swap Tenor")
            //{
            //    object[,] trimmedArray = new object[hi1D + 1, hi2D];

            //    for (int i = 0; i <= hi1D; i++)
            //    {
            //        int k = 0;
            //        for (int j = 0; j <= hi2D; j++)
            //        {
            //            if(j!=1)
            //            {
            //                trimmedArray[i, k] = rawTable[i, j];
            //                k++;
            //            }
            //        }
            //    }
            //    rawTable = trimmedArray;
            //    hi1D = rawTable.GetUpperBound(0);
            //    hi2D = rawTable.GetUpperBound(1);
            //}
            var zeroBasedArray = new object[hi1D + 1, hi2D + 1];
            for (int row = 0; row <= hi1D; row++)
            {
                for (int col = 0; col <= hi2D; col++)
                {
                    if (rawTable[row, col] != null && rawTable[row, col] is double && convertPercentage)
                        zeroBasedArray[row, col] = Convert.ToDecimal((double)rawTable[row, col] / 100);
                    else
                        zeroBasedArray[row, col] = rawTable[row, col];
                }
            }
            return zeroBasedArray;
        }

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="rawTable"></param>
        /// <returns>0 based 2D array</returns>
        public static object[,] RedimensionAssetGrid(object[,] rawTable)
        {
            // Set the upper/lower bounds of the converted array and adjust the array we write to accordingly
            int rowCount = rawTable.GetUpperBound(0);
            int columnCount = rawTable.GetUpperBound(1);
            // Check whether there is a header row
            int startRow = 0;
            if (rawTable.GetUpperBound(0) > 0 && rawTable.GetUpperBound(1) > 1 && (rawTable[0, 2] == null
                || !(rawTable[0, 2] is double || rawTable[0, 2] is int)))
            {
                startRow = 1;
            }
            // We will also strip out the additional column that has no purpose (if it still exists)
            // This requires a cell by cell copy (it will be slow)
            if (rawTable[startRow, 1] == null || rawTable[startRow, 1].ToString() == "Swap Tenor")
            {
                var trimmedArray = new object[rowCount + 1, columnCount];
                for (int i = 0; i <= rowCount; i++)
                {
                    int k = 0;
                    for (int j = 0; j <= columnCount; j++)
                    {
                        if (j != 1)
                        {
                            trimmedArray[i, k] = rawTable[i, j];
                            k++;
                        }
                    }
                }
                rawTable = trimmedArray;
                rowCount = rawTable.GetUpperBound(0);
                columnCount = rawTable.GetUpperBound(1);
            }
            var zeroBasedArray = new object[rowCount - startRow + 1, columnCount + 1];
            for (int row = startRow; row <= rowCount; row++)
            {
                for (int col = 0; col <= columnCount; col++)
                {
                    if (row == startRow)
                        zeroBasedArray[row - startRow, col] = rawTable[row, col];
                    else
                    {
                        if (rawTable[row, col] != null && rawTable[row, col] is double)
                            zeroBasedArray[row - startRow, col] = Convert.ToDecimal(rawTable[row, col]) / 100.0m; // / 100.0m;
                        else
                            zeroBasedArray[row - startRow, col] = rawTable[row, col];
                    }
                }
            }
            return zeroBasedArray;
        }

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="rawTable"></param>
        /// <returns>0 based 2D array</returns>
        public static object[,] RedimensionFullCapVolatilities(object[,] rawTable)
        {
            // Set the upper/lower bounds of the converted array and adjust the array we write to accordingly
            int hi1D = rawTable.GetUpperBound(0);
            int hi2D = rawTable.GetUpperBound(1);
            var zeroBasedArray = new object[hi1D, hi2D];
            for (int row = 0; row < hi1D; row++)
            {
                for (int col = 0; col < hi2D; col++)
                {
                    if (rawTable[row + 1, col + 1] != null && rawTable[row + 1, col + 1] is double &&
                        col > 0)
                        zeroBasedArray[row, col] = Convert.ToDecimal(rawTable[row + 1, col + 1]) / 100.0m; // / 100.0m;
                    else
                        zeroBasedArray[row, col] = rawTable[row + 1, col + 1];
                }
            }
            return zeroBasedArray;
        }

        //private static object[] ConvertToObjectArray(decimal[] values)
        //{
        //    var result = new object[values.Length];
        //    int i = 0;
        //    foreach (decimal value in values)
        //        result[i++] = value;

        //    return result;
        //}

        /// <summary>
        /// A range will devolve to a 2d array of object. This method will take this and generate
        /// an array of strings. This is required by the Interpolated clibration engine routines.
        /// </summary>
        public static string[] ConvertRangeToStringArray(object[,] objHandles)
        {
            var handleList = new List<string>();
            // object[,] objHandles = (object[,])((Range)calibrationRange).get_Value(Type.Missing);
            int lo1D = objHandles.GetLowerBound(0);
            int hi1D = objHandles.GetUpperBound(0);
            int lo2D = objHandles.GetLowerBound(1);
            for (int idx = lo1D; idx <= hi1D; idx++)
                if (objHandles[idx, lo2D] != null)
                    handleList.Add(objHandles[idx, lo2D].ToString());
            return handleList.ToArray();
        }

        public static object[,] ToRange(double[,] inputRange)
        {
            if (inputRange == null) return null;
            int rowCount = inputRange.GetLength(0);
            int colCount = inputRange.GetLength(1);
            var result = new object[rowCount, colCount];
            var rowIndex = 0;
            var colIndex = 0;
            if (rowCount > 1 || colCount > 1)
            {
                for (int i = inputRange.GetLowerBound(0); i <= inputRange.GetUpperBound(0); i++)
                {
                    for (int j = inputRange.GetLowerBound(1); j <= inputRange.GetUpperBound(1); j++)
                    {
                        result[rowIndex, colIndex] = inputRange[i, j];
                        colIndex++;
                    }
                    colIndex = 0;
                    rowIndex++;
                }
            }
            return result;
        }

        public static T[,] ToMatrix<T>(Excel.Range inputRange)
        {
            if (inputRange == null) return null;
            int rowCount = inputRange.Rows.Count;
            int colCount = inputRange.Columns.Count;
            var result = new T[rowCount, colCount];
            var rowIndex = 0;
            var colIndex = 0;
            if (rowCount > 1 || colCount > 1)
            {
                var range = inputRange.Value[System.Reflection.Missing.Value] as object[,];
                if (range != null)
                    for (int i = range.GetLowerBound(0); i <= range.GetUpperBound(0); i++)
                    {
                        for (int j = range.GetLowerBound(1); j <= range.GetUpperBound(1); j++)
                        {
                            result[rowIndex, colIndex] = (T) range[i, j];
                            colIndex++;
                        }
                        colIndex = 0;
                        rowIndex++;
                    }
            }
            else
            {
                result[0, 0] = (T)inputRange.Value[System.Reflection.Missing.Value];
            }
            return result;
        }

        public static object[,] ToMatrix(Excel.Range inputRange)
        {
            if (inputRange == null) return null;
            int rowCount = inputRange.Rows.Count;
            int colCount = inputRange.Columns.Count;
            var result = new object[rowCount, colCount];
            var rowIndex = 0;
            var colIndex = 0;
            if (rowCount > 1 || colCount > 1)
            {
                var range = inputRange.Value[System.Reflection.Missing.Value] as object[,];
                if (range != null)
                    for (int i = range.GetLowerBound(0); i <= range.GetUpperBound(0); i++)
                    {
                        for (int j = range.GetLowerBound(1); j <= range.GetUpperBound(1); j++)
                        {
                            result[rowIndex, colIndex] = range[i, j];
                            colIndex++;
                        }
                        colIndex = 0;
                        rowIndex++;
                    }
            }
            else
            {
                result[0, 0] = inputRange.Value[System.Reflection.Missing.Value];
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Array TrimNulls(Array array)
        {
            //  find the last non null row
            //
            int index1LowerBound = array.GetLowerBound(1);
            int nonNullRows = 0;
            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                object o = array.GetValue(i, index1LowerBound);
                if (o == null || o.ToString() == String.Empty)
                    break;
                ++nonNullRows;
            }
            //  get the element type
            //
            Type elementType = array.GetType().GetElementType();
            if (elementType == null) return null;
            {
                var result = Array.CreateInstance(elementType, new[] { nonNullRows, array.GetLength(1) },
                    new[] { array.GetLowerBound(0), array.GetLowerBound(1) });
                for (int i = array.GetLowerBound(0); i < array.GetLowerBound(0) + nonNullRows; ++i)
                {
                    for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                    {
                        var value = array.GetValue(i, j);
                        result.SetValue(value, i, j);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange"></param>
        /// <returns></returns>
        public static object[,] RangeToMatrix(object[,] inputRange)
        {
            int rowCount = inputRange.GetLength(0);
            int colCount = inputRange.GetLength(1);
            var result = new object[rowCount, colCount];
            var rowIndex = 0;
            var colIndex = 0;
            for (int i = inputRange.GetLowerBound(0); i <= inputRange.GetUpperBound(0); i++)
            {
                for (int j = inputRange.GetLowerBound(1); j <= inputRange.GetUpperBound(1); j++)
                {
                    result[rowIndex, colIndex] = inputRange[i, j];
                    colIndex++;
                }
                colIndex = 0;
                rowIndex++;
            }
            return result;
        }

        /// <summary>
        /// Converts an object matrix to a double matrix.
        /// </summary>
        /// <param name="inputRange">The range must already be zero based.</param>
        /// <returns></returns>
        public static double[,] ConvertToDoubleMatrix(object[,] inputRange)
        {
            int rowCount = inputRange.RowCount();
            int colCount = inputRange.ColumnCount();
            var result = new double[rowCount, colCount];
            for (var row = 0; row < rowCount; row++)
            {
                for (var col = 0; col < colCount; col++)
                {
                    try
                    {
                        if (inputRange[row, col] != null)
                        {
                            result[row, col] = Convert.ToDouble(inputRange[row, col]);
                        }
                        else
                        {
                            result[row, col] = 0.0;
                        }
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Input range must contain numbers");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange"></param>
        /// <returns></returns>
        public static decimal[] ConvertToDecimalArray(object[] inputRange)
        {
            var rowCount = inputRange.Length;
            var result = new decimal[rowCount];
            for (var row = 0; row < rowCount; row++)
            {
                try
                {
                    result[row] = Convert.ToDecimal(inputRange[row]);
                }
                catch (Exception)
                {
                    throw new ArgumentException("Input range must contain numbers");
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange"></param>
        /// <param name="rowHeaderCount"></param>
        /// <returns></returns>
        public static decimal[,] ConvertToDecimalArray(object[,] inputRange, int rowHeaderCount)
        {
            var numRows = inputRange.GetLength(0);
            var numColumns = inputRange.GetLength(1) - rowHeaderCount;
            var result = new decimal[numRows, numColumns];
            for (var row = 0; row < numRows; row++)
            {
                for (var col = 0; col < numColumns; col++)
                {
                    try
                    {
                        result[row, col] = Convert.ToDecimal(inputRange[row, col + rowHeaderCount]);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Input range must contain numbers");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange"></param>
        /// <returns></returns>
        public static DateTime[] ConvertToDateTimeArray(object[] inputRange)
        {
            var result = new List<DateTime>();
            foreach (object item in inputRange)
            {
                if (typeof(DateTime) != item.GetType())
                {
                    throw new ArgumentException("Input range must contain DateTime");
                }
                result.Add((DateTime)item);
            }
            return result.ToArray();
        }

        ///<summary>
        /// Maps from a flattened 3D Cube into a collection of matrices: strike by expiry.
        ///</summary>
        ///<param name="expiries">The expiry array.</param>
        ///<param name="tenors">The underlying tenor array.</param>
        ///<param name="strikes">The strike array.</param>
        ///<param name="dataRange">The volatiltiy data range.</param>
        ///<param name="numTenors">The number of underlying tenors.</param>
        ///<returns></returns>
        public static List<Matrix> MapFromSurface(String[] expiries, String[] tenors, Decimal strikes, Matrix dataRange, int numTenors)
        {
            var result = new List<Matrix>();
            return result;
        }

        ///<summary>
        /// Sttrips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikes">The strike array.</param>
        ///<returns></returns>
        public static double[,] FilterSurface(object[,] inputRange, String tenorFilter, int numTenors, Double[] strikes)
        {
            var result = GetTenorSurfaceFromCube2(inputRange, tenorFilter, numTenors, strikes);
            return result;

        }

        ///<summary>
        /// Sttrips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikes">The strike array.</param>
        ///<returns></returns>
        public static object[,] FilterSurfaceWithExpiries(object[,] inputRange, String tenorFilter, int numTenors, Double[] strikes)
        {
            var result = GetTenorSurfaceFromCube3(inputRange, tenorFilter, numTenors, strikes);
            return result;

        }

        ///<summary>
        /// Gets the disctinct expisiries, which maust be in the first column.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<returns></returns>
        public static IEnumerable<string> ExtractExpiries(object[,] inputRange)
        {
            var numRows = inputRange.GetLength(0);
            var result = new List<string>();
            for (var i = 0; i < numRows; i++)
            {
                result.Add(Convert.ToString(inputRange[i,0]));
            }
            return result.ToArray();
        }

        ///<summary>
        /// Gets the disctinct expisiries, which maust be in the first column.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<returns></returns>
        public static IEnumerable<string> ExtractTenors(object[,] inputRange)
        {
            var numRows = inputRange.GetLength(0);
            var result = new List<string>();
            for (var i = 0; i < numRows; i++)
            {
                result.Add(Convert.ToString(inputRange[i, 1]));
            }
            return result.ToArray();
        }

        ///<summary>
        /// Sttrips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikes">The strike array.</param>
        ///<returns></returns>
        public static object[,] GetTenorSurfaceFromCube3(object[,] inputRange, String tenorFilter, int numTenors, Double[] strikes)
        {
            var numRows = inputRange.GetLength(0);
            var rows = numRows / numTenors;
            var numStrikes = strikes.Length;
            var result = new object[rows, numStrikes + 1];
            //int index = 0;
            var j = 0;
            for (var i = 0; i < numRows; i++)
            {
                if (Convert.ToString(inputRange[i, 1]) == tenorFilter)
                {
                    result[j, 0] = Convert.ToString(inputRange[i, 0]);
                    for (var col = 1; col <= numStrikes; col++)
                    {
                        result[j, col] = Convert.ToDouble(inputRange[i, col + 1]);
                    }
                    j++;
                }
                //index++;
            }
            return result;
        }

        ///<summary>
        /// Sttrips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikes">The strike array.</param>
        ///<returns></returns>
        public static double[,] GetTenorSurfaceFromCube2(object[,] inputRange, String tenorFilter, int numTenors, Double[] strikes)
        {
            var numRows = inputRange.GetLength(0);
            var rows = numRows / numTenors;
            var numStrikes = strikes.Length;
            var result = new double[rows, numStrikes];
            //var index = 0;
            var j = 0;
            for (var i = 0; i < numRows; i++)
            {
                if (Convert.ToString(inputRange[i, 1]) == tenorFilter)
                {
                    for (var col = 0; col < numStrikes; col++)
                    {
                        result[j, col] = Convert.ToDouble(inputRange[i, col + 2]);
                    }
                    j++;
                }
                //index++;
            }
            return result;
        }

        ///<summary>
        /// Sttrips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikes">The strike array.</param>
        ///<returns></returns>
        public static Matrix GetTenorSurfaceFromCube(object[,] inputRange, String tenorFilter, int numTenors, Decimal[] strikes)
        {
            var numRows = inputRange.GetLength(1);
            var rows = numRows/numTenors;
            var numStrikes = strikes.Length;
            var result = new Matrix(rows, numStrikes);
            //var index = 0;
            var j = 0;
            for (var i = 0; i < numRows; i++)
            {
                if (Convert.ToString(inputRange[i, 2]) == tenorFilter)
                {
                    for(var col = 0; col <= numStrikes; col++)
                    {
                        result[j, col] = Convert.ToDouble(inputRange[i, col + 2]);
                    }
                    j++;
                }
                //index++;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excelRange"></param>
        /// <returns></returns>
        public static List<string> StripRange(Excel.Range excelRange)
        {
            var unqVals = new List<string>();
            if (excelRange.Cells.Count == 1)
            {
                var value = excelRange.Value[System.Reflection.Missing.Value] as string;
                if (value != null)
                {
                    var elements = value.Split('-');
                    unqVals.AddRange(elements);
                }
            }
            else
            {
                var values = excelRange.Value[System.Reflection.Missing.Value] as object[,];
                if (values != null)
                    foreach (object obj in values)
                    {
                        if (obj != null)
                        {
                            unqVals.Add(obj.ToString());
                        }
                    }
            }
            return unqVals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excelRange"></param>
        /// <returns></returns>
        public static List<DateTime> StripDateTimeRange(Excel.Range excelRange)
        {
            var unqVals = new List<DateTime>();
            var values = excelRange.Value[System.Reflection.Missing.Value] as object[,];
            if (values != null)
            {
                foreach (object obj in values)
                {
                    var date = obj is DateTime;
                    if (date)
                    {
                        unqVals.Add((DateTime)obj);
                    }
                }
            }
            else
            {
                var date = (DateTime)excelRange.Value[System.Reflection.Missing.Value];
                unqVals.Add(date);
            }
            return unqVals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excelRange"></param>
        /// <returns></returns>
        public static List<int> StripIntRange(Excel.Range excelRange)
        {
            var unqVals = new List<int>();
            var values = excelRange.Value[System.Reflection.Missing.Value] as object[,];
            if (values != null)
            {
                foreach (object obj in values)
                {
                    try
                    {
                        var result = obj is int;
                        unqVals.Add(!result ? int.Parse(obj.ToString()) : Convert.ToInt32(obj));
                    }
                    catch
                    {
                        throw new Exception("The value of this range is not a valid integer.");
                    }
                }
            }
            else
            {
                var val = excelRange.Value[System.Reflection.Missing.Value];
                if (val != null)
                {
                    unqVals.Add(Convert.ToInt32(val));
                }
            }
            return unqVals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excelRange"></param>
        /// <returns></returns>
        public static List<Double> StripDoubleRange(Excel.Range excelRange)
        {
            var unqVals = new List<Double>();
            var values = excelRange.Value[System.Reflection.Missing.Value] as object[,];
            if (excelRange.Cells.Count > 1)
            {
                if (values != null)
                    foreach (object obj in values)
                    {
                        unqVals.Add(obj != null ? Convert.ToDouble(obj) : 0.0);
                    }
            }
            else
            {
                var val = excelRange.Value[System.Reflection.Missing.Value];
                if (val != null)
                {
                    unqVals.Add(Convert.ToDouble(val));
                }
            }
            return unqVals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excelRange"></param>
        /// <returns></returns>
        public static List<Decimal> StripDecimalRange(Excel.Range excelRange)
        {
            var unqVals = new List<Decimal>();
            if (excelRange.Cells.Count > 1)
            {
                var values = excelRange.Value[System.Reflection.Missing.Value] as object[,];
                if (values != null)
                    foreach (object obj in values)
                    {
                        unqVals.Add(obj != null ? Convert.ToDecimal(obj) : 0.0m);
                    }
            }
            else
            {
                var val = excelRange.Value[System.Reflection.Missing.Value];
                if (val != null)
                {
                    unqVals.Add(Convert.ToDecimal(val));
                }
            }
            return unqVals;
        }

        #endregion
    }
}