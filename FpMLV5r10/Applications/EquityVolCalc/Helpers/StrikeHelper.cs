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
using FpML.V5r10.EquityVolatilityCalculator.Exception;
using Orion.Util.Helpers;

namespace FpML.V5r10.EquityVolatilityCalculator.Helpers
{
    /// <summary>
    /// Helper class for Strike
    /// </summary>
    public static class StrikeHelper
    { 
        /// <summary>
        /// Adds the strike.
        /// </summary>
        /// <param name="strike">The strike.</param>
        /// <param name="strikes">The strikes.</param>
        internal static void AddStrike(Strike strike, List<Strike> strikes)
        {
            Strike match = FindStrikeWithPrice(strike.StrikePrice, strikes, out _);
            if (match == null)
            {
                if (strikes.Count == 0)
                {
                    strikes.Add(strike);
                }
                else
                {
                    List<Strike> strikesFound = strikes.FindAll(
                        strikeItem => strikeItem.StrikePrice < strike.StrikePrice
                        );
                    strikes.Insert(strikesFound.Count == 0 ? 0 : strikesFound.Count, strike);
                }
            }
            else
            {
                throw new DuplicateNotAllowedException($"Strike with price {strike.StrikePrice} already exists");
            }
        }
      


        /// <summary>
        /// Finds the strike with price.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static Strike FindStrikeWithPrice(Double strikePrice, List<Strike> strikes, out int index)
        {
            Strike match = strikes.Find(
                strikeItem => strikeItem.StrikePrice == strikePrice
                );

            if (match == null)
            {
                index = -1;
            }
            else
            {
                index = strikes.IndexOf(match);
            }
            return match;
        }
                  

        internal static Pair<Strike,Strike> GetBoundingStrikes(ForwardExpiry expiry, double strike)
        {
            if (expiry.Strikes.Length < 2) return new Pair<Strike, Strike>(expiry.Strikes[0], expiry.Strikes[0]);
            Strike strike1;
            Strike strike2;         
            if (strike <= expiry.RawStrikePrices[0])
            {
                strike1 = expiry.Strikes[0];
                strike2 = strike1;
                return new Pair<Strike, Strike>(strike1, strike2);
            }           

            for ( int idx =0; idx < expiry.Strikes.Length-1 ; idx++)
            {
                if (strike > expiry.RawStrikePrices[idx] & strike <= expiry.RawStrikePrices[idx + 1])
                {
                    strike1 = expiry.Strikes[idx];
                    strike2 = expiry.Strikes[idx+1];
                    return new Pair<Strike, Strike>(strike1, strike2);
                }                    
            }
            strike1 = expiry.Strikes[expiry.Strikes.Length - 1];
            strike2 = strike1;
            return new Pair<Strike, Strike>(strike1, strike2);
        }
    }
}
