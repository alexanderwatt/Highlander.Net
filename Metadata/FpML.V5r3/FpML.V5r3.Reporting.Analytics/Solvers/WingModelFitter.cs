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
using Orion.Analytics.Helpers;
using Orion.Analytics.LinearAlgebra;

namespace Orion.Analytics.Solvers
{
    ///<summary>
    ///</summary>
    public class WingModelFitter
    {
        /// <summary>
        /// </summary>
        /// <param name="vols"></param>
        /// <param name="strikes"></param>
        /// <param name="forward"></param>
        /// <param name="spot"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static OrcWingParameters FitWing(double[] strikes,
                                         double[] vols,                                       
                                         double forward,
                                         double spot)
        {
            int n = strikes.Length;
            if (n != vols.Length) throw new Exception("Unequal length vectors");
            int n1 = 0;
            for (int idx = 0; idx < n & n1 == 0; idx++)
            {
                if (strikes[idx] < strikes[(idx == 0) ? 0 : idx - 1])
                {
                    throw new Exception("Strikes not ordered");
                }
                if (n1 == 0 & strikes[idx] > forward)
                {
                    n1 = idx;
                }
            }
            int n2 = n - n1;
            var lnStrike1 = new double[n1];
            var lnStrike2 = new double[n2];
            for (int idx = 0; idx < n1; idx++)
            {
                lnStrike1[idx] = Math.Log(strikes[idx] / forward);
            }
            for (int idx = 0; idx < n2; idx++)
            {
                lnStrike2[idx] = Math.Log(strikes[n1 + idx] / forward);
            }
            Matrix xz = CreateXz(lnStrike1, lnStrike2);
            Matrix xzTr = CreateXzdash(lnStrike1, lnStrike2);
            Matrix a = xzTr * xz;
            //int aCols = a.ColumnCount;
            //int aRows = a.RowCount;
            var y = new Matrix(n1 + n2, 1);
            for (int idx = 0; idx < n1 + n2; idx++)
            {
                y[idx, 0] = vols[idx];
            }
            Matrix b = xzTr * y;
            Matrix c = a.Solve(b);
            var result = new OrcWingParameters
                             {
                                 Vcr = 0,
                                 Scr = 0,
                                 AtmForward = forward,
                                 PutCurve = c[2, 0],
                                 CallCurve = c[3, 0],
                                 CurrentVol = c[0, 0],
                                 DnCutoff = -0.50 - Math.Log(forward/spot),
                                 UpCutoff = 0.50 - Math.Log(forward/spot),
                                 RefFwd = forward,
                                 RefVol = c[0, 0],
                                 Dsr = 0.20,
                                 Usr = 0.20,
                                 Ssr = 100.0,
                                 SlopeRef = c[1, 0]
                             };         
            return result;
        }

        static Matrix CreateXz(double[] x1, double[] x2)
        {
            int n1 = x1.Length;
            int n2 = x2.Length;
            var z = new Matrix(n1 + n2, 4);

            //top left 
            for (int idx = 0; idx < n1; idx++)
            {
                for (int jdx = 0; jdx <= 2; jdx++)
                {
                    z[idx, jdx] = Math.Pow(x1[idx], jdx);
                }

            }
            //bottom left
            for (int idx = 0; idx < n2; idx++)
            {
                for (int jdx = 0; jdx < 2; jdx++)
                {
                    z[idx + n1, jdx] = Math.Pow(x2[idx], jdx);
                    z[idx + n1, 3] = Math.Pow(x2[idx], 2);
                }

            }
            return z;
        }

        static Matrix CreateXzdash(double[] x1, double[] x2)
        {
            int n1 = x1.Length;
            int n2 = x2.Length;
            var z = new Matrix(4, n1 + n2);
            //top 2 rows
            for (int idx = 0; idx < 2; idx++)
            {
                for (int jdx = 0; jdx < n1; jdx++)
                {
                    z[idx, jdx] = Math.Pow(x1[jdx], idx);
                }
                for (int jdx = 0; jdx < n2; jdx++)
                {
                    z[idx, jdx + n1] = Math.Pow(x2[jdx], idx);
                }
            }
            // 3rd row
            for (int jdx = 0; jdx < n1; jdx++)
            {
                z[2, jdx] = Math.Pow(x1[jdx], 2);
            }
            // 4th row
            for (int jdx = 0; jdx < n2; jdx++)
            {
                z[3, jdx + n1] = Math.Pow(x2[jdx], 2);
            }
            return z;
        }     
    }
}