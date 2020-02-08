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
using System.Collections;
using System.Collections.Generic;
using Highlander.Equities;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.Utilities.Serialisation;

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
    /// <summary>
    /// Helper class for Volatility Surface
    /// </summary>
    public static class VolatilitySurfaceHelper
    {
        /// <summary>
        /// Gets the closest non-zero volatility strike to money.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        /// 
        public static double GetClosestNonZeroVolStrikeToMoney(ForwardExpiry expiry)
        {
            decimal fwdPrice = expiry.FwdPrice;
            var expiryCopy = BinarySerializerHelper.Clone(expiry);
            //Remove strikes with zero vol in the search object
            //Want to return the location of the nearest *non-zero* strike
            foreach (EquityStrike strike in expiryCopy.Strikes)
            {
                if (strike.Volatility.Value == 0)
                    expiryCopy.RemoveStrike(strike.StrikePrice);
            }
            /* Bin search bitwise complement returns first element larger than search key
             * Returns Array Length + 1 if search key greater than all elements */
            expiryCopy.RawStrikePrices.Sort();
            double[] strikes = expiryCopy.RawStrikePrices.ToArray();
            int index = Array.BinarySearch(strikes, Convert.ToDouble(fwdPrice));
            if (index < 0)
            {
                index = ~index;
            }
            if (expiryCopy.RawStrikePrices.Count == 0)
                return 0.0;
            if (index < expiryCopy.RawStrikePrices.Count)
            {
                double dist1 = Math.Abs(strikes[index] - Convert.ToDouble(fwdPrice));
                double dist2 = 0.0;
                if (index >= 1)
                    dist2 = Math.Abs(strikes[index - 1] - Convert.ToDouble(fwdPrice));
                if (dist1 <= dist2 || index == 0)
                    return expiryCopy.RawStrikePrices[index];
                return expiryCopy.RawStrikePrices[index - 1];
            }
            return expiryCopy.RawStrikePrices[expiryCopy.RawStrikePrices.Count - 1];
        }

        /// <summary>
        /// Gets the  i-th closest non-zero volatility strike to money.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double GetClosestNonZeroVolStrikeToMoney(ForwardExpiry expiry, int i)
        {
            decimal fwdPrice = expiry.FwdPrice;
            var expiryCopy = BinarySerializerHelper.Clone(expiry);
            //Remove strikes with zero vol in the search object
            //Want to return the location of the nearest *non-zero* strike
            foreach (EquityStrike strike in expiryCopy.Strikes)
            {
                if (strike.Volatility.Value == 0)
                    expiryCopy.RemoveStrike(strike.StrikePrice);
            }
            var newStrikes = new double[expiryCopy.RawStrikePrices.Count];
            expiryCopy.RawStrikePrices.Sort();
            double[] strikes = expiryCopy.RawStrikePrices.ToArray();

            if (strikes.Length < i)
                return 0;
            var keys= new int[strikes.Length];
            for(int idx=0;idx<strikes.Length;idx++)
            {
                newStrikes[idx] = strikes[idx] - (double)fwdPrice;
                keys[idx] = idx;
            }
            //Get min distance to forward
            Array.Sort(newStrikes, keys, new DistanceComparer());           
            if (i > 0)
                return expiryCopy.Strikes[keys[i - 1]].StrikePrice;
            return 0;
            /* Bin search bitwise complement returns first element larger than search key
             * Returns Array Length + 1 if search key greater than all elements */           
        }

        /// <summary>
        /// Gets the closest non-zero volatility strike to money.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        /// 
        public static EquityStrike GetClosestStrikeToMoney(ForwardExpiry expiry)
        {
            decimal fwdPrice = expiry.FwdPrice;
            var expiryCopy = BinarySerializerHelper.Clone(expiry);
            /* Bin search bitwise complement returns first element larger than search key
             * Returns Array Length + 1 if search key greater than all elements */
            expiryCopy.RawStrikePrices.Sort();
            double[] strikes = expiryCopy.RawStrikePrices.ToArray();
            int index = Array.BinarySearch(strikes, Convert.ToDouble(fwdPrice));
            if (index < 0)
            {
                index = ~index;
            }
            if (expiryCopy.Strikes.Length == 0)
                return null;
            if (index < expiryCopy.Strikes.Length)
            {
                double dist1 = Math.Abs(strikes[index] - Convert.ToDouble(fwdPrice));
                double dist2 = 0.0;
                if (index >= 1)
                    dist2 = Math.Abs(strikes[index - 1] - Convert.ToDouble(fwdPrice));
                if (dist1 <= dist2 || index == 0)
                    return expiryCopy.Strikes[index];
                return expiryCopy.Strikes[index - 1];
            }
            return expiryCopy.Strikes[expiryCopy.RawStrikePrices.Count - 1];
        }

        /// <summary>
        /// Determines whether [is dividend ex date] [the specified valuation].
        /// </summary>
        /// <param name="valuations">The valuations.</param>
        /// <param name="dividends">The dividends.</param>
        /// <returns>
        /// 	<c>true</c> if [is dividend ex date] [the specified valuation]; otherwise, <c>false</c>.
        /// </returns>
        public static void UpdateDivsValuations(List<Valuation> valuations, List<Dividend> dividends)
        {
            //reset all to false
            foreach (Valuation val in valuations)
            {
                val.ExDate = false;
            }
            // create indicator to true 
            if (dividends != null)
            {
                foreach (Dividend dividend in dividends)
                {
                    Valuation match = valuations.Find(valuationItem => (valuationItem.Date == dividend.ExDate)
                        );
                    if (match != null)
                        match.ExDate = true;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified strike is match.
        /// </summary>
        /// <param name="strike">The strike.</param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expiries">The expiries.</param>
        /// <returns>
        /// 	<c>true</c> if the specified strike is match; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMatch(double strike, DateTime expiry, ForwardExpiry[] expiries)
        {
            ForwardExpiry exp = ForwardExpiryHelper.FindExpiry(expiry, new List<ForwardExpiry>(expiries));
            if (exp==null)
                return false;         
            foreach (EquityStrike str in exp.Strikes)
            {
                int i;
                StrikeHelper.FindStrikeWithPrice(strike, new List<EquityStrike>(exp.Strikes), out i);
                if (i >= 0)
                    return true;
            }
            return false;           
        }

        /// <summary>
        /// Determines whether the specified strike is match.
        /// </summary>
        /// <param name="strike">The strike.</param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expiries">The expiries.</param>
        /// <returns>
        /// 	<c>true</c> if the specified strike is match; otherwise, <c>false</c>.
        /// </returns>
        public static EquityStrike GetStrike(double strike, DateTime expiry, ForwardExpiry[] expiries)
        {
            ForwardExpiry exp = ForwardExpiryHelper.FindExpiry(expiry, new List<ForwardExpiry>(expiries));
            if (exp == null)
                return null;
            foreach (EquityStrike str in exp.Strikes)
            {
                int i;
                StrikeHelper.FindStrikeWithPrice(strike, new List<EquityStrike>(exp.Strikes), out i);
                if (i >= 0)
                    return exp.Strikes[i];
            }
            return null;
        }           
    }

    /// <summary>
    /// 
    /// </summary>
    public class DistanceComparer : IComparer
    {
          // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
          int IComparer.Compare( Object x, Object y )
          {
              if (Math.Abs((double)x) < Math.Abs((double)y)) return -1;
              if (Math.Abs((double)x) == Math.Abs((double)y)) return 0;
              return 1;
          }
    }
}
