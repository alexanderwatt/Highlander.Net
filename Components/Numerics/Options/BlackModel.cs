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

#region Using directives

using System;
using Highlander.Numerics.Distributions;

#endregion

namespace Highlander.Numerics.Options
{
    #region Public Enum for Valid Swaption Types

    /// <summary>
    /// Enumerated type for the category of valid Black vanilla swaptions. 
    /// </summary>
    public enum PayStyle
    {
        /// <summary>
        /// Call.
        /// </summary>
        Call,
        /// <summary>
        /// Put
        /// </summary>
        Put
    }

    #endregion Public Enum for Valid Swaption Types

    ///<summary>
    ///</summary>
    public static class BlackModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <param name="pay"></param>
        /// <returns></returns>
        public static double GetValue(double t, double f, double k, double v, PayStyle pay)
        {
            var d1 = (Math.Log(f / k) + 0.5 * v * v * t) / v / Math.Sqrt(t);
            var d2 = d1 - v * Math.Sqrt(t);
            double n1 = new NormalDistribution().CumulativeDistribution(d1);
            double n2 = new NormalDistribution().CumulativeDistribution(d2);
            if (pay == PayStyle.Call)
            {
                return f * n1 - k * n2;
            }
            return k * (1 - n2) - f * (1 - n1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <param name="p"></param>
        /// <param name="atm"></param>
        /// <param name="pay"></param>
        /// <returns></returns>
        public static double GetImpliedVolatility(double t, double f, double k, double p, double atm, PayStyle pay)
        {
            const double tolerance = 0.000000001;
            double result = atm;

            for (int i = 0; i < 50; i++)
            {
                var fun = GetValue(t, f, k, result, pay) - p;
                if (Math.Abs(fun) < tolerance)
                {
                    return result;
                }
                var f1 = GetValue(t, f, k, result + 0.00001, pay) - p;
                var delta = -fun * 0.00001 / (f1 - fun);
                result += 0.7 * delta;
                //0.7 is a dampening factor
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="rate"></param>
        ///<param name="strikeRate"></param>
        ///<param name="volatility"></param>
        ///<param name="timeToExpiry"></param>
        ///<returns></returns>
        public static double GetSwaptionValue(double rate, double strikeRate, double volatility, double timeToExpiry)
        {
            double d1 = (Math.Log(rate / strikeRate) + 0.5 * Math.Pow(volatility, 2) * timeToExpiry) / volatility * timeToExpiry;
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiry);
            // Compute the cumulative Normal distribution evaluated at
            // d1 and d2.
            double n1 = new NormalDistribution().CumulativeDistribution(d1);
            double n2 = new NormalDistribution().CumulativeDistribution(d2);          
            // Compute and return the price of a Black vanilla payer swaption.
            double price = rate * n1 - strikeRate * n2;
            return price;
        }
        
        ///<summary>
        ///</summary>
        ///<param name="floatRate"></param>
        ///<param name="strikeRate"></param>
        ///<param name="volatility"></param>
        ///<param name="timeToExpiry"></param>
        ///<returns></returns>
        public static double GetCapletValue(double floatRate, double strikeRate, double volatility, double timeToExpiry)
        {
            double d1 = (Math.Log(floatRate / strikeRate) + 0.5 * Math.Pow(volatility, 2) * timeToExpiry) / volatility * timeToExpiry;
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiry);
            // Compute the cumulative Normal distribution evaluated at
            // d1 and d2.
            double n1 = new NormalDistribution().CumulativeDistribution(d1);
            double n2 = new NormalDistribution().CumulativeDistribution(d2);          
            // Compute and return the price of a Black vanilla payer swaption.
            double price = (floatRate * n1 - strikeRate * n2);
            return price;
        }
    }
}