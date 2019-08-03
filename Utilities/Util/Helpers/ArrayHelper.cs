/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;

namespace Orion.Util.Helpers
{
    /// <summary>
    /// Array helper class
    /// </summary>
    public static class ArrayHelper
    {
        /// <summary>
        /// Converts the dictionary to2 D array.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static object[,] ConvertDictionaryTo2DArray<T1, T2>(IDictionary<T1, T2> dictionary)
        {
          const int colIndex = 1;
          var twoDArray = new object[dictionary.Count, colIndex + 1];
          int rowIndex = 0;

          foreach (KeyValuePair<T1, T2> de in dictionary)
          {
              twoDArray[rowIndex, 0] = de.Key;
              twoDArray[rowIndex, colIndex] = de.Value;
              rowIndex++;
          }
          return twoDArray;
        }

        /// <summary>
        /// Converts an array to a horizontal matrix.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>A horizontal vector.</returns>
        public static T[,] ArrayToHorizontalMatrix<T>(T[] array)
        {
            var result = new T[1, array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                result[0, i] = array[i];
            }
            return result;
        }

        /// <summary>
        /// Converts an array to a double vertical matrix for ExcelAPI display.
        /// </summary>
        /// <param name="array"></param>
        /// <returns>A vertical vector.</returns>
        public static T[,] ArrayToVerticalMatrix<T>(T[] array)
        {
            var result = new T[array.Length, 1];
            for (var i = 0; i < array.Length; i++)
            {
                result[i, 0] = array[i];
            }
            return result;
        }

        /// <summary>
        /// Converts a double matrix of either column or row dimension 1, to a double array.
        /// </summary>
        /// <param name="dblMatrix"></param>
        /// <returns>A double array</returns>
        public static T[] MatrixToArray<T>(T[,] dblMatrix)
        {
            var result = new T[] { };
            if (dblMatrix.GetLength(0) > dblMatrix.GetLength(1))
            {
                result = new T[dblMatrix.GetLength(0)];
                for (var i = dblMatrix.GetLowerBound(0); i <= dblMatrix.GetUpperBound(0); i++)
                {
                    result[i - dblMatrix.GetLowerBound(0)] = dblMatrix[i, dblMatrix.GetLowerBound(1)];
                }
            }
            else if (dblMatrix.GetLength(1) > 0)
            {
                result = new T[dblMatrix.GetLength(1)];
                for (var i = dblMatrix.GetLowerBound(1); i <= dblMatrix.GetUpperBound(1); i++)
                {
                    result[i - dblMatrix.GetLowerBound(1)] = dblMatrix[dblMatrix.GetLowerBound(0), i];
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a range (possible one based) to a matrix.
        /// </summary>
        /// <param name="dblMatrix"></param>
        /// <returns>A matrix</returns>
        public static T[,] RangeToMatrix<T>(T[,] dblMatrix)
        {
            var result = new T[dblMatrix.GetLength(0), dblMatrix.GetLength(1)];
                for (var i = dblMatrix.GetLowerBound(0); i <= dblMatrix.GetUpperBound(0); i++)
                {
                    for (var j = dblMatrix.GetLowerBound(1); j <= dblMatrix.GetUpperBound(1); j++)
                    {
                         result[i - dblMatrix.GetLowerBound(1), j - dblMatrix.GetLowerBound(0)] = dblMatrix[i, j];
                    }
            }
            return result;
        }
    }
}
