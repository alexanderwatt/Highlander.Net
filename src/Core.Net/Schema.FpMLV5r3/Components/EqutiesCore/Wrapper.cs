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

namespace Highlander.Equities
{
    /// <summary>
    /// </summary>
    public class Wrapper
    {
        int _kdx;

        /// <summary>
        /// </summary>
        /// <param name="today"></param>
        /// <param name="dates"></param>
        /// <param name="amounts"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ZeroCurve UnpackZero(DateTime today, DateTime[] dates, double[] amounts)
        {
            int n1 = dates.Length;
            int n2 = dates.Length;
            if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
            var zc = new ZeroCurve {RatePoints = n1};
            zc.MakeArrays();
            int kdx = 0;
            for (int idx=0; idx<n1;idx++)
            {
                double time = dates[idx].Subtract(today).Days/365.0;
                double rate = amounts[idx];            
                zc.SetR(kdx, rate, time);
                kdx++;          
            }         
            return zc;
        }

        /// <summary>
        /// </summary>
        /// <param name="today"></param>
        /// <param name="expiry"></param>
        /// <param name="dates"></param>
        /// <param name="amounts"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DivList UnpackDiv(DateTime today, DateTime expiry, DateTime[] dates, double[] amounts)
        {
            int n1 = dates.Length;
            int n2 = dates.Length;
            double timeToExp = expiry.Subtract(today).Days / 365.0;
            if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
            var dl = new DivList {DivPoints = n1};
            dl.MakeArrays();
            for (int idx = 0; idx < n1; idx++)
            {
                double time = dates[idx].Subtract(today).Days / 365.0;
                double rate = amounts[idx];
                if (time > 0 & time <= timeToExp)
                {
                    dl.SetD(_kdx, rate, time);
                    _kdx++;
                }
            }
            return dl;
        }
    }
}
