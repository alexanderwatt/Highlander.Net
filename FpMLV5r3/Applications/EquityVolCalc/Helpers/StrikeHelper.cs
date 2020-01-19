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
using System.Collections.Generic;
using Highlander.Utilities.Exception;
using Highlander.Utilities.Helpers;

namespace Highlander.Equity.Calculator.V5r3.Helpers
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
        internal static void AddStrike(EquityStrike strike, List<EquityStrike> strikes)
        {
            EquityStrike match = FindStrikeWithPrice(strike.StrikePrice, strikes, out int _);
            if (match == null)
            {
                if (strikes.Count == 0)
                {
                    strikes.Add(strike);
                }
                else
                {
                    List<EquityStrike> strikesFound = strikes.FindAll(
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
        public static EquityStrike FindStrikeWithPrice(Double strikePrice, List<EquityStrike> strikes, out int index)
        {
            EquityStrike match = strikes.Find(
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
                  
        internal static Pair<EquityStrike, EquityStrike> GetBoundingStrikes(ForwardExpiry expiry, double strike)
        {
            if (expiry.Strikes.Length < 2) return new Pair<EquityStrike, EquityStrike>(expiry.Strikes[0], expiry.Strikes[0]);
            EquityStrike strike1;
            EquityStrike strike2;         
            if (strike <= expiry.RawStrikePrices[0])
            {
                strike1 = expiry.Strikes[0];
                strike2 = strike1;
                return new Pair<EquityStrike, EquityStrike>(strike1, strike2);
            }           
            for ( int idx =0; idx < expiry.Strikes.Length-1 ; idx++)
            {
                if (strike > expiry.RawStrikePrices[idx] & strike <= expiry.RawStrikePrices[idx + 1])
                {
                    strike1 = expiry.Strikes[idx];
                    strike2 = expiry.Strikes[idx+1];
                    return new Pair<EquityStrike, EquityStrike>(strike1, strike2);
                }                    
            }
            strike1 = expiry.Strikes[expiry.Strikes.Length - 1];
            strike2 = strike1;
            return new Pair<EquityStrike, EquityStrike>(strike1, strike2);
        }
    }
}
