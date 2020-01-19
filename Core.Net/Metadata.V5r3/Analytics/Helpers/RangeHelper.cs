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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Highlander.Utilities.Helpers;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
    /// <summary>
    /// Class was made public to make it visible to Unit Test project.
    /// </summary>
    public static class RangeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listArray"></param>
        /// <returns></returns>
        public static object[,] ConvertArrayToRange(IList listArray)
        {
            object[,] resVals = new object[listArray.Count, 1];
            for (int idx = 0; idx < resVals.Length; ++idx)
                resVals[idx, 0] = listArray[idx];
            return resVals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange">All the arrays are the same length and zero based</param>
        /// <returns></returns>
        public static object[,] ListedRangeToMatrix(List<object[]> inputRange)
        {
            int rowCount = inputRange.Count;
            int colCount = inputRange[0].Length;
            var result = new object[rowCount, colCount];
            var rowIndex = 0;
            var colIndex = 0;
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    result[rowIndex, colIndex] = inputRange[i][j];
                    colIndex++;
                }
                colIndex = 0;
                rowIndex++;
            }
            return result;
        }

        /// <summary>
        /// Converts a range (possible one based) to a matrix.
        /// </summary>
        /// <param name="dblMatrix"></param>
        /// <returns>A matrix</returns>
        public static double[,] RangeToDoubleMatrix(object[,] dblMatrix)
        {
            var result = new double[dblMatrix.GetLength(0), dblMatrix.GetLength(1)];
            var rowIndex = 0;
            var colIndex = 0;
            for (var i = dblMatrix.GetLowerBound(0); i <= dblMatrix.GetUpperBound(0); i++)
            {
                for (var j = dblMatrix.GetLowerBound(1); j <= dblMatrix.GetUpperBound(1); j++)
                {
                    result[rowIndex, colIndex] = Convert.ToDouble(dblMatrix[i, j]);
                    colIndex++;
                }
                colIndex = 0;
                rowIndex++;
            }
            return result;
        }

        /// <summary>
        /// Converts a range (possible one based) to a matrix.
        /// </summary>
        /// <param name="dblMatrix"></param>
        /// <returns>A matrix</returns>
        public static decimal[,] RangeToDecimalMatrix(object[,] dblMatrix)
        {
            var result = new decimal[dblMatrix.GetLength(0), dblMatrix.GetLength(1)];
            var rowIndex = 0;
            var colIndex = 0;
            for (var i = dblMatrix.GetLowerBound(0); i <= dblMatrix.GetUpperBound(0); i++)
            {
                for (var j = dblMatrix.GetLowerBound(1); j <= dblMatrix.GetUpperBound(1); j++)
                {
                    result[rowIndex, colIndex] = Convert.ToDecimal(dblMatrix[i, j]);
                    colIndex++;
                }
                colIndex = 0;
                rowIndex++;
            }
            return result;
        }

        /// <summary>
        /// Convert2s the D array to class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rangeAsArray">The range as array.</param>
        /// <returns></returns>
        public static T Convert2DArrayToClass<T>(object[,] rangeAsArray) where T : new()
        {
            // The range is considered to be vertical (contains more rows than columns) by default
            //
            bool isVerticalRange = true;
            int matrixRowCount = rangeAsArray.GetUpperBound(0) + 1;
            if (rangeAsArray.GetUpperBound(1) > rangeAsArray.GetUpperBound(0))
            {
                matrixRowCount = rangeAsArray.GetUpperBound(1) + 1;
                isVerticalRange = false;
            }
            var result = new T();
            // Get all public fields
            //
            FieldInfo[] fields = typeof(T).GetFields();
            PropertyInfo[] properties = typeof(T).GetProperties();
            // Check fields first
            //
            if (fields.Length != 0)
            {
                for (int i = 0; i < matrixRowCount; ++i)
                {
                    object objectFromRange = isVerticalRange ? rangeAsArray[i, 1] : rangeAsArray[1, i];
                    // check for nulls
                    //
                    if (null != objectFromRange)
                    {
                        FieldInfo fieldInfo = fields[i];
                        object convertedObjectFromRange;
                        try
                        {
                            convertedObjectFromRange = ReflectionHelper.ChangeType(objectFromRange, fieldInfo.FieldType);
                        }
                        catch(Exception ex)
                        {
                            string message =
                                $"Error changing type of '{objectFromRange}' ({objectFromRange.GetType()}) to {fieldInfo.FieldType}. Field name: {fieldInfo.Name}";
                            var exception = new Exception(message, ex);
                            exception.Data.Add("ErrorChangingType", objectFromRange.ToString());
                            throw exception;
                        }
                        fieldInfo.SetValue(result, convertedObjectFromRange);
                    }
                }
            }
                // if there's no public fields - iterate thru public properties
                //
            else if (properties.Length != 0)
            {
                for (int i = 0; i < matrixRowCount; ++i)
                {
                    object objectFromRange = isVerticalRange ? rangeAsArray[i, 1] : rangeAsArray[1, i];
                    // check for nulls
                    //
                    if (null != objectFromRange)
                    {
                        PropertyInfo propertyInfo = properties[i];
                        object convertedObjectFromRange;
                        try 
                        {
                            convertedObjectFromRange = ReflectionHelper.ChangeType(objectFromRange, propertyInfo.PropertyType);
                        }
                        catch(Exception ex)
                        {
                            string message =
                                $"Error changing type of '{objectFromRange}' ({objectFromRange.GetType()}) to {propertyInfo.PropertyType}. Property name: {propertyInfo.Name}";                      
                            throw new Exception(message, ex);
                        }
                        propertyInfo.SetValue(result, convertedObjectFromRange, null);
                    }
                }
            }
            else
            {
                throw new Exception();
            }
            return result;
        }

        public static object[,] ToRange<T1, T2>(IList<Tuple<T1, T2>> logMessages)
        {
            if (logMessages == null)
            {
                return new object[,] { };
            }
            // A 2D array hardwired to match the Tuple in the list
            var output = new object[logMessages.Count, 2];
            for (int i = 0; i < logMessages.Count; i++)
            {
                output[i, 0] = logMessages[i].Item1;
                output[i, 1] = logMessages[i].Item2;
            }
            return output;
        }

        public static object[,] ToRange<T1, T2, T3>(IList<Tuple<T1, T2, T3>> logMessages)
        {
            if (logMessages == null)
            {
                return new object[,] { };
            }
            // A 2D array hardwired to match the Tuple in the list
            var output = new object[logMessages.Count, 3];
            for (int i = 0; i < logMessages.Count; i++)
            {
                output[i, 0] = logMessages[i].Item1;
                output[i, 1] = logMessages[i].Item2;
                output[i, 2] = logMessages[i].Item3;
            }
            return output;
        }

        public static object[,] ToRange<T>(IList<T> items)
        {
            if (items == null || items.Count == 0)
            {
                return new object[,] { };
            }
            object[,] output;
            if (items[0] is DictionaryEntry)
            {
                // If the internal array is of DictionaryEntry process
                var keyValuePairs = items.ToDictionary(k => ((DictionaryEntry)(object)k).Key,
                                                               v => ((DictionaryEntry)(object)v).Value);
                output = new object[items.Count, 2];
                int idx = 0;
                foreach (var keyValuePair in keyValuePairs)
                {
                    output[idx, 0] = keyValuePair.Key;
                    output[idx++, 1] = keyValuePair.Value;
                }
            }
            else
            {
                output = new object[items.Count, 1];
                for (int i = 0; i < items.Count(); i++)
                {
                    output[i, 0] = items[i];
                }
            }
            return output;
        }

        public static object[][] FromRange(object[,] dataSet)
        {
            // Get the original array dimensions (always a 2D array)
            int rankD0 = dataSet.GetLength(0);
            int rankD1 = dataSet.GetLength(1);

            rankD0 = MaxValidRowLength(dataSet, rankD0, rankD1);
            var values = new object[rankD0][];

            for (int rows = 0; rows < rankD0; rows++)
            {
                values[rows] = new object[rankD1];
                for (int columns = 0; columns < rankD1; columns++)
                {
                    values[rows][columns] = dataSet[rows, columns];
                }
            }
            return values;
        }

        public static object[][] FromDatedRange(object[,] dataSet)
        {
            // Get the original array dimensions (always a 2D array)
            int rankD0 = dataSet.GetLength(0);
            int rankD1 = dataSet.GetLength(1);

            rankD0 = MaxValidRowLength(dataSet, rankD0, rankD1);
            var values = new object[rankD0][];

            for (int rows = 0; rows < rankD0; rows++)
            {
                values[rows] = new object[rankD1];
                for (int columns = 0; columns < rankD1; columns++)
                {
                    values[rows][columns] = (columns == 0)
                                                ? DateTime.FromOADate((double)dataSet[rows, columns])
                                                : dataSet[rows, columns];
                }
            }
            return values;
        }

        /// <summary>
        /// Remove any extra rows in a named range that are empty (ExcelDNA.Integration.ExcelEmpty)
        /// The WebService soap doesn't know what this type is
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="rankD0"></param>
        /// <param name="rankD1"></param>
        /// <returns></returns>
        private static int MaxValidRowLength(object[,] dataSet, int rankD0, int rankD1)
        {
            int maxlength = rankD0;
            int column = rankD1 - 1;

            if (column > -1)
            {
                for (int rows = 0; rows < rankD0; rows++)
                {
                    if (dataSet[rows, column] is Missing)//TODO check this!
                    {
                        maxlength = rows;
                        break;
                    }
                }
            }
            return maxlength;
        }
    }
}