using System;
using System.Collections.Generic;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

namespace Orion.V5r3.PublisherWebService
{
    /// <summary>
    /// Extension methods for object[,]
    /// </summary>
    internal static class RangeExtension
    {
        /// <summary>
        /// Convert an Excel range (object[,]) to a Dictionary
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object[,] properties)
        {
            IDictionary<string, object> results = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            if (properties != null)
            {
                int rowStart = properties.GetLowerBound(0);
                int colStart = properties.GetLowerBound(1);
                int rowCount = properties.GetUpperBound(0);
                for (int i = rowStart; i <= rowCount; i++)
                {
                    if (properties[i, colStart] != null)
                    {
                        results[(string)properties[i, colStart]] = properties[i, colStart + 1];
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Convert an Excel range (object[,]) to a List of Pairs of defined type
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static List<Pair<T1, T2>> ToList<T1, T2>(this object[,] range)
        {
            var results = new List<Pair<T1, T2>>();
            if (range != null)
            {
                int rowStart = range.GetLowerBound(0);
                int rowCount = range.GetUpperBound(0);
                int colStart = range.GetLowerBound(1);
                int colCount = range.GetUpperBound(1) - colStart + 1;
                if (colCount != 2)
                {
                    throw new ArgumentException("Expecting a range with two columns");
                }
                for (int i = rowStart; i <= rowCount; i++)
                {
                    var value1 = (T1)Convert.ChangeType(range[i, colStart], typeof(T1));
                    var value2 = (T2)Convert.ChangeType(range[i, colStart + 1], typeof(T2));
                    results.Add(new Pair<T1, T2>(value1, value2));
                }
            }
            return results;
        }

        /// <summary>
        /// Convert an Excel range (object[,]) to a List of Pairs of string, object
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static List<Pair<string, object>> ToList(this object[,] range)
        {
            return ToList<string, object>(range);
        }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="rowNumber">The row number.</param>
        /// <returns></returns>
        public static object[] GetRow(this object[,] rawData, int rowNumber)
        {
            int cols = rawData.GetUpperBound(1) + 1;
            int rows = rawData.GetUpperBound(0) + 1;
            var items = new List<object>();

            if (rowNumber <= rows)
            {
                for (int index = 0; index < cols; index++)
                {
                    items.Add(rawData[rowNumber, index]);
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="rowNumber">The row number.</param>
        /// <param name="startColumn">The start column.</param>
        /// <param name="endColumn">The end column.</param>
        /// <returns></returns>
        public static object[] GetRow(this object[,] rawData, int rowNumber, int startColumn, int endColumn)
        {
            int cols = rawData.GetUpperBound(1) + 1;
            int rows = rawData.GetUpperBound(0) + 1;
            var items = new List<object>();

            if ((rowNumber <= rows) && (startColumn < endColumn) && (startColumn <= cols && endColumn <= cols))
            {
                for (int index = startColumn; index < endColumn; index++)
                {
                    items.Add(rawData[rowNumber, index]);
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="columnNumber">The column number.</param>
        /// <returns></returns>
        public static object[] GetColumn(this object[,] rawData, int columnNumber)
        {
            int cols = rawData.GetUpperBound(1) + 1;
            int rows = rawData.GetUpperBound(0) + 1;
            var items = new List<object>();

            if (columnNumber <= cols)
            {
                for (int index = 0; index < rows; index++)
                {
                    items.Add(rawData[index, columnNumber]);
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Sets the column.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="columnNumber">The column number.</param>
        /// <param name="values">The values.</param>
        public static void SetColumn(this object[,] rawData, int columnNumber, object[] values)
        {
            int cols = rawData.GetUpperBound(1) + 1;
            int rows = rawData.GetUpperBound(0) + 1;
            if (columnNumber <= cols)
            {
                for (int index = 0; index < rows; index++)
                {
                    if (index < values.Length)
                        rawData[index, columnNumber] = values[index];
                }
            }
        }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        public static object[][] GetRows(this object[,] rawData, int startRow, int endRow)
        {
            return GetRows(rawData, startRow, endRow, 0, rawData.ColumnCount());
        }

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <param name="startColumn">The start column.</param>
        /// <param name="endColumn">The end column.</param>
        /// <returns></returns>
        public static object[][] GetRows(this object[,] rawData, int startRow, int endRow, int startColumn, int endColumn)
        {
            int rowIndex = 0;
            var results = new object[endRow - startRow][];
            for (int index = startRow; index < endRow; index++)
            {
                results[rowIndex] = GetRow(rawData, index, startColumn, endColumn);
                rowIndex++;
            }
            return results;
        }

        /// <summary>
        /// Rows the count.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <returns></returns>
        public static int RowCount(this object[,] rawData)
        {
            return rawData.GetUpperBound(0) + 1;
        }

        /// <summary>
        /// Columns the count.
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <returns></returns>
        public static int ColumnCount(this object[,] rawData)
        {
            return rawData.GetUpperBound(1) + 1;
        }

        /// <summary>
        /// Values as array.
        /// </summary>
        /// <param name="vals">The vals.</param>
        /// <returns></returns>
        public static object[][] ValueAsArray(this object[,] vals)
        {
            int lb = vals.GetLowerBound(0);
            int ub = vals.GetUpperBound(0);

            int lb1 = vals.GetLowerBound(1);
            int ub1 = vals.GetUpperBound(1);
            var values = new object[ub - lb + 1][];
            for (int i = lb; i <= ub; i++)
            {
                values[i - lb] = new object[ub1 - lb1 + 1];
                for (int j = lb1; j <= ub1; j++)
                    values[i - lb][j - lb1] = vals[i, j];
            }
            return values;
        }

        /// <summary>
        /// Trims nulls for a 2D array
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static Array TrimNulls(this Array array)
        {
            //  find the last non null row
            int index1LowerBound = array.GetLowerBound(1);

            int nonNullRows = 0;

            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                object o = array.GetValue(i, index1LowerBound);

                if ((o == null) || (o.ToString() == String.Empty))
                    break;
                ++nonNullRows;
            }

            //  get the element type
            Type elementType = array.GetType().GetElementType();

            if (elementType != null)
            {
                var result = Array.CreateInstance(elementType, new[] { nonNullRows, array.GetLength(1) },
                    new[] { array.GetLowerBound(0), array.GetLowerBound(1) });

                for (int i = array.GetLowerBound(0); i < array.GetLowerBound(0) + nonNullRows; ++i)
                {
                    for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                    {
                        object value = array.GetValue(i, j);
                        result.SetValue(value, i, j);
                    }
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Trims nulls for a 1D array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static T[] TrimNulls<T>(this T[] array)
        {
            if (typeof(T).IsValueType)
            {
                return array;
            }

            int lower = array.GetLowerBound(0);
            int upper = array.GetUpperBound(0);
            for (int i = upper; i >= lower; i--)
            {
                if (array[i] != null)
                {
                    break;
                }
                upper--;
            }
            var newArray = new T[upper - lower + 1];
            for (int i = lower; i <= upper; i++)
            {
                newArray[i - lower] = array[i];
            }
            return newArray;
        }

        /// <summary>
        /// Results the over fills selection.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="rowCount">The row count.</param>
        /// <param name="columnCount">The column count.</param>
        /// <returns></returns>
        public static Boolean ResultOverFillsSelection(this object[,] result, int rowCount, int columnCount)
        {
            bool bOverFills = (rowCount < result.GetUpperBound(0) + 1) || (columnCount < result.GetUpperBound(1) + 1);
            return bOverFills;
        }

        /// <summary>
        /// Adds the more text.
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <param name="result">The result.</param>
        public static void AddMoreText(this object[,] result, int rowCount, int columnCount)
        {
            int maxRow = Math.Min(rowCount, result.GetUpperBound(0) + 1);
            int maxCol = Math.Min(columnCount, result.GetUpperBound(1) + 1);
            result[maxRow - 1, maxCol - 1] = $"more({result.GetUpperBound(0) + 1},{result.GetUpperBound(1) + 1})...";
        }

        /// <summary>
        /// Convert object[,] to a NamedValueSet
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static NamedValueSet ToNamedValueSet(this object[,] range)
        {
            //ToDo move this conversion code into the NamedValueSet constructor
            NamedValueSet result;
            if (range.GetLowerBound(0) == 1)
            {
                // One based array, need to change to zero based
                int upperBound = range.GetUpperBound(0);
                var range0 = new object[upperBound, 2];
                Array.Copy(range, range0, upperBound * 2);
                result = new NamedValueSet(range0);
            }
            else
            {
                result = new NamedValueSet(range);
            }
            return result;
        }

        /// <summary>
        /// Convert an Excel 1D range (object[]) to an array of defined type
        /// </summary>
        /// <param name="range"></param>
        /// <param name="rowHeaderCount">The number of header rows</param>
        /// <returns></returns>
        public static T[] ToArray<T>(this object[] range, int rowHeaderCount)
        {
            T[] results = null;
            if (range != null)
            {
                int rowMin = range.GetLowerBound(0) + rowHeaderCount;
                int rowMax = range.GetUpperBound(0);
                // Exclude null rows
                for (int row = rowMax; row >= rowMin; row--)
                {
                    if (range[row] != null)
                    {
                        break;
                    }
                    rowMax--;
                }
                if (rowMin > rowMax)
                {
                    throw new ArgumentException("rowHeaderCount cannot be larger than the number of rows");
                }
                results = new T[rowMax - rowMin + 1];
                for (int row = rowMin; row <= rowMax; row++)
                {
                    results[row - rowMin] = (T)Convert.ChangeType(range[row], typeof(T));
                }
            }
            return results;
        }

        /// <summary>
        /// Convert an Excel 1D range (object[]) to an array of defined type
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this object[] properties)
        {
            return ToArray<T>(properties, 0);
        }

        /// <summary>
        /// Convert an Excel 2D range (object[,]) to a 2D array of defined type
        /// </summary>
        /// <param name="range"></param>
        /// <param name="rowHeaderCount">The number of header rows</param>
        /// <param name="columnHeaderCount">The number of header columns</param>
        /// <returns></returns>
        public static T[,] ToArray<T>(this object[,] range, int rowHeaderCount, int columnHeaderCount)
        {
            T[,] results = null;
            if (range != null)
            {
                int rowMin = range.GetLowerBound(0) + rowHeaderCount;
                int rowMax = range.GetUpperBound(0);
                int colMin = range.GetLowerBound(1) + columnHeaderCount;
                int colMax = range.GetUpperBound(1);
                // Exclude null rows
                for (int row = rowMax; row >= rowMin; row--)
                {
                    if (range[row, 0] != null)
                    {
                        break;
                    }
                    rowMax--;
                }
                if (rowMin > rowMax)
                {
                    throw new ArgumentException("rowHeaderCount cannot be larger than the number of rows");
                }
                if (colMin > colMax)
                {
                    throw new ArgumentException("colHeaderCount cannot be larger than the number of columns");
                }
                results = new T[rowMax - rowMin + 1, colMax - colMin + 1];
                for (int row = rowMin; row <= rowMax; row++)
                {
                    for (int col = colMin; col <= colMax; col++)
                    {
                        results[row - rowMin, col - colMin] = (T)Convert.ChangeType(range[row, col], typeof(T));
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Convert an Excel 2D range (object[,]) to a 2D array of defined type
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static T[,] ToArray<T>(this object[,] range)
        {
            return range.ToArray<T>(0, 0);
        }
    }
}
