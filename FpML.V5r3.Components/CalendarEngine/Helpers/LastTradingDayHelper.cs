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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Models.Rates.Futures;
using Orion.Util.Helpers;
using Orion.Analytics.Helpers;
using Orion.CalendarEngine.Dates;

#endregion

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// Helper class for Last Trading Days
    /// </summary>
    public class LastTradingDayHelper
    {
        /// <summary>
        /// Gets the appropriate exchange calendar class for the contract defined.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W. The expiry must be concatenated. </param>
        /// <returns></returns>
        public static ILastTradingDate ParseCode(string futuresCode)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            return Parse(code);
        }
        /// <summary>
        /// Gets the appropriate exchange calendar class for the contract defined.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W. The expiry must be concatenated. </param>
        /// <returns></returns>
        public static ILastTradingDate Parse(RateFutureAssetAnalyticModelIdentifier futuresCode)
        {
            switch (futuresCode)
            {
                case RateFutureAssetAnalyticModelIdentifier.ED:
                case RateFutureAssetAnalyticModelIdentifier.ER:
                case RateFutureAssetAnalyticModelIdentifier.ES:
                case RateFutureAssetAnalyticModelIdentifier.EY:
                    return new SecondWednesdayPlusFive();
                case RateFutureAssetAnalyticModelIdentifier.W:
                case RateFutureAssetAnalyticModelIdentifier.CER:
                case RateFutureAssetAnalyticModelIdentifier.L:
                case RateFutureAssetAnalyticModelIdentifier.RA:
                case RateFutureAssetAnalyticModelIdentifier.LME:
                    return new LastTradingDate();               
                case RateFutureAssetAnalyticModelIdentifier.ZB:
                    return new FirstWednesdayOffsetNine();                        
                case RateFutureAssetAnalyticModelIdentifier.IR:
                    return new SecondFriday();
                case RateFutureAssetAnalyticModelIdentifier.BAX:
                    return new SecondWednesdayPlusFive();
                case RateFutureAssetAnalyticModelIdentifier.HR:
                    return new SecondTuesday();
                case RateFutureAssetAnalyticModelIdentifier.B:
                    return new FirstDayLessFifteen();
                case RateFutureAssetAnalyticModelIdentifier.IB:
                    return new LastDayOfTheMonth();
                default:
                    return new LastTradingDate();
            }
        }

        /// <summary>
        /// Gets the appropriate exchange calendar class for the contract defined.
        /// </summary>
        /// <param name="futuresCodeWithDelivery">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W. The expiry must be concatenated. </param>
        /// <returns></returns>
        public static ILastTradingDate Parse(string futuresCodeWithDelivery)
        {
            var futuresPrefixImmMonthCodeAndYear = LastTradingDate.BreakCodeIntoPrefixAndYear(futuresCodeWithDelivery);
            switch (futuresPrefixImmMonthCodeAndYear.FuturesPrefix)
            {
                case RateFutureAssetAnalyticModelIdentifier.ED:
                case RateFutureAssetAnalyticModelIdentifier.ER:
                case RateFutureAssetAnalyticModelIdentifier.ES:
                case RateFutureAssetAnalyticModelIdentifier.EY:
                    return new SecondWednesdayPlusFive(futuresCodeWithDelivery);
                case RateFutureAssetAnalyticModelIdentifier.W:
                case RateFutureAssetAnalyticModelIdentifier.CER:
                case RateFutureAssetAnalyticModelIdentifier.L:
                case RateFutureAssetAnalyticModelIdentifier.LME:
                case RateFutureAssetAnalyticModelIdentifier.RA:
                    return new LastTradingDate(futuresCodeWithDelivery);
                case RateFutureAssetAnalyticModelIdentifier.ZB:
                    return new FirstWednesdayOffsetNine(futuresCodeWithDelivery);                                
                case RateFutureAssetAnalyticModelIdentifier.IR:
                    return new SecondFriday(futuresCodeWithDelivery);
                case RateFutureAssetAnalyticModelIdentifier.BAX:
                    return new SecondWednesdayPlusFive(futuresCodeWithDelivery);
                case RateFutureAssetAnalyticModelIdentifier.HR:
                    return new SecondTuesday(futuresCodeWithDelivery);
                case RateFutureAssetAnalyticModelIdentifier.B:
                    return new FirstDayLessFifteen(futuresCodeWithDelivery);
                case RateFutureAssetAnalyticModelIdentifier.IB:
                    return new LastDayOfTheMonth(futuresCodeWithDelivery);
                default:
                    return new LastTradingDate();
            }
        }

        /// <summary>
        /// Gets the appropriate exchange calendar class for the contract defined.
        /// </summary>
        /// <param name="tradingName">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W </param>
        /// <param name="expiryCode">E.g. Z8 </param>
        /// <returns></returns>
        public static ILastTradingDate Parse(string tradingName, string expiryCode)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(tradingName);
            switch (code)
            {
                case RateFutureAssetAnalyticModelIdentifier.ED:
                case RateFutureAssetAnalyticModelIdentifier.ER:
                case RateFutureAssetAnalyticModelIdentifier.ES:
                case RateFutureAssetAnalyticModelIdentifier.EY:
                    return new SecondWednesdayPlusFive(tradingName, expiryCode);
                case RateFutureAssetAnalyticModelIdentifier.W:
                case RateFutureAssetAnalyticModelIdentifier.CER:
                case RateFutureAssetAnalyticModelIdentifier.L:
                case RateFutureAssetAnalyticModelIdentifier.LME:
                case RateFutureAssetAnalyticModelIdentifier.RA:
                    return new LastTradingDate(tradingName, expiryCode);
                case RateFutureAssetAnalyticModelIdentifier.ZB:
                    return new FirstWednesdayOffsetNine(tradingName, expiryCode);                                
                case RateFutureAssetAnalyticModelIdentifier.IR:
                    return new SecondFriday(tradingName, expiryCode);
                case RateFutureAssetAnalyticModelIdentifier.BAX:
                    return new SecondWednesdayPlusFive(tradingName, expiryCode);
                case RateFutureAssetAnalyticModelIdentifier.HR:
                    return new SecondTuesday(tradingName, expiryCode);
                case RateFutureAssetAnalyticModelIdentifier.B:
                    return new FirstDayLessFifteen(tradingName, expiryCode);
                case RateFutureAssetAnalyticModelIdentifier.IB:
                    return new LastDayOfTheMonth(tradingName, expiryCode);
                default:
                    return new LastTradingDate();
            }
        }

        /// <summary>
        /// returns the IMM code for next contract listed in the
        /// relevant Exchange.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="date">The date.</param>
        /// <param name="mainCycle">Is the contract a main cycle type?</param>
        /// <returns>The futures code string</returns>
        public static string GetNextFuturesCode(string futuresCode, DateTime date, bool mainCycle)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            var exchangeCal = Parse(code);
            var result = exchangeCal.NextFuturesCode(date, mainCycle);
            return result;
        }

        /// <summary>
        /// Gets the next valid futures code.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="referenceDate">The reference date to use.</param>
        /// <returns>e.g "Z8"</returns>
        public static string GetNextAbsoluteMainCycleCode(string futuresCode, DateTime referenceDate)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            var exchangeCal = Parse(code);
            var result = exchangeCal.GetNextAbsoluteMainCycleCode(referenceDate);
            return result;
        }

        /// <summary>
        /// next IMM date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="refDate">The reference date to use.</param>
        /// <param name="mainCycle">Is the contract a main cycle type?</param>
        /// <returns></returns>
        public static DateTime GetNextLastTradingDate(string futuresCode, DateTime refDate, bool mainCycle)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            var exchangeCal = Parse(code);
            var date = exchangeCal.NextLastTradingDate(refDate, mainCycle);
            return date;
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="date">THe date to verify.</param>
        /// <param name="mainCycle">Is the contract a main cycle type?</param>
        /// <returns>true/false</returns>
        public static bool IsLastTradingDate(string futuresCode, DateTime date, bool mainCycle)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            var exchangeCal = Parse(code);
            var result = exchangeCal.IsLastTradingDate(date, mainCycle);
            return result;
        }

        /// <summary>
        /// Gets the last trading dates for the contract and year specified.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="months">The months.</param>
        /// <param name="years">The years.</param>
        /// <returns></returns>
        public static List<DateTime> GetLastTradingDays(string futuresCode, int[] months, int[] years)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            var exchangeCal = Parse(code);
            var dates = new List<DateTime>();
            foreach(var year in years)
            {
                dates.AddRange(months.Select(month => exchangeCal.GetLastTradingDay(month, year)));
            }
            return dates;
        }

        /// <summary>
        /// Gets the last trading dates for the contract and year specified.
        /// </summary>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W.</param>
        /// <param name="years">The years.</param>
        /// <param name="mainCycle">The mainCycle flag.</param>
        /// <returns></returns>
        public static List<DateTime> GetLastTradingDays(string futuresCode, int[] years, bool mainCycle)
        {
            var code = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresCode);
            var exchangeCal = Parse(code);
            var dates = new List<DateTime>();
            foreach (var year in years)
            {
                dates.AddRange(exchangeCal.GetLastTradingDays(year, mainCycle));
            }
            return dates;
        }

        /// <summary>
        /// Gets the last trading date for the contract specified.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="futuresCode">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W. THe code must also have the expiry.</param>
        /// <returns></returns>
        public static DateTime GetLastTradingDay(DateTime referenceDate, string futuresCode)
        {
            var exchangeCal = Parse(futuresCode);
            return exchangeCal.GetLastTradingDay(referenceDate);
        }

        /// <summary>
        /// Gets the last trading date for the contract specified.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="tradingName">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W </param>
        /// <param name="expiryCode">E.g. Z8 </param>
        /// <returns></returns>
        public static DateTime GetLastTradingDay(DateTime referenceDate, string tradingName, string expiryCode)
        {
            var exchangeCal = Parse(tradingName, expiryCode);
            return exchangeCal.GetLastTradingDay(referenceDate);
        }

        /// <summary>
        /// Gets the last trading date for the contract specified.
        /// </summary>
        /// <param name="tradingName">Currently defined for:  ED, ER, RA, BAX, L, ES, EY, HR, IR, IB and W </param>
        /// <param name="expiryCode">E.g. Z8 </param>
        /// <returns></returns>
        public static DateTime GetLastTradingDay(string tradingName, string expiryCode)
        {
            return GetLastTradingDay(DateTime.Today, tradingName, expiryCode);
        }

        /// <summary>
        /// Parses a string representation to am imm month code,
        /// </summary>
        /// <param name="immMonth"></param>
        /// <returns>The imm month code.</returns>
        public static FuturesCodesEnum ParseToCode(string immMonth)
        {
            switch (immMonth)
            {
                case "F": return FuturesCodesEnum.F;
                case "G": return FuturesCodesEnum.G;
                case "H": return FuturesCodesEnum.H;
                case "J": return FuturesCodesEnum.J;
                case "K": return FuturesCodesEnum.K;
                case "M": return FuturesCodesEnum.M;
                case "N": return FuturesCodesEnum.N;
                case "Q": return FuturesCodesEnum.Q;
                case "U": return FuturesCodesEnum.U;
                case "V": return FuturesCodesEnum.V;
                case "X": return FuturesCodesEnum.X;
                case "Z": return FuturesCodesEnum.Z;
                default: return FuturesCodesEnum.Unknown;
            }
        }
    }
}