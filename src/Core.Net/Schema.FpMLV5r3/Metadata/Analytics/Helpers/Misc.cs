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

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class Misc
    {
        public static double ScaleVariable(double rawValue, double minRange, double maxRange)
        {
            double scaledOut = (rawValue - minRange) / (maxRange - minRange);
            return scaledOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scaledValue"></param>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        /// <returns></returns>
        public static double DeScaleVariable(double scaledValue, double minRange, double maxRange)
        {
            double scaledOut = scaledValue * (maxRange - minRange) + minRange;
            return scaledOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="col1"></param>
        /// <param name="col2"></param>
        /// <returns></returns>
        public static object[,] Extract2Columns(object[,] inputArray, int col1, int col2)
        {
            int length = inputArray.GetUpperBound(0);
            var output = new object[length+1,2];

            for (int i = 0; i <= length; i++)
            {
                output[i, 0] = inputArray[i, col1-1];
                output[i, 1] = inputArray[i, col2-1];
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double CummNormDist(double x)
        {
            double xAbs = Math.Abs(x);
            double cumNorm;

            if (xAbs > 37)
            {
                cumNorm = 0;
            }
            else
            {
                double exponential = Math.Exp(-Math.Pow(xAbs, 2) / 2);
                double build;
                if (xAbs < 7.07106781186547)
                {
                    build = 3.52624965998911E-02 * xAbs + 0.700383064443688;
                    build = build * xAbs + 6.37396220353165;
                    build = build * xAbs + 33.912866078383;
                    build = build * xAbs + 112.079291497871;
                    build = build * xAbs + 221.213596169931;
                    build = build * xAbs + 220.206867912376;
                    cumNorm = exponential * build;
                    build = 8.83883476483184E-02 * xAbs + 1.75566716318264;
                    build = build * xAbs + 16.064177579207;
                    build = build * xAbs + 86.7807322029461;
                    build = build * xAbs + 296.564248779674;
                    build = build * xAbs + 637.333633378831;
                    build = build * xAbs + 793.826512519948;
                    build = build * xAbs + 440.413735824752;
                    cumNorm = cumNorm / build;
                }
                else
                {
                    build = xAbs + 0.65;
                    build = xAbs + 4 / build;
                    build = xAbs + 3 / build;
                    build = xAbs + 2 / build;
                    build = xAbs + 1 / build;
                    cumNorm = exponential / build / 2.506628274631;
                }
            }

            if (x > 0) { cumNorm = 1 - cumNorm; }

            return cumNorm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static double TrapezoidalArea(double a, double b, double h)
        {
            return 0.5 * h * (a + b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static double[,] NormVarZ(int num, int seed)
        {
            var output = new double[num, 1];
            var T = new Random(seed);

            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    output[i, 0] += T.NextDouble();
                }
                output[i, 0] -= 6;
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mean1"></param>
        /// <param name="mean2"></param>
        /// <param name="stDev1"></param>
        /// <param name="stDev2"></param>
        /// <param name="correlation"></param>
        /// <returns></returns>
        public static double ProductNormalMean(double mean1, double mean2, double stDev1, double stDev2, double correlation)
        {
            return mean1 * mean2 + correlation * stDev1 * stDev2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mean1"></param>
        /// <param name="mean2"></param>
        /// <param name="stDev1"></param>
        /// <param name="stDev2"></param>
        /// <param name="correlation"></param>
        /// <returns></returns>
        public static double ProductNormalStDev(double mean1, double mean2, double stDev1, double stDev2, double correlation)
        {
            double term1 = Math.Pow(mean1, 2) * Math.Pow(stDev2, 2);
            double term2 = Math.Pow(mean2, 2) * Math.Pow(stDev1, 2);
            double term3 = Math.Pow(stDev1, 2) * Math.Pow(stDev2, 2);
            double term4 = 2 * correlation * mean1 * mean2 * stDev1 * stDev2;
            double term5 = Math.Pow(correlation, 2) * Math.Pow(stDev1, 2) * Math.Pow(stDev2, 2);

            return Math.Sqrt(term1 + term2 + term3 + term4 + term5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static double[,] CholeskyDecomp(double[,] A)
        {
            // The matrix A should be symetric and positive-definate

            int size = A.GetUpperBound(0);
            var L = new double[size + 1, size + 1];

            for (int i = 0; i <= size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        if (k < j) { L[i, j] -= (L[i, k] * L[j, k]); }
                        if (k == j) { L[i, j] += A[i, j]; }
                    }
                    if (i == j) { L[i, j] = Math.Sqrt(L[i, j]); }
                    if (i != j) { L[i, j] = L[i, j] / L[j, j]; }
                }
            }

            return L;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double Determinant(double[,] a)
        {
            int size = a.GetUpperBound(0);
            a = CholeskyDecomp(a);
            double output = a[0, 0];
            for (int i = 1; i <= size; i++) { output *= a[i, i]; }
            output = Math.Pow(output, 2);
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static object[,] SquareSubMatrix(object[,] input, int row, int col)
        {
            int size = input.GetUpperBound(0);
            var output = new object[size, size];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++) { output[i, j] = input[i, j]; }
                for (int j = col; j < size; j++) { output[i, j] = input[i, j + 1]; }
            }

            for (int i = row; i < size; i++)
            {
                for (int j = 0; j < col; j++) { output[i, j] = input[i + 1, j]; }
                for (int j = col; j < size; j++) { output[i, j] = input[i + 1, j + 1]; }
            }

            return output;
        }
    }
}
