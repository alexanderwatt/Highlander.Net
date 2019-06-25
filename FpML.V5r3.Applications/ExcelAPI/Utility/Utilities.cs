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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using HLV5r3.Properties;
using Microsoft.Win32;
using Orion.Analytics.Helpers;
using Orion.Util.Helpers;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Utility
{
    /// <summary>
    /// This is a lightweight wrapper that interfaces between the SABRInterface class and the ExcelAPI.
    /// The wrapper acts as a layer that will map Excel specific data/operations to Interface expected types
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("8137E2AD-EA04-48F1-AE0A-FFB4819581C1")]
    public class Utilities
    {
        private const int RowDimension = 0;
        private const int ColumnDimension = 1;

        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key?.SetValue("", Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion

        #region Helper Methods

        ///<summary>
        /// Strips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikeArray">The strike array.</param>
        ///<returns></returns>
        public static double[,] FilterSurface(Excel.Range inputRange, String tenorFilter, int numTenors, Excel.Range strikeArray)
        {
            var values = inputRange.Value[System.Reflection.Missing.Value] as object[,];
            var strike = DataRangeHelper.StripDoubleRange(strikeArray);
            return DataRangeHelper.FilterSurface(values, tenorFilter, numTenors, strike.ToArray());
        }

        ///<summary>
        /// Strips a surface: expiry by strike, from a flattened cube, excluding the strike headers.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<param name="tenorFilter">The tenor string to filter on.</param>
        ///<param name="numTenors">The number of tenors.</param>
        ///<param name="strikeArray">The strike array.</param>
        ///<returns></returns>
        public static object[,] FilterSurfaceWithExpiries(Excel.Range inputRange, String tenorFilter, int numTenors, Double[] strikeArray)
        {
            var values = inputRange.Value[System.Reflection.Missing.Value] as object[,];
            return DataRangeHelper.FilterSurfaceWithExpiries(values, tenorFilter, numTenors, strikeArray); 
        }

        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public string GetCurveEngineVersionInfo()
        {
            return Orion.CurveEngine.Information.GetVersionInfo();
        }

        /// <summary>
        /// Gets the Resource Version
        /// </summary>
        /// <returns></returns>
        public string GetResourcesVersion()
        {
            //return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return Resources.QVersion;
        }

        private static void VectorAppend(ICollection<object> vector, [Optional] object arg)
        {
            if (!(arg is System.Reflection.Missing))
            {
                vector.Add(arg);
            }
        }

        #endregion

        #region Array Methods

        public static object[,] ArrayAppend(Excel.Range arrayA, Excel.Range arrayB, string acrossOrDown, object pad = null)
        {
            var values1 = arrayA.Value[System.Reflection.Missing.Value] as object[,];
            var values2 = arrayB.Value[System.Reflection.Missing.Value] as object[,];
            bool across = (string.IsNullOrEmpty(acrossOrDown) ||
                           !acrossOrDown.Equals("down", StringComparison.CurrentCultureIgnoreCase));
            return AppendArray(values1, values2, across, pad);
        }

        public static object[,] ArrayPad(Excel.Range inputArray, object pad = null, int rows = 0, int cols = 0)
        {
            return PadArray(inputArray, pad, rows, cols);
        }

        public object[] Vector(
            object arg1,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null)
        {
            IList<object> vector = new List<object> { arg1 };
            VectorAppend(vector, arg2);
            VectorAppend(vector, arg3);
            VectorAppend(vector, arg4);
            VectorAppend(vector, arg5);
            VectorAppend(vector, arg6);
            VectorAppend(vector, arg7);
            VectorAppend(vector, arg8);
            VectorAppend(vector, arg9);
            VectorAppend(vector, arg10);
            VectorAppend(vector, arg11);
            VectorAppend(vector, arg12);
            return vector.ToArray();
        }

        /// <summary>
        /// Pad an array with the requested padValue or default to an empty string
        /// </summary>
        /// <param name="inputArray">The original (un-padded) array</param>
        /// <param name="padValue">An optional scalar pad value. The default is an empty string</param>
        /// <param name="numRows">The number of rows in the padded array. The default is to use the input row size</param>
        /// <param name="numColumns">The number of columns in the padded array. The default is to use the input column size</param>
        /// <returns>The input array (or an enlarged version) with padding of any nulls</returns>
        public static object[,] PadArray(Excel.Range inputArray, [Optional]  object padValue, [Optional] object numRows, [Optional] object numColumns)
        {
            var values1 = inputArray.Value[System.Reflection.Missing.Value] as object[,];
            int rows = values1.GetLength(RowDimension);
            int cols = values1.GetLength(ColumnDimension);
            // Set defaults - rows/cols must be at least the same size, pad cannot be null
            var rowSize = 0;// (rows > rowSize) ? rows : rowSize;
            var columnSize = 0;// (cols > columnSize) ? cols : columnSize;
            padValue = string.Empty;
            if (!(padValue is System.Reflection.Missing))
            {
                var r1 = (Excel.Range) null;
                padValue = r1.Value2;
                if (!(numRows is System.Reflection.Missing))
                {
                    if (numRows is Excel.Range r2) rowSize = Convert.ToInt32(r2.Value2);
                }
                if (!(numColumns is System.Reflection.Missing))
                {
                    if (numColumns is Excel.Range r3) columnSize = Convert.ToInt32(r3.Value2);
                }
            }
            rowSize = (rows > rowSize) ? rows : rowSize;
            columnSize = (cols > columnSize) ? cols : columnSize;
            // Only process the array if the rows and/or columns have changed in size
            if (rowSize == rows && columnSize == cols)
            {
                return values1;
            }
            // Create new array
            var newArray = new object[rowSize, columnSize];
            // Fill the new array with the original array plus padding
            for (int rowIndex = 0; rowIndex < rowSize; rowIndex++)
            {
                for (int colIndex = 0; colIndex < columnSize; colIndex++)
                {
                    if (rowIndex >= rows ||
                        colIndex >= cols)
                    {
                        newArray[rowIndex, colIndex] = padValue;
                    }
                    else
                    {
                        newArray[rowIndex, colIndex] = values1[rowIndex, colIndex];
                    }
                }
            }
            return newArray;
        }

        /// <summary>
        /// Join Array B to Array A and pad either array to match dimension
        /// Either Row dimension if across (columns) or Column dimension if join is down (rows)
        /// </summary>
        /// <param name="arrayA">The first array to join</param>
        /// <param name="arrayB">The second array to join</param>
        /// <param name="appendAcross">If true append array B to array A across columns else join across rows</param>
        /// <param name="padValue">A value to use to fill short arrays (in either dimension)</param>
        /// <returns>A new array joining both source arrays (B appended to A)</returns>
        public static object[,] AppendArray(object[,] arrayA, object[,] arrayB, bool appendAcross, [Optional] object padValue)
        {
            // Check that we have 2 correct arrays
            if (arrayA == null && arrayB == null)
            {
                return new object[,] { };
            }
            if (arrayB == null)
            {
                return arrayA;
            }
            if (arrayA == null)
            {
                return arrayB;
            }
            padValue = string.Empty;
            if (!(padValue is System.Reflection.Missing))
            {
                var r1 = (Excel.Range) null;
                padValue = r1.Value2;
            }
            int maxRows;
            int maxCols;
            int rowLengthA = arrayA.GetLength(RowDimension);
            int rowLengthB = arrayB.GetLength(RowDimension);
            int colLengthA = arrayA.GetLength(ColumnDimension);
            int colLengthB = arrayB.GetLength(ColumnDimension);
            // Determine the array dimensions - depending on whether appending arrays across (col) or down (row)
            if (appendAcross)
            {
                maxRows = Math.Max(rowLengthA, rowLengthB);
                maxCols = colLengthA + colLengthB;
            }
            else
            {
                maxCols = Math.Max(colLengthA, colLengthB);
                maxRows = rowLengthA + rowLengthB;
            }
            // Create the new array
            var joinedArray = new object[maxRows, maxCols];
            if (appendAcross)
            {
                for (int colIdx = 0; colIdx < maxCols; colIdx++)
                {
                    for (int rowIdx = 0; rowIdx < maxRows; rowIdx++)
                    {
                        if (colIdx < colLengthA)
                        {
                            if (rowIdx < rowLengthA)
                            {
                                joinedArray[rowIdx, colIdx] = arrayA[rowIdx, colIdx];
                            }
                            else
                            {
                                joinedArray[rowIdx, colIdx] = padValue;
                            }
                        }
                        else
                        {
                            if (rowIdx < rowLengthB)
                            {
                                joinedArray[rowIdx, colIdx] = arrayB[rowIdx, colIdx - colLengthA];
                            }
                            else
                            {
                                joinedArray[rowIdx, colIdx] = padValue;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int rowIdx = 0; rowIdx < maxRows; rowIdx++)
                {
                    for (int colIdx = 0; colIdx < maxCols; colIdx++)
                    {
                        if (rowIdx < rowLengthA)
                        {
                            if (colIdx < colLengthA)
                            {
                                joinedArray[rowIdx, colIdx] = arrayA[rowIdx, colIdx];
                            }
                            else
                            {
                                joinedArray[rowIdx, colIdx] = padValue;
                            }
                        }
                        else
                        {
                            if (colIdx < colLengthB)
                            {
                                joinedArray[rowIdx, colIdx] = arrayB[rowIdx - rowLengthA, colIdx];
                            }
                            else
                            {
                                joinedArray[rowIdx, colIdx] = padValue;
                            }
                        }
                    }
                }
            }
            return joinedArray;
        }

        #endregion

        #region Range Examples

        /// <summary>
        /// Stacks a couple of equal length vertical ranges.
        /// </summary>
        /// <param name="firstVertical1DRange"></param>
        /// <param name="secondVertical1DRange"></param>
        /// <returns></returns>
        public static object StackVerticalArrays(Excel.Range firstVertical1DRange, Excel.Range secondVertical1DRange)
        {
            object[,] firstColumn = ArrayHelper.RangeToMatrix(firstVertical1DRange.Value[System.Reflection.Missing.Value] as object[,]);
            object[,] secondColumn = ArrayHelper.RangeToMatrix(secondVertical1DRange.Value[System.Reflection.Missing.Value] as object[,]);
            var first = firstColumn.RowCount();
            var second = secondColumn.RowCount();
            if (first == second)
            {
                var result = new object[first, 2];
                var column1 = firstColumn.GetColumn(0);
                result.SetColumn(0, column1);
                var column2 = secondColumn.GetColumn(0);
                result.SetColumn(1, column2);
                return result;
            }
            else
            {
                var result = new object[1, 1];
                result[0, 0] = "Arrays are not of equal length or not vertical!";
                return result;
            }
        }

        /// <summary>
        /// An example of optional parameters.
        /// </summary>
        /// <param name="number1">The first number as a [double];</param>
        /// <param name="number2">The [optional] second number as a [double];</param>
        /// <param name="number3">The [optional] third number as a [double];</param>
        /// <returns>The addition of all provided numbers.</returns>
        public static double AddNumbers(double number1, [Optional] object number2, [Optional] object number3)
        {
            double result = 0;
            result += Convert.ToDouble(number1);
            if (!(number2 is System.Reflection.Missing))
            {
                if (number2 is Excel.Range r2)
                {
                    double d2 = Convert.ToDouble(r2.Value2);
                    result += d2;
                }
            }
            if (!(number3 is System.Reflection.Missing))
            {
                if (number3 is Excel.Range r3)
                {
                    double d3 = Convert.ToDouble(r3.Value2);
                    result += d3;
                }
            }
            return result;
        }

        /// <summary>
        /// Calculates the area of the range.
        /// </summary>
        /// <param name="range">The selected range passed to the function.</param>
        /// <returns>The area.</returns>
        public static double CalculateArea(object range)
        {
            return range is Excel.Range r ? Convert.ToDouble(r.Width)*Convert.ToDouble(r.Height) : 0;
        }

        /// <summary>
        /// Returns the number of cells in the selected range.
        /// </summary>
        /// <param name="range">The selected range.</param>
        /// <returns>The number of cells.</returns>
        public static double NumberOfCells(object range)
        {
            var r = range as Excel.Range;
            return r?.Cells.Count ?? 0;
        }

        /// <summary>
        /// Converts a string to upper case.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The upper case of the string.</returns>
        public static string ToUpperCase(string input)
        {
            return input.ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetRange"></param>
        /// <returns></returns>
        public static object[,] UNIQUEVALUES(Excel.Range targetRange)
        {
            var values = targetRange.Value[System.Reflection.Missing.Value] as object[,];
            var unqVals = new List<object>();
            if (values != null)
                foreach (var obj in values)
                {
                    if (!unqVals.Contains(obj))
                        unqVals.Add(obj);
                }
            var resVals = new object[unqVals.Count, 1];
            for (int idx = 0; idx < resVals.Length; ++idx)
                resVals[idx, 0] = unqVals[idx];
            return resVals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static object[,] ConvertRangeTo2DArray(Excel.Range range)
        {
            var valueArray = range.Value[Type.Missing] as object[,];
            var formulaArray = (object[,])range.Formula;
            if (
                valueArray != null && (valueArray.GetUpperBound(0) != formulaArray.GetUpperBound(0) ||
                                       valueArray.GetUpperBound(1) != formulaArray.GetUpperBound(1) ||
                                       valueArray.GetLowerBound(0) != formulaArray.GetLowerBound(0) ||
                                       valueArray.GetLowerBound(1) != formulaArray.GetLowerBound(1))
                )
            {
                throw new Exception("RangeHelper.ConvertRangeTo2DArray method failed. Dimensions of the value and formula arrays must be the same.");
            }
            if (valueArray != null)
            {
                for (int i = valueArray.GetLowerBound(0); i <= valueArray.GetUpperBound(0); ++i)
                {
                    for (int j = valueArray.GetLowerBound(1); j <= valueArray.GetUpperBound(1); ++j)
                    {
                        var formulaCellValue = (string) formulaArray[i, j];
                        // if a correspondent formula value has indicated that there's no value in it - put the NULL into value array.
                        //
                        if (IsNAString(formulaCellValue))
                        {
                            valueArray[i, j] = null;
                        }
                    }
                }
                return valueArray;
            }
            return new object[,] { };
        }

        /// list of possible N/A strings in Excel
        /// 
        /// #NULL! 0x800a07d0
        /// #DIV/0! 0x800a07d7
        /// #VALUE! 0x800a07df
        /// #REF! 0x800a07e7
        /// #NAME? 0x800a07ed
        /// #NUM! 0x800a07f4
        /// #N/A 0x800a07fa
        private static readonly List<string> NAValues = new List<string>(new[] { "#NULL!", "#DIV/0!", "#VALUE!", "#REF!", "#NAME?", "#NUM!", "#N/A" });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNAString(string value)
        {
            return -1 != NAValues.IndexOf(value);
        }

        #endregion
    }
}