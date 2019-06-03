#region Using directives

using System;
using Orion.Analytics.LinearAlgebra.Sparse;

#endregion

namespace Orion.Analytics.Tests.Numerics
{
    /// <summary> Utilities for the testers</summary>
    internal class TesterUtilities
    {
        private TesterUtilities()
        {
        }

        /// <summary> Creates a double array of the given size</summary>
        public static double[] getDoubleArray(int n, System.Random r)
        {
            double[] arr = new double[n];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = r.NextDouble();
            return arr;
        }

        /// <summary> Creates a double array of the given size</summary>
        public static double[,] getDoubleArray(int n, int m, System.Random r)
        {
            double[,] arr = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				arr[i] = new double[m];
            //			}
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < m; ++j)
                    arr[i, j] = r.NextDouble();
            return arr;
        }

        /// <summary> Creates an integer array of the given size and maximum amplitude</summary>
        public static int[] getIntArray(int max, System.Random r)
        {
            int[] arr = new int[getInt(max, r)];
            for (int k = 0; k < arr.Length; ++k)
            {
                arr[k] = getInt(max, r);
                if (arr[k] >= max)
                    throw new IndexOutOfRangeException();
            }
            return arr;
        }

        /// <summary> Returns an integer between zero (inclusive) and max (exclusive)</summary>
        public static int getInt(int max, Random r)
        {
            return (int)(r.NextDouble() * (max));
        }

        /// <summary> Dense representation of the matrix using getValue</summary>
        public static double[,] getElMatrix(IElementalAccessMatrix A)
        {
            double[,] ret = new double[A.RowCount, A.ColumnCount];
            //			for (int i = 0; i < A.RowCount; i++)
            //			{
            //				ret[i] = new double[A.ColumnCount];
            //			}
            for (int i = 0; i < A.RowCount; ++i)
                for (int j = 0; j < A.ColumnCount; ++j)
                    ret[i, j] = A.GetValue(i, j);
            return ret;
        }

        /// <summary> Dense representation of the matrix using getValues</summary>
        public static double[,] getMatrix(IElementalAccessMatrix A)
        {
            int[] row = new int[A.RowCount], col = new int[A.ColumnCount];
            for (int i = 0; i < row.Length; ++i)
                row[i] = i;
            for (int i = 0; i < col.Length; ++i)
                col[i] = i;
            return A.GetValues(row, col);
        }

        /// <summary> Dense representation of the matrix using getArrayCopy</summary>
        public static double[,] getMatrixCopy(IMatrix A)
        {
            return Blas.Default.GetArrayCopy(A);
        }

        /// <summary> Dense representation of the vector using getValue</summary>
        public static double[] getElVector(IElementalAccessVector x)
        {
            double[] ret = new double[x.Length];
            for (int i = 0; i < ret.Length; ++i)
                ret[i] = x.GetValue(i);
            return ret;
        }

        /// <summary> Dense representation of the vector using getValues</summary>
        public static double[] getVector(IElementalAccessVector x)
        {
            int[] ind = new int[x.Length];
            for (int i = 0; i < ind.Length; ++i)
                ind[i] = i;
            return x.GetValues(ind);
        }

        /// <summary> Dense representation of the vector using getArrayCopy</summary>
        public static double[] getVectorCopy(IVector x)
        {
            return Blas.Default.GetArrayCopy(x);
        }

