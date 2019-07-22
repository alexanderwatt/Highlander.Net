/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Orion.Analytics.Counterparty
{
    /// <summary>
    /// Class to calculate return on equity 
    /// </summary> 
    public class ROEAnalytics
    {
        ///<summary>
        ///</summary>
        ///<param name="revenue"></param>
        ///<param name="sp"></param>
        ///<param name="cost"></param>
        ///<param name="rore"></param>
        ///<param name="dfCapital"></param>
        ///<param name="dfMarket"></param>
        ///<param name="taxRate"></param>
        ///<param name="frankingRate"></param>
        ///<param name="regCap"></param>
        ///<param name="ffp"></param>
        ///<param name="fxRate"></param>
        ///<returns></returns>
        public static decimal[] CalculateROE(decimal[] revenue, decimal[] sp, decimal[] cost, decimal[] rore, 
            decimal[] dfCapital, decimal[] dfMarket, decimal taxRate, decimal frankingRate, 
            decimal[] regCap, decimal ffp, decimal fxRate)
        {
            decimal sum1 = 0;
            decimal sum2 = 0;
            const decimal tol = 0.0001M;
            var roe1 = new decimal[revenue.Length];
            for (int k = 0; k < revenue.Length; k++)
            {
                for (int j = k; j < revenue.Length; j++)
                {

                    sum1 += ((revenue[j] - cost[j] + rore[j]) * dfCapital[j] - (sp[j] / fxRate) * dfMarket[j]) * (1.0M - taxRate * (1.0M - frankingRate));
                    sum2 += regCap[j] * dfCapital[j];

                }
                if ( ((-tol < sum1) && (sum1 < tol)) &&
                     ((-tol < sum2) && (sum2 < tol)) ) //zero condition
                    roe1[k] = 0.0M;
                else roe1[k] = ( sum1 / sum2 )* (12.0M / ffp);
            }
            return roe1;
        }

        /// <summary>
        /// Generates cost of capital discount factors.
        /// 
        /// Cost of capital is assumed to be provided as yearly pecentage value (decimal form)
        /// </summary>
        /// 
        /// <param name="revenueBuckets">Set of integer offsets (representing days from basedate)</param>
        /// <param name="costCapital">cost of capital p.a (decimal value of percentage)</param>
        /// <returns></returns>
        public static decimal[] GetCostOfCapitalDFs(int[] revenueBuckets, decimal costCapital)
        {
            var dfCapital = new decimal[revenueBuckets.Length];
            for (int j = 0; j < revenueBuckets.Length; j++)
            {
                //Using flat annually compounded rate
                dfCapital[j] = 1.0M / (decimal)Math.Pow(1.0 + (double)costCapital, revenueBuckets[j] / 365.0);
            }
            return dfCapital;
        }

        /// <summary>
        /// Generates cost of capital discount factors.
        /// </summary>
        /// <param name="revenueBuckets">Set of integer offsets (representing days from base date)</param>
        /// <param name="costCapitalRate">cost of capital p.a (decimal value of percentage)</param>
        /// <param name="valueDateOffset">Length of forward start. When start date > eval (base) date</param>
        /// <returns></returns>
        public static decimal[] GetCostOfCapitalDFs(int[] revenueBuckets, decimal costCapitalRate, int valueDateOffset)
        {
            var dfCapital = new decimal[revenueBuckets.Length];
            //NOTE: valueDateOffset must be > 0
            for (int j = 0; j < revenueBuckets.Length; j++)
            {
                //Using flat annually compounded rate
                dfCapital[j] = 1.0M / (decimal)Math.Pow(1.0 + (double)costCapitalRate, (revenueBuckets[j] - valueDateOffset) / 365.0);
            }
            return dfCapital;
        }

        /// <summary>
        /// Generates cost of capital discount factors.
        /// 
        /// Cost of capital is assumed to be provided as yearly pecentage value (decimal form)
        /// </summary>
        /// <param name="dateBuckets"></param>
        /// <param name="baseDate"></param>
        /// <param name="costCapital"></param>
        /// <returns></returns>
        public static decimal[] GetCostOfCapitalDFs(DateTime[] dateBuckets, DateTime baseDate, decimal costCapital)
        {
            var dfCapital = new decimal[dateBuckets.Length];
            for (int j = 0; j < dateBuckets.Length; j++)
            {
                //Using flat annually compounded rate
                TimeSpan ts = dateBuckets[j] - baseDate;
                int noOfDays = ts.Days;
                dfCapital[j] = 1.0M / (decimal)Math.Pow(1.0 + (double)costCapital, noOfDays / 365.0);
            }
            return dfCapital;
        }

        ///<summary>
        ///</summary>
        ///<param name="costCapital"></param>
        ///<returns></returns>
        public static decimal GetCostOfCapitalDiscountFactor(decimal costCapital)
        {
            return (1.0M / (1.0M + costCapital));
        }
    }
}
