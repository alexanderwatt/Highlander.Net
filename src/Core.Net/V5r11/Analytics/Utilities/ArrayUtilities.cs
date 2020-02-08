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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Utilities
{
    //// here are extensions to IList to accomodate some QL functionality as well as have useful things for .net
    //public static class DynamicModuleLambdaCompiler
    //{
    //    public static Func<T> GenerateFactory<T>() where T : new()
    //    {
    //        NewExpression body = (NewExpression)((LambdaExpression)(() => Expression.New(typeof(T)))).Body;
    //        DynamicMethod dynamicMethod = new DynamicMethod("lambda", body.Type, new System.Type[0], typeof(DynamicModuleLambdaCompiler).Module, true);
    //        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
    //        ilGenerator.Emit(OpCodes.Newobj, body.Constructor);
    //        ilGenerator.Emit(OpCodes.Ret);
    //        Type delegateType = typeof(Func<T>);
    //        return (Func<T>)dynamicMethod.CreateDelegate(delegateType);
    //    }
    //}

    //public static class FastActivator<T> where T : new()
    //{
    //    public static readonly Func<T> Create = DynamicModuleLambdaCompiler.GenerateFactory<T>();
    //}

    //// this is a redefined collection class to emulate array-type behaviour at initialisation
    //// if T is a class then the list is initilized with default constructors instead of null
    //public class InitializedList<T> : List<T> where T : new()
    //{
    //    public InitializedList()
    //    { }

    //    public InitializedList(int size) : base(size)
    //    {
    //        for (int i = 0; i < Capacity; i++)
    //            Add(default(T) == null ? FastActivator<T>.Create() : default(T));
    //    }

    //    public InitializedList(int size, T value) : base(size)
    //    {
    //        for (int i = 0; i < Capacity; i++)
    //            Add(value);
    //    }

    //    // erases the contents without changing the size
    //    public void Erase()
    //    {
    //        for (int i = 0; i < Count; i++)
    //            this[i] = default(T);       // do we need to use "new T()" instead of default(T) when T is class?
    //    }
    //}

    /// <summary>
    /// This class manages array operations.
    /// </summary>
    public class ArraySupport
        {


            /// <summary> Searches for a key in a subset of a sorted array.</summary>
            /// <param name="index">Sorted array of integers
            /// </param>
            /// <param name="key">Key to search for
            /// </param>
            /// <param name="begin">Start posisiton in the index
            /// </param>
            /// <param name="end">One past the end position in the index
            /// </param>
            /// <returns> Integer index to key. -1 if not found
            /// </returns>
            public static int BinarySearch(int[] index, int key, int begin, int end)
        {
            end--;
            while (begin <= end)
            {
                int mid = (end + begin) >> 1;

                if (index[mid] < key)
                    begin = mid + 1;
                else if (index[mid] > key)
                    end = mid - 1;
                else
                    return mid;
            }
            return -1;
        }

        /// <summary> Searches for a key in a sorted array, and returns an index to an
        /// element which is greater than or equal key.
        /// </summary>
        /// <param name="index">Sorted array of integers</param>
        /// <param name="key">Search for something equal or greater</param>
        /// <param name="begin">Start posisiton in the index</param>
        /// <param name="end">One past the end position in the index</param>
        /// <returns> end if nothing greater or equal was found, else an
        /// index satisfying the search criteria</returns>
        public static int BinarySearchGreater(int[] index, int key, int begin, int end)
        {
            return BinarySearchInterval(index, key, begin, end, true);
        }

        private static int BinarySearchInterval(int[] index, int key, int begin, int end, bool greater)
        {
            // Zero length array?
            if (begin == end)
                if (greater)
                    return end;
                else
                    return begin - 1;

            end--; // Last index
            int mid = (end + begin) >> 1;

            // The usual binary search
            while (begin <= end)
            {
                mid = (end + begin) >> 1;

                if (index[mid] < key)
                    begin = mid + 1;
                else if (index[mid] > key)
                    end = mid - 1;
                else
                    return mid;
            }

            // No direct match, but an inf/sup was found
            if ((greater && index[mid] >= key) || (!greater && index[mid] <= key))
                return mid;
            // No inf/sup, return at the end of the array
            if (greater)
                return mid + 1;
            // One past end
            return mid - 1; // One before start
        }

        /// <summary>
        /// Compares the entire members of one array whith the other one.
        /// </summary>
        /// <param name="array1">The array to be compared.</param>
        /// <param name="array2">The array to be compared with.</param>
        /// <returns>True if both arrays are equals otherwise it returns false.</returns>
        /// <remarks>Two arrays are equal if they contains the same elements in the same order.</remarks>
        public static bool Equals(Array array1, Array array2)
        {
            bool result = false;
            if ((array1 == null) && (array2 == null))
                result = true;
            else if ((array1 != null) && (array2 != null))
            {
                if (array1.Length == array2.Length)
                {
                    int length = array1.Length;
                    result = true;
                    for (int index = 0; index < length; index++)
                    {
                        if (!(array1.GetValue(index).Equals(array2.GetValue(index))))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Fills the array with an specific value from an specific index to an specific index.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="fromindex">The first index to be filled.</param>
        /// <param name="toindex">The last index to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill(Array array, int fromindex, int toindex, object val)
        {
            for (int i = fromindex; i < toindex; i++)
            {
                array.SetValue(val, i);
            }
        }

        /// <summary>
        /// Fills the array with an specific value.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill(Array array, object val)
        {
            Fill(array, 0, array.Length, val);
        }
    }

    /// <summary>
    /// Class that provides a collection of useful utilities for common tasks
    /// used to process the built in array type.
    /// </summary>
    public static class ArrayUtilities
    {
        #region Method to Convert an Array of Doubles to an Array of Decimals

        /// <summary>
        /// Method to convert a one dimensional array of doubles to an array
        /// of decimals.
        /// </summary>
        /// <param name="sourceArray">One dimensional array that stores all
        /// the doubles that require conversion to decimal.</param>
        /// <returns>Array with each double converted to a decimal.</returns>
        public static decimal[] ArrayToDecimal(double[] sourceArray)
        {
            var decimalArray = new List<decimal>();

            // Convert each element in the source array to a decimal.
            foreach (var value in sourceArray)
            {
                decimalArray.Add((decimal)value);
            }

            // Construct and return the array of decimals.
            return decimalArray.ToArray();
        }

        /// <summary>
        /// Method to convert a one dimensional array of doubles to an array
        /// of decimals.
        /// </summary>
        /// <param name="sourceArray">One dimensional array that stores all
        /// the decimal that require conversion to double.</param>
        /// <returns>Array with each double converted to a decimal.</returns>
        public static double[] DecimalArrayToDouble(decimal[] sourceArray)
        {
            var doubleArray = new List<double>();

            // Convert each element in the source array to a decimal.
            foreach (var value in sourceArray)
            {
                doubleArray.Add(decimal.ToDouble(value));
            }

            // Construct and return the array of decimals.
            return doubleArray.ToArray();
        }

        /// <summary>
        /// Convert a 2D decimal array to a 1D array. If I could unbox the array it would be a generic
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] Convert2DTo1DDoubleArray(object[,] array)
        {
            var rb = array.GetUpperBound(0) + 1;
            var cb = array.GetUpperBound(1) + 1;

            var ub = (rb > cb) ? rb : cb;

            var retval = new double[ub];

            for (var row = 0; row < ub; row++)
                retval[row] = (rb > cb) ? Convert.ToDouble(array[row, 0], CultureInfo.CurrentCulture) :
                    Convert.ToDouble(array[0, row], CultureInfo.CurrentCulture);

            return retval;
        }

        /// <summary>
        /// Convert a 2D array to an array of arrays
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[,] Convert2DArray(object[,] array)
        {
            var ubRows = array.GetUpperBound(0) + 1;
            var ubCols = array.GetUpperBound(1) + 1;
            var retval = new double[ubRows, ubCols];

            for (var row = 0; row < ubRows; row++)
            {
                for (var col = 0; col < ubCols; col++)
                    retval[row, col] = Convert.ToDouble(array[row, col], CultureInfo.CurrentCulture);
            }
            return retval;
        }

        /// <summary>
        /// Convert a 2D decimal array to a 1D array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] DoubleMatrixToDoubleArray(double[,] array)
        {
            var rb = array.GetUpperBound(0) + 1;
            var cb = array.GetUpperBound(1) + 1;

            var ub = (rb > cb) ? rb : cb;

            var retval = new double[ub];

            for (var row = 0; row < ub; row++)
                retval[row] = (rb > cb) ? array[row, 0] :
                    array[0, row];

            return retval;
        }

        /// <summary>
        /// Convert a 2D decimal array to a 1D array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] DecimalMatrixToDoubleArray(decimal[,] array)
        {
            var rb = array.GetUpperBound(0) + 1;
            var cb = array.GetUpperBound(1) + 1;

            var ub = (rb > cb) ? rb : cb;

            var retval = new double[ub];

            for (var row = 0; row < ub; row++)
                retval[row] = (rb > cb) ? Convert.ToDouble(array[row, 0], CultureInfo.CurrentCulture) :
                    Convert.ToDouble(array[0, row], CultureInfo.CurrentCulture);

            return retval;
        }

        /// <summary>
        /// Gets the sum of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <returns>The sum of the products of each element at the same index.</returns>
        public static Decimal SumProduct(Decimal[] array1, Decimal[] array2)
        {
            var sumProduct = 0.0m;
            var index = 0;

            var length = array1.Length;
            if (array2.Length == length)
            {
                foreach (var element in array1)
                {
                    var temp = element * array2[index];
                    sumProduct += temp;
                    index++;
                }
            }
            return sumProduct;
        }

        /// <summary>
        /// Gets the sum of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <returns>The sum of the products of each element at the same index.</returns>
        public static double SumProduct(double[] array1, double[] array2)
        {
            var sumProduct = 0.0;
            var index = 0;

            var length = array1.Length;
            if (array2.Length == length)
            {
                foreach (var element in array1)
                {
                    var temp = element * array2[index];
                    sumProduct += temp;
                    index++;
                }
            }
            return sumProduct;
        }
        /// <summary>
        /// Gets the sum of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <returns>The sum of the products of each element at the same index.</returns>
        public static Decimal SumProduct(Decimal[] array1, Decimal[] array2, Decimal[] array3)
        {
            var sumProduct = 0.0m;
            var index = 0;

            var length = array1.Length;
            if (array2.Length == length && array3.Length == length)
            {
                foreach (var element in array1)
                {
                    var temp = element * array2[index] * array3[index];
                    sumProduct += temp;
                    index++;
                }
            }
            return sumProduct;
        }

        /// <summary>
        /// Gets the sum of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <returns>The sum of the products of each element at the same index.</returns>
        public static double SumProduct(double[] array1, double[] array2, double[] array3)
        {
            var sumProduct = 0.0;
            var index = 0;

            var length = array1.Length;
            if (array2.Length == length && array3.Length == length)
            {
                foreach (var element in array1)
                {
                    var temp = element * array2[index] * array3[index];
                    sumProduct += temp;
                    index++;
                }
            }
            return sumProduct;
        }
        /// <summary>
        /// Gets the sum of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <param name="array4">The forth  array of decimals.</param>
        /// <returns>The sum of the products of each element at the same index.</returns>
        public static Decimal SumProduct(Decimal[] array1, Decimal[] array2, Decimal[] array3, Decimal[] array4)
        {
            var sumProduct = 0.0m;
            var index = 0;

            var length = array1.Length;
            if (array2.Length == length && array3.Length == length && array4.Length == length)
            {
                foreach (var element in array1)
                {
                    var temp = element * array2[index] * array3[index] * array4[index];
                    sumProduct += temp;
                    index++;
                }
            }
            return sumProduct;
        }

        /// <summary>
        /// Gets the sum of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <param name="array4">The forth  array of decimals.</param>
        /// <returns>The sum of the products of each element at the same index.</returns>
        public static double SumProduct(double[] array1, double[] array2, double[] array3, double[] array4)
        {
            var sumProduct = 0.0;
            var index = 0;

            var length = array1.Length;
            if (array2.Length == length && array3.Length == length && array4.Length == length)
            {
                foreach (var element in array1)
                {
                    var temp = element * array2[index] * array3[index] * array4[index];
                    sumProduct += temp;
                    index++;
                }
            }
            return sumProduct;
        }
        /// <summary>
        /// Gets the array of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static Decimal[] Product(Decimal[] array1, Decimal[] array2)
        {
            var index = 0;
            var length = array1.Length;
            var product = new Decimal[length];
            if (array2.Length == length)
            {
                foreach (var element in array1)
                {
                    product[index] = element * array2[index];
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static double[] Product(double[] array1, double[] array2)
        {
            var index = 0;
            var length = array1.Length;
            var product = new double[length];
            if (array2.Length == length)
            {
                foreach (var element in array1)
                {
                    product[index] = element * array2[index];
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static Decimal[] Product(Decimal[] array1, Decimal[] array2, Decimal[] array3)
        {
            var index = 0;
            var length = array1.Length;
            var product = new Decimal[length];
            if (array2.Length == length && array3.Length == length)
            {
                foreach (var element in array1)
                {
                    product[index] = element * array2[index] * array3[index];
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static double[] Product(double[] array1, double[] array2, double[] array3)
        {
            var index = 0;
            var length = array1.Length;
            var product = new double[length];
            if (array2.Length == length && array3.Length == length)
            {
                foreach (var element in array1)
                {
                    product[index] = element * array2[index] * array3[index];
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <param name="array4">The forth  array of decimals.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static Decimal[] Product(Decimal[] array1, Decimal[] array2, Decimal[] array3, Decimal[] array4)
        {
            var index = 0;
            var length = array1.Length;
            var product = new Decimal[length];
            if (array2.Length == length && array3.Length == length && array4.Length == length)
            {
                foreach (var element in array1)
                {
                    product[index] = element * array2[index] * array3[index] * array4[index];
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of the products of the array elements at index i.
        /// </summary>
        /// <param name="array1">The first array of decimals.</param>
        /// <param name="array2">The second  array of decimals.</param>
        /// <param name="array3">The third  array of decimals.</param>
        /// <param name="array4">The forth  array of decimals.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static double[] Product(double[] array1, double[] array2, double[] array3, double[] array4)
        {
            var index = 0;
            var length = array1.Length;
            var product = new double[length];
            if (array2.Length == length && array3.Length == length && array4.Length == length)
            {
                foreach (var element in array1)
                {
                    product[index] = element * array2[index] * array3[index] * array4[index];
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of elements for delta calcs.
        /// </summary>
        /// <param name="curveYearFractions">The first array curveYearFractions.</param>
        /// <param name="periodAsTimesPerYears">The periodAsTimesPerYears.</param>
        /// <param name="discountFractors">The third  array of discountFractors.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static Decimal[] DeltaHelper(Decimal[] curveYearFractions, Decimal periodAsTimesPerYears,
            Decimal[] discountFractors)
        {
            var index = 0;
            var length = curveYearFractions.Length;
            var product = new Decimal[length];
            if (discountFractors.Length == length)
            {
                foreach (var element in curveYearFractions)
                {
                    var rate = (Decimal)Math.Log(-(double)discountFractors[index]) / curveYearFractions[index];
                    product[index] = -element / (1 + periodAsTimesPerYears * rate) / 10000;
                    index++;
                }
            }
            return product;
        }

        /// <summary>
        /// Gets the array of elements for delta calcs.
        /// </summary>
        /// <param name="curveYearFractions">The first array curveYearFractions.</param>
        /// <param name="periodAsTimesPerYears">The periodAsTimesPerYears.</param>
        /// <param name="discountFractors">The third  array of discountFractors.</param>
        /// <returns>The array of the products of each element at the same index.</returns>
        public static double[] DeltaHelper(double[] curveYearFractions, double periodAsTimesPerYears,
            double[] discountFractors)
        {
            var index = 0;
            var length = curveYearFractions.Length;
            var product = new double[length];
            if (discountFractors.Length == length)
            {
                foreach (var element in curveYearFractions)
                {
                    var rate = Math.Log(-discountFractors[index]) / curveYearFractions[index];
                    product[index] = -element / (1 + periodAsTimesPerYears * rate) / 10000;
                    index++;
                }
            }
            return product;
        }

        #endregion
    }
}