        /// <summary> Row-wise assembly using setValue</summary>
        /// <param name="nu">Maximum bandwidth on each row
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] SetAssembleRowMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < n; ++i)
                for (int j = 0; j < nu; ++j)
                {
                    int ind = TesterUtilities.getInt(m, r);
                    double entry = r.NextDouble();
                    data[i, ind] = entry;
                    A.SetValue(i, ind, entry);
                }

            return data;
        }

        /// <summary> Column-wise assembly using set</summary>
        /// <param name="nu">Maximum bandwidth on each column
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] setAssembleColumnMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            // Construct the matrix
            for (int i = 0; i < m; ++i)
                for (int j = 0; j < nu; ++j)
                {
                    int ind = TesterUtilities.getInt(n, r);
                    double entry = r.NextDouble();
                    data[ind, i] = entry;
                    A.SetValue(ind, i, entry);
                }

            return data;
        }

        /// <summary> Assembly using set</summary>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[] setAssembleVector(IElementalAccessVector x)
        {
            int n = x.Length;
            double[] data = new double[n];
            System.Random r = new System.Random();

            for (int i = 0; i < n; ++i)
            {
                double entry = r.NextDouble();
                data[i] = entry;
                x.SetValue(i, entry);
            }
            return data;
        }

        /// <summary> Row-wise assembly using setValues</summary>
        /// <param name="nu">Maximum bandwidth on each row
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] setsAssembleRowMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < n; ++i)
            {
                int[] row = TesterUtilities.getIntArray(n, r), col = TesterUtilities.getIntArray(nu, r);
                double[,] entry = TesterUtilities.getDoubleArray(row.Length, col.Length, r);
                for (int ii = 0; ii < row.Length; ++ii)
                    for (int jj = 0; jj < col.Length; ++jj)
                        data[row[ii], col[jj]] = entry[ii, jj];
                A.SetValues(row, col, entry);
            }

            return data;
        }

        /// <summary> Row-wise assembly using addValues</summary>
        /// <param name="nu">Maximum bandwidth on each row
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] addsAssembleRowMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < n; ++i)
            {
                int[] row = TesterUtilities.getIntArray(n, r), col = TesterUtilities.getIntArray(nu, r);
                double[,] entry = TesterUtilities.getDoubleArray(row.Length, col.Length, r);
                for (int ii = 0; ii < row.Length; ++ii)
                    for (int jj = 0; jj < col.Length; ++jj)
                        data[row[ii], col[jj]] += entry[ii, jj];
                A.AddValues(row, col, entry);
            }

            return data;
        }

        /// <summary> Row-wise assembly using add</summary>
        /// <param name="nu">Maximum bandwidth on each row
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] AddAssembleRowMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < n; ++i)
                for (int j = 0; j < nu; ++j)
                {
                    int ind = TesterUtilities.getInt(m, r);
                    double entry = r.NextDouble();
                    data[i, ind] += entry;
                    A.AddValue(i, ind, entry);
                }

            return data;
        }

        /// <summary> Column-wise assembly using addValues</summary>
        /// <param name="nu">Maximum bandwidth on each column
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] addsAssembleColumnMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < m; ++i)
            {
                int[] row = TesterUtilities.getIntArray(nu, r), col = TesterUtilities.getIntArray(m, r);
                double[,] entry = TesterUtilities.getDoubleArray(row.Length, col.Length, r);
                for (int ii = 0; ii < row.Length; ++ii)
                    for (int jj = 0; jj < col.Length; ++jj)
                        data[row[ii], col[jj]] += entry[ii, jj];
                A.AddValues(row, col, entry);
            }

            return data;
        }

        /// <summary> Column-wise assembly using setValues</summary>
        /// <param name="nu">Maximum bandwidth on each column
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] setsAssembleColumnMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < m; ++i)
            {
                int[] row = TesterUtilities.getIntArray(nu, r), col = TesterUtilities.getIntArray(m, r);
                double[,] entry = TesterUtilities.getDoubleArray(row.Length, col.Length, r);
                for (int ii = 0; ii < row.Length; ++ii)
                    for (int jj = 0; jj < col.Length; ++jj)
                        data[row[ii], col[jj]] = entry[ii, jj];
                A.SetValues(row, col, entry);
            }

            return data;
        }

        /// <summary> Column-wise assembly using add</summary>
        /// <param name="nu">Maximum bandwidth on each column
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[,] addAssembleColumnMatrix(IElementalAccessMatrix A, int nu)
        {
            int n = A.RowCount, m = A.ColumnCount;
            double[,] data = new double[n, m];
            //			for (int i = 0; i < n; i++)
            //			{
            //				data[i] = new double[m];
            //			}
            System.Random r = new System.Random();

            for (int i = 0; i < m; ++i)
                for (int j = 0; j < nu; ++j)
                {
                    int ind = TesterUtilities.getInt(n, r);
                    double entry = r.NextDouble();
                    data[ind, i] += entry;
                    A.AddValue(ind, i, entry);
                }

            return data;
        }

        /// <summary> Assembly using addValue</summary>
        /// <param name="nu">Maximum number of entries
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[] addAssembleVector(IElementalAccessVector x, int nu)
        {
            int n = x.Length;
            double[] data = new double[n];
            System.Random r = new System.Random();

            for (int i = 0; i < nu; ++i)
            {
                int ind = TesterUtilities.getInt(n, r);
                double entry = r.NextDouble();
                data[ind] += entry;
                x.AddValue(ind, entry);
            }

            return data;
        }

        /// <summary> Assembly using setValue</summary>
        /// <param name="nu">Maximum number of entries
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[] setAssembleVector(IElementalAccessVector x, int nu)
        {
            int n = x.Length;
            double[] data = new double[n];
            System.Random r = new System.Random();

            for (int i = 0; i < nu; ++i)
            {
                int ind = TesterUtilities.getInt(n, r);
                double entry = r.NextDouble();
                data[ind] = entry;
                x.SetValue(ind, entry);
            }

            return data;
        }

        /// <summary> Assembly using addValues</summary>
        /// <param name="nu">Maximum number of entries
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[] addsAssembleVector(IElementalAccessVector x, int nu)
        {
            int n = x.Length;
            double[] data = new double[n];
            System.Random r = new System.Random();

            int[] ind = TesterUtilities.getIntArray(nu, r);
            double[] arr = TesterUtilities.getDoubleArray(ind.Length, r);
            for (int i = 0; i < ind.Length; ++i)
                data[ind[i]] += arr[i];
            x.AddValues(ind, arr);
            return data;
        }

        /// <summary> Assembly using setValues</summary>
        /// <param name="nu">Maximum number of entries
        /// </param>
        /// <returns> Dense representation (not a direct copy)
        /// </returns>
        public static double[] setsAssembleVector(IElementalAccessVector x, int nu)
        {
            int n = x.Length;
            double[] data = new double[n];
            System.Random r = new System.Random();

            int[] ind = TesterUtilities.getIntArray(nu, r);
            double[] arr = TesterUtilities.getDoubleArray(ind.Length, r);
            for (int i = 0; i < ind.Length; ++i)
                data[ind[i]] = arr[i];
            x.SetValues(ind, arr);
            return data;
        }
    }
}