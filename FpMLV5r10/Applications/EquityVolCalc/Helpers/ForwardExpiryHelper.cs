/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.EquityVolatilityCalculator.Exception;
using Orion.Util.Helpers;

namespace FpML.V5r10.EquityVolatilityCalculator.Helpers
{
    /// <summary>
    /// Helper class for Expiries
    /// </summary>
    public static class ForwardExpiryHelper
    {
        //const int CExpiryDays = 5;
        
        /// <summary>
        /// Adds the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        /// <param name="expiries">The expiries.</param>
        internal static void AddExpiry(ForwardExpiry expiry, List<ForwardExpiry> expiries)
        {
            ForwardExpiry match = FindExpiry(expiry.ExpiryDate, expiry.FwdPrice, expiry.InterestRate, expiries);
            if (match == null)
            {
                if (expiries.Count == 0)
                {
                    expiries.Add(expiry);
                }
                else
                {
                    List<ForwardExpiry> expiriesFound = expiries.FindAll(
                        expiryItem => (expiryItem.ExpiryDate < expiry.ExpiryDate)
                        );
                    expiries.Insert(expiriesFound.Count == 0 ? 0 : expiriesFound.Count, expiry);
                }
            }
            else
            {
                throw new DuplicateNotAllowedException(
                    $"An expiry date {expiry.ExpiryDate} with price {expiry.FwdPrice} already exists on this surface");
            }
        }

        /// <summary>
        /// Finds the expiry.
        /// </summary>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="forwardPrice">The forward price.</param>
        /// <param name="interestRate">The interest rate.</param>
        /// <param name="expiries">The expiries.</param>
        /// <returns></returns>
        internal static ForwardExpiry FindExpiry(DateTime expiryDate, decimal forwardPrice, Double interestRate, List<ForwardExpiry> expiries)
        {
            return expiries.Find(
                expiryItem =>
                (DateTime.Compare(expiryItem.ExpiryDate, expiryDate) == 0 && expiryItem.FwdPrice == forwardPrice &&
                 expiryItem.InterestRate == interestRate)
                );
        }


        /// <summary>
        /// Finds the expiry.
        /// </summary>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="expiries">The expiries.</param>
        /// <returns></returns>
        public static ForwardExpiry FindExpiry(DateTime expiryDate, List<ForwardExpiry> expiries)
        {
            return expiries.Find(expiryItem => (expiryItem.ExpiryDate.Subtract(expiryDate).Days == 0)
                );
        }


        /// <summary>
        /// Groups the strikes by moneyness.
        /// </summary>
        /// <param name="forwardPrice">The forward price.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="rawStrikePrices">The raw strike prices.</param>
        /// <param name="strikesLessThanFwd">The strikes less than FWD.</param>
        /// <param name="strikesEqualToFwd">The strikes equal to FWD.</param>
        /// <param name="strikesGreaterThanFwd">The strikes greater than FWD.</param>
        internal static void GroupStrikesByMoneyness(decimal forwardPrice, List<Strike> strikes, List<Double> rawStrikePrices, ref List<Strike> strikesLessThanFwd, ref List<Strike> strikesEqualToFwd, ref  List<Strike> strikesGreaterThanFwd)
        {
            var rawPrices = new List<Double>();
            rawPrices.AddRange(rawStrikePrices);
            rawPrices.Sort();
            int index = rawPrices.BinarySearch((double)forwardPrice);

            Boolean itemFound = false;
            if (index < 0)
            {
                index = ~index;
            }
            else
                itemFound = true;


            // entire list is out of the money
            if (index == 0 && !itemFound)
            {
                strikesGreaterThanFwd.AddRange(strikes);
            }
            // entire list is in the money
            else if (index == strikes.Count && !itemFound)
            {
                strikesLessThanFwd.AddRange(strikes);
            }
            else
            {
                if (itemFound)
                {
                    if (index == 0)
                    {
                        strikesEqualToFwd.AddRange(strikes.GetRange(index, 1));
                        strikesGreaterThanFwd.AddRange(strikes.GetRange(index + 1, strikes.Count - (index + 1)));
                    }
                    else if (index == strikes.Count - 1)
                    {
                        strikesLessThanFwd.AddRange(strikes.GetRange(0, index));
                        strikesEqualToFwd.AddRange(strikes.GetRange(index, 1));
                    }
                    else
                    {
                        strikesLessThanFwd.AddRange(strikes.GetRange(0, index));
                        strikesEqualToFwd.AddRange(strikes.GetRange(index, 1));
                        strikesGreaterThanFwd.AddRange(strikes.GetRange(index + 1, strikes.Count - (index + 1)));
                    }
                }
                else
                {
                    strikesLessThanFwd.AddRange(strikes.GetRange(0, index));
                    strikesGreaterThanFwd.AddRange(strikes.GetRange(index, strikes.Count - index));
                }
            }
            strikesLessThanFwd.Reverse();
        }


     
        /// <summary>
        /// Does the grouping.
        /// </summary>
        public static void DoGrouping(ForwardExpiry expiry)
        {
            var inTheMoney = new List<Strike>();
            var atTheMoney = new List<Strike>();
            var outTheMoney = new List<Strike>();
            GroupStrikesByMoneyness(expiry.FwdPrice, new List<Strike>(expiry.Strikes), expiry.RawStrikePrices, ref inTheMoney, ref atTheMoney, ref outTheMoney);
        }

