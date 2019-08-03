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
    ///<summary>
    ///</summary>
    public class RAROCAnalytics
    {
        ///<summary>
        ///</summary>
        ///<param name="revenue"></param>
        ///<param name="sp"></param>
        ///<param name="cost"></param>
        ///<param name="rorc"></param>
        ///<param name="dfCapital"></param>
        ///<param name="taxRate"></param>
        ///<param name="frankingRate"></param>
        ///<param name="riskCap"></param>
        ///<param name="ffp"></param>
        ///<param name="fxRate"></param>
        ///<returns></returns>
        public static decimal[] CalculateRAROC(decimal[] revenue, decimal[] sp, decimal[] cost, decimal[] rorc, decimal[] dfCapital, decimal taxRate, decimal frankingRate, decimal[] riskCap, decimal ffp, decimal fxRate)
        {
            decimal sum1 = 0;
            decimal sum2 = 0;
            var raroc1 = new decimal[revenue.Length];
            for (int k = 0; k < revenue.Length; k++)
            {
                for (int j = k; j < revenue.Length; j++)
                {
                    sum1 += (revenue[j] - cost[j] + rorc[j]  - (sp[j] / fxRate)) * (1.0M - taxRate * (1.0M - frankingRate)) * dfCapital[j];
                    sum2 += riskCap[j] * dfCapital[j];
                }
                raroc1[k] = (sum1 / sum2) * (12.0M / ffp);
            }
            return raroc1;
        }

        ///<summary>
        ///</summary>
        ///<param name="revenueBuckets"></param>
        ///<param name="costCapital"></param>
        ///<returns></returns>
        public static decimal[] GetCostOfCapitalDFs(int[] revenueBuckets, decimal costCapital)
        {
            var dfCapital = new decimal[revenueBuckets.Length];
            for (int j = 0; j < revenueBuckets.Length; j++)
            {
                //Using flat anually compounded rate
                dfCapital[j] = 1.0M / (decimal)Math.Pow(1.0 + (double)costCapital, revenueBuckets[j] / 365.0);
            }
            return dfCapital;
        }
    }
}