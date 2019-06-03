using System;
using System.Collections.Generic;
using Orion.Util.NamedValues;

namespace Orion.ValuationEngine
{
    internal static class ArrayExtension
    {
        public static object[,] ConvertArrayToMatrix(this object[][] inputArray)
        {
            int c1 = inputArray.Length;
            int c2 = inputArray[inputArray.GetLowerBound(0)].Length;
            var result = new object[c1, c2];
            int lb1 = inputArray.GetLowerBound(0);
            int lb2 = inputArray[lb1].GetLowerBound(0);
            int ub1 = inputArray.GetUpperBound(0);
            int ub2 = inputArray[lb1].GetUpperBound(0);
            for (int i = lb1; i <= ub1; i++)
            {
                for (int j = lb2; j <= ub2; j++)
                {
                    result[i - lb1, j - lb2] = inputArray[i][j];
                }
            }
            return result;
        }

        public static double[,] ConvertArrayToMatrix(this double[][] inputArray)
        {
            int c1 = inputArray.Length;
            int c2 = inputArray[inputArray.GetLowerBound(0)].Length;
            var result = new double[c1, c2];
            int lb1 = inputArray.GetLowerBound(0);
            int lb2 = inputArray[lb1].GetLowerBound(0);
            int ub1 = inputArray.GetUpperBound(0);
            int ub2 = inputArray[lb1].GetUpperBound(0);
            for (int i = lb1; i <= ub1; i++)
            {
                for (int j = lb2; j <= ub2; j++)
                {
                    result[i - lb1, j - lb2] = inputArray[i][j];
                }
            }
            return result;
        }

        public static T[] ConvertToOneDimensionalArray<T>(this object[][] inputArray)
        {
            var result = new List<T>();
            int lb1 = inputArray.GetLowerBound(0);
            int lb2 = inputArray[lb1].GetLowerBound(0);
            int ub1 = inputArray.GetUpperBound(0);
            int ub2 = inputArray[ub1].GetUpperBound(0);

            if (ub1 == lb1)
            {
                for (int i = lb2; i <= ub2; i++)
                {
                    result.Add((T)Convert.ChangeType(inputArray[0][i], typeof(T)));
                }
            }
            else if (ub2 == lb2)
            {
                for (int i = lb1; i <= ub1; i++)
                {
                    result.Add((T)Convert.ChangeType(inputArray[i][0], typeof(T)));
                }
            }
            else
            {
                throw new ArgumentException("Attempt to convert a 2D Range to 1D");
            }
            return result.ToArray();
        }

        public static NamedValueSet ToNamedValueSet(this object[][] inputArray)
        {
            int c1 = inputArray.Length;
            int c2 = inputArray[inputArray.GetLowerBound(0)].Length;
            var properties = new object[c1, c2];
            int lb1 = inputArray.GetLowerBound(0);
            int lb2 = inputArray[lb1].GetLowerBound(0);
            int ub1 = inputArray.GetUpperBound(0);
            int ub2 = inputArray[lb1].GetUpperBound(0);
            for (int i = lb1; i <= ub1; i++)
            {
                for (int j = lb2; j <= ub2; j++)
                {
                    properties[i - lb1, j - lb2] = inputArray[i][j];
                }
            }
            var result = new NamedValueSet(properties);
            return result;
        }
    }
}