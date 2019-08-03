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

namespace Orion.Analytics.Counterparty
{
    /// <summary>
    /// Class to calculate RORE (Return on Regulatory Equity)
    /// </summary>
    public class ROREAnalytics
    {
        /// <summary>
        /// Method to calculate RORE as RORE = r_{riskFree}*RE*Delta(t) where Delta(t) is year fraction (ACT365)
        /// 
        /// RORE is return on regulatory equity.
        /// 
        /// Note: Uses forward difference (except for last time bucket where backward difference is used)
        /// </summary>
        /// <param name="revenueBuckets">time buckets (day offsets)</param>
        /// <param name="regCap">regulatory capital amounts for time buckets</param>
        /// <param name="riskFree">per period risk free rate</param>
        /// <returns>RORE vector</returns>
        public static decimal[] CalculateRORE(int[] revenueBuckets, decimal[] regCap, decimal riskFree)
        {
            decimal delta_t;
            var rore = new decimal[revenueBuckets.Length];
            if (revenueBuckets.Length < 2) throw new Exception("Invalid number of revenue buckets");
            for (int j = 0; j < revenueBuckets.Length; j++)
            {          
                if (j == (revenueBuckets.Length - 1))
                {
                    //backward difference (last revenue bucket)
                    delta_t = (revenueBuckets[j] - revenueBuckets[j - 1]) / 365.0M; 
                }
                else
                {
                    //forward difference
                    delta_t = (revenueBuckets[j + 1] - revenueBuckets[j]) / 365.0M;
                }
                rore[j] = riskFree * regCap[j] * delta_t; 
            }
            return rore;
        }
    }
}