        /// <summary>
        /// Returns a reference to the minimum proximity contract to lead expiry
        /// (contract expiries are not perfectly aligned) provided the date difference
        /// is within some specified number of days.
        /// </summary>
        /// <param name="fwdExpiry">The FWD expiry.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="proximityDays">The proximity days.</param>
        /// <returns></returns>
        public static ForwardExpiry FindExpiryClosestTo(ForwardExpiry fwdExpiry, VolatilitySurface volSurface, int proximityDays)
        {
            int lastProx = proximityDays;
            ForwardExpiry closestChildExpiry = null;
            foreach (ForwardExpiry childExpiry in volSurface.Expiries)
            {
                int prox = Math.Abs(childExpiry.ExpiryDate.Subtract(fwdExpiry.ExpiryDate).Days);

                if (prox <= proximityDays)
                {
                    lastProx = prox;
                    closestChildExpiry = childExpiry;
                }
            }
            return lastProx <= proximityDays ? closestChildExpiry : null;
        }

        /// <summary>
        /// Gets the longest expiry.
        /// </summary>
        /// <param name="fwdExpiries">The FWD expiries.</param>
        /// <returns></returns>
        public static ForwardExpiry GetLongestExpiry(ForwardExpiry[] fwdExpiries)
        {
            DateTime lastExpiryDate = DateTime.MinValue;
            ForwardExpiry lastExpiry = null;

            foreach (ForwardExpiry fwdExpiry in fwdExpiries)
            {
                if (fwdExpiry.ExpiryDate <= lastExpiryDate) continue;
                lastExpiryDate = fwdExpiry.ExpiryDate;
                lastExpiry  = fwdExpiry;
            }
            return lastExpiry;                      
        }


        internal static Pair<ForwardExpiry, ForwardExpiry> GetBoundingExpiries(ForwardExpiry[] expiries , DateTime expiry)
        {
            if (expiries.Length < 2) return new Pair<ForwardExpiry, ForwardExpiry>(expiries[0], expiries[1]);
            ForwardExpiry exp1;
            ForwardExpiry exp2;            
            if (expiry <= expiries[0].ExpiryDate)
            {
                exp1 = expiries[0];
                exp2 = exp1;
                return new Pair<ForwardExpiry, ForwardExpiry>(exp1, exp2);
            }
            for (int idx = 0; idx < expiries.Length - 1; idx++)
            {
                if (expiry > expiries[idx].ExpiryDate & expiry <= expiries[idx+1].ExpiryDate)
                {
                    exp1 = expiries[idx];
                    exp2 = exp1;
                    return new Pair<ForwardExpiry, ForwardExpiry>(exp1, exp2);
                }
            }
            exp1 = expiries[expiries.Length -1];
            exp2 = exp1;
            return new Pair<ForwardExpiry, ForwardExpiry>(exp1, exp2);

        }


        /// <summary>
        /// Clones the expiries.
        /// </summary>
        /// <param name="exps">The exps.</param>
        /// <param name="cloneexps"></param>
        /// <returns></returns>
        public static void CloneExpiries(List<ForwardExpiry> exps, List<ForwardExpiry> cloneexps)
        {
            cloneexps.AddRange(exps.Select(fwdExp => (ForwardExpiry) fwdExp.Clone()));
        }
    }
}
