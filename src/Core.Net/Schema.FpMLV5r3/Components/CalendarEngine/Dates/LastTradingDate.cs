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
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Highlander.Reporting.Models.V5r3.Rates.Futures;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Dates;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Utilities.Helpers;

#endregion

namespace Highlander.CalendarEngine.V5r3.Dates
{
    /// <summary>
    /// Evaluates Last Trading Days
    /// </summary>
    public class LastTradingDate : ILastTradingDate
    {
        #region Properties

        protected const int YearsInDecade = 10;
        protected const int TradeMainCycleInterval =3;

        /// <summary>
        /// 
        /// </summary>
        protected static string[] FutureCodesPrefixes = { "ED", "ER", "RA", "BAX", "L", "ES", "EY", "HR", "IR", "IB", "W", "ICE_B" };

        protected static string[] FuturesExpiryCodes = {"F", "G", "H", "J", "K", "M", "N", "Q", "U", "V", "X", "Z"};

        protected static string[] FuturesMainCycleCodes = { "H", "H", "H", "M", "M", "M", "U", "U", "U", "Z", "Z", "Z" };

        /// <summary>
        /// 
        /// </summary>
        public struct FuturesPrefixImmMonthCodeAndYear
        {
            /// <summary>
            /// 
            /// </summary>
            public RateFutureAssetAnalyticModelIdentifier FuturesPrefix;

            /// <summary>
            /// 
            /// </summary>
            public readonly string ImmMonthCode;

            /// <summary>
            /// 
            /// </summary>
            public readonly int Year;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="futuresPrefix"></param>
            /// <param name="immMonthCode"></param>
            /// <param name="year"></param>
            public FuturesPrefixImmMonthCodeAndYear(string futuresPrefix, string immMonthCode, string year)
            {
                try
                {
                    FuturesPrefix = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(futuresPrefix);
                    ImmMonthCode = immMonthCode;
                    Year = int.Parse(year);
                }
                catch { throw new Exception("This is not a valid futures code!"); }
            }
        }

        /// <summary>
        /// The valid futures.
        /// </summary>
        public FuturesPrefixImmMonthCodeAndYear CodeAndExpiryMonth { get; protected set; }

        /// <summary>
        /// Returns the concatenated code for the contract.
        /// Need to check the L contract.
        /// </summary>
        /// <returns></returns>
        public string GetCode()
        {
            return CodeAndExpiryMonth.FuturesPrefix + CodeAndExpiryMonth.ImmMonthCode + CodeAndExpiryMonth.Year;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LastTradingDate"/> class.
        /// By default this is an IMM date.
        /// </summary>
        public LastTradingDate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LastTradingDate"/> class.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity e.g. EDZ8.</param>
        public LastTradingDate(string exchangeCommodityName)
        {
            CodeAndExpiryMonth = BreakCodeIntoPrefixAndYear(exchangeCommodityName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LastTradingDate"/> class.
        /// </summary>
        /// <param name="exchangeCommodityCode">Name of the exchange commodity.</param>
        /// <param name="expiryMonthAndYear">Name of the expiryMonthAndYear.</param>
        public LastTradingDate(string exchangeCommodityCode, string expiryMonthAndYear)
        {
            CodeAndExpiryMonth = new FuturesPrefixImmMonthCodeAndYear(exchangeCommodityCode, expiryMonthAndYear[0].ToString(CultureInfo.InvariantCulture), expiryMonthAndYear[1].ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region Interface Methods

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns></returns>
        public List<DateTime> GetLastTradingDays(int year, bool mainCycle)
        {
            var dts = new List<DateTime>();
            var cycle = TradeMainCycleInterval;
            if (!mainCycle) cycle = 1;
            for (int i = 1; i <= 12; i = i + cycle)
            {
                var dt = new DateTime(year, i, 1);
                dt = NextLastTradingDate(dt, mainCycle);
                dts.Add(dt);
            }
            return dts;
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public virtual DateTime GetLastTradingDay(int month, int year)
        {
            var significantDate = RuleHelper.GetNthDayInMonth(month, 3, 3, year, 0, 0);
            return significantDate;
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public virtual bool IsLastTradingDate(DateTime date, bool mainCycle)
        {
            if (date.DayOfWeek != DayOfWeek.Wednesday)
                return false;
            int d = date.Day;
            if (d < 15 || d > 21)
                return false;
            if (!mainCycle) return true;
            int m = date.Month;
            return (m == 3 || m == 6 || m == 9 || m == 12);
        }

        /// <summary>
        /// next IMM date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="refDate"></param>
        /// <param name="mainCycle"></param>
        /// <returns></returns>
        public virtual DateTime NextLastTradingDate(DateTime refDate, bool mainCycle)
        {
            int d = refDate.Day;
            int y = refDate.Year;
            int m = refDate.Month;
            int offset = mainCycle ? 3 : 1;
            int skipMonths = offset - (m % offset);
            if (skipMonths != offset || d > 21)
            {
                skipMonths += m;
                if (skipMonths <= 12)
                {
                    m = skipMonths;
                }
                else
                {
                    m = skipMonths - 12;
                    y += 1;
                }
            }
            DateTime result = GetLastTradingDay(m, y);
            if (result <= refDate)
                result = NextLastTradingDate(new DateTime(y, m, 22), mainCycle);
            return result;
        }

        /// <summary>
        /// 
        /// IRH7,IRZ9 (90 day futures)
        /// 
        /// IBH8,IBU9 (30 day futures)
        /// </summary>
        /// <returns></returns>
        /// <param name="referenceDate">
        /// if 2000, Z8 is futures that expires in December 2008. 
        /// if 2010, Z8 is futures that expires in December 2018. 
        /// </param>
        public virtual DateTime GetLastTradingDay(DateTime referenceDate)
        {
            var month = (int)LastTradingDayHelper.ParseToCode(CodeAndExpiryMonth.ImmMonthCode);
            //  Expiration - 2nd (2) Friday (4) of month.
            //
            DateTime unadjustedExpirationDate;
            if (CodeAndExpiryMonth.Year < referenceDate.Year % 10)
            {
                int realYear = referenceDate.Year - (referenceDate.Year % 10) + CodeAndExpiryMonth.Year + 10;
                unadjustedExpirationDate = RuleHelper.GetNthDayInMonth(month, 3, 3, realYear, 0, 0);
            }
            else
            {
                int realYear = referenceDate.Year - (referenceDate.Year % 10) + CodeAndExpiryMonth.Year;
                unadjustedExpirationDate = RuleHelper.GetNthDayInMonth(month, 3, 3, realYear, 0, 0);
            }
            return unadjustedExpirationDate;
        }

        #endregion

        #region LastTradingDayHelper Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="mainCycle">Is it a main cycle contract?</param>
        /// <returns>e.g "Z8"</returns>
        public string GetNextFuturesCode(DateTime referenceDate, bool mainCycle)
        {
            return mainCycle ? GetNextCode(referenceDate) : GetNextAbsoluteMainCycleCode(referenceDate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public virtual string GetNextCode(DateTime referenceDate)
        {
            int referenceYear = referenceDate.Year;
            int referenceMonth = referenceDate.Month;
            string absoluteMonthCode = FuturesExpiryCodes[referenceMonth - 1];
            //  check if adjustment required (if it is already e.g. 20th of the March - the March futures has already expired)
            //
            DateTime unadjustedExpirationDate = DateHelper.NthWeekday(3, 3, referenceMonth, referenceYear);
            if (referenceDate >= unadjustedExpirationDate)//if option expires today - move to next code
            {
                if (referenceMonth == 12)
                {
                    referenceMonth = 1;
                    ++referenceYear;
                }
                else
                {
                    ++referenceMonth;
                }
                absoluteMonthCode = FuturesExpiryCodes[referenceMonth];
            }
            return $"{absoluteMonthCode}{referenceYear % YearsInDecade}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public virtual string GetNextAbsoluteMainCycleCode(DateTime referenceDate)
        {
            int referenceYear = referenceDate.Year;
            int referenceMonth = referenceDate.Month;
            string absoluteMonthCode = FuturesMainCycleCodes[referenceMonth - 1];
            //  check if adjustment required (if it is already e.g. 20th of the March - the March futures has already expired)
            //
            if (referenceMonth % 3 == 0)//check if need to advance to next code
            {
                DateTime unadjustedExpirationDate = DateHelper.NthWeekday(3, 3, referenceMonth, referenceYear);
                if (referenceDate >= unadjustedExpirationDate)//if option expires today - move to next code
                {
                    if (referenceMonth == 12)
                    {
                        referenceMonth = 1;
                        ++referenceYear;
                    }
                    else
                    {
                        ++referenceMonth;
                    }
                    absoluteMonthCode = FuturesMainCycleCodes[referenceMonth];
                }
            }
            return $"{absoluteMonthCode}{referenceYear % YearsInDecade}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="iteration">e.g. 3 for the third next from the reference date.</param>
        /// <returns>e.g. H9</returns>
        public string GetNthCode(DateTime referenceDate, int iteration)
        {
            //Get the next valid code.
            string futuresCode = GetNextCode(referenceDate);
            //Iterate through the codes.
            for (int i = 1; i < iteration; i++)
            {
                futuresCode = GetFollowingCode(futuresCode);
            }
            return futuresCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absoluteCode">e.g. Z8</param>
        /// <returns>e.g. H9</returns>
        public static string GetFollowingCode(string absoluteCode)
        {
            if (2 != absoluteCode.Length)
            {
                string message =
                    $"{absoluteCode} is not recognised as absolute futures code. Examples are: 'H8', 'Z9', etc";
                throw new Exception(message);
            }
            string monthCode = absoluteCode[0].ToString(CultureInfo.InvariantCulture);
            int yearCode = int.Parse(absoluteCode[1].ToString(CultureInfo.InvariantCulture));
            if (FuturesExpiryCodes.Length - 1 == Array.IndexOf(FuturesExpiryCodes, monthCode))
            {
                ++yearCode;
                monthCode = FuturesExpiryCodes[0];
            }
            else
            {
                monthCode = FuturesExpiryCodes[Array.IndexOf(FuturesExpiryCodes, monthCode) + 1];
            }
            return $"{monthCode}{yearCode % YearsInDecade}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="iteration">e.g. 3 for the third next from the reference date.</param>
        /// <returns>e.g. H9</returns>
        public string GetNthMainCycleCode(DateTime referenceDate, int iteration)
        {
            //Get the next valid code.
            string futuresCode = GetNextAbsoluteMainCycleCode(referenceDate);
            //Iterate through the codes.
            for (int i = 1; i < iteration; i++)
            {
                futuresCode = GetFollowingMainCycleCode(futuresCode);
            }
            return futuresCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absoluteCode">e.g. Z8</param>
        /// <returns>e.g. H9</returns>
        public static string GetFollowingMainCycleCode(string absoluteCode)
        {
            if (2 != absoluteCode.Length)
            {
                string message =
                    $"{absoluteCode} is not recognised as absolute futures code. Examples are: 'H8', 'Z9', etc";
                throw new Exception(message);
            }
            string monthCode = absoluteCode[0].ToString(CultureInfo.InvariantCulture);
            int yearCode = int.Parse(absoluteCode[1].ToString(CultureInfo.InvariantCulture));
            //TODO get rid of this.
            var codesArray = new[] { "H", "M", "U", "Z" };
            if (codesArray.Length - 1 == Array.IndexOf(codesArray, monthCode))
            {
                ++yearCode;
                monthCode = codesArray[0];
            }
            else
            {
                monthCode = codesArray[Array.IndexOf(codesArray, monthCode) + 1];
            }
            return $"{monthCode}{yearCode % YearsInDecade}";
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the futures month.
        /// </summary>
        /// <param name="absoluteFuturesCode">The absolute Futures code.</param>
        /// <returns></returns>
        internal static int GetFuturesMonth(string absoluteFuturesCode)
        {
            if (!IsFuturesCode(absoluteFuturesCode, false))
                throw new ArgumentException("Invalid absolute IMMCode supplied.");
            string immPrefix = absoluteFuturesCode.Substring(0, 1);
            var immMonth = (FuturesCodesEnum)Enum.Parse(typeof(FuturesCodesEnum), immPrefix, true);
            return (int)immMonth;
        }

        /// <summary>
        /// Gets the IMM year.
        /// </summary>
        /// <param name="absoluteFuturesCode">The absolute Futures code.</param>
        /// <returns></returns>
        internal static int GetFuturesYear(string absoluteFuturesCode)
        {
            if (!IsFuturesCode(absoluteFuturesCode, false))
                throw new ArgumentException("Invalid absolute IMMCode supplied.");
            return int.Parse(absoluteFuturesCode.Substring(1, 1));
        }

        /// <summary>
        /// Determines whether [is decade start year] [the specified year].
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// 	<c>true</c> if [is decade start year] [the specified year]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDecadeStartYear(int year)
        {
            return year / YearsInDecade * YearsInDecade == year;
        }

        /// <summary>
        /// Finds the next year from the referenceDateYear with the requested last digit
        /// </summary>
        /// <param name="referenceDateYear"></param>
        /// <param name="yearLastDigit"></param>
        /// <returns></returns>
        public static int GetNextYear(int referenceDateYear, int yearLastDigit)
        {
            int year = (referenceDateYear - referenceDateYear % YearsInDecade) + yearLastDigit;
            if (year < referenceDateYear)
            {
                year += YearsInDecade;
            }
            return year;
        }

        /// <summary>
        /// Gets the last trade date.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <param name="significantDates">The significant dates.</param>
        /// <returns></returns>
        public static DateTime GetLastTradeDay(int month, int year, IEnumerable<DateTime> significantDates)
        {
            return significantDates.Single(a => a.Month == month && a.Year == year);
        }

        ///<summary>
        ///</summary>
        ///<param name="relativeCode"></param>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        public static bool IsRelativeCode(string relativeCode)
        {
            foreach (string futureCodesPrefix in FutureCodesPrefixes)
            {
                if (relativeCode.StartsWith(futureCodesPrefix))
                {
                    string futuresPosition = relativeCode.Substring(futureCodesPrefix.Length);
                    int dummy;
                    return int.TryParse(futuresPosition, out dummy);
                }
            }
            throw new Exception("Futures code prefix has not been identified as such.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeCode">
        /// ED01
        /// ED02
        /// ED03 
        /// etc
        /// </param>
        /// <param name="referenceDate"></param>
        /// <returns></returns>
        public string ConvertRelativeToAbsoluteCode(string relativeCode, DateTime referenceDate)
        {
            string subCode = relativeCode.Substring(2);
            string nextAbsoluteCode = GetNextAbsoluteMainCycleCode(referenceDate);
            int numberOfIterations = int.Parse(subCode);
            while (--numberOfIterations > 0)
            {
                nextAbsoluteCode = GetFollowingMainCycleCode(nextAbsoluteCode);
            }
            return CodeAndExpiryMonth.FuturesPrefix + nextAbsoluteCode;
        }

        /// <summary>
        /// whether or not the given string is an IMM code
        /// </summary>
        /// <param name="expiryCode"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public static bool IsFuturesCode(string expiryCode, bool mainCycle)
        {
            bool result = false;
            const string str2 = "0123456789";
            string str1 = mainCycle ? "hmzuHMZU" : "fghjkmnquvxzFGHJKMNQUVXZ";
            if (expiryCode.Length == 2)
                result = (str1.Contains(expiryCode.Substring(0, 1))) && (str2.Contains(expiryCode.Substring(1, 1)));
            return result;
        }


        ///<summary>
        ///</summary>
        ///<param name="relativeCode"></param>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        public static int GetFuturesPositionByCode(string relativeCode)
        {
            foreach (string futureCodesPrefix in FutureCodesPrefixes)
            {
                if (relativeCode.StartsWith(futureCodesPrefix))
                {
                    string futuresPosition = relativeCode.Substring(futureCodesPrefix.Length);
                    int futuresPositionAsInt = int.Parse(futuresPosition);
                    return futuresPositionAsInt;
                }
            }
            throw new Exception("Futures code prefix has not been identified as such.");
        }

        /// <summary>
        /// Breaks up the code.
        /// </summary>
        /// <param name="futuresCode"></param>
        /// <returns></returns>
        public static FuturesPrefixImmMonthCodeAndYear BreakCodeIntoPrefixAndYear(string futuresCode)
        {
            foreach (string futureCodesPrefix in FutureCodesPrefixes)
            {
                if (futuresCode.StartsWith(futureCodesPrefix))
                {
                    string immMonthCodeAndYear = futuresCode.Substring(futureCodesPrefix.Length);
                    var result = new FuturesPrefixImmMonthCodeAndYear(futureCodesPrefix, immMonthCodeAndYear[0].ToString(CultureInfo.InvariantCulture), immMonthCodeAndYear[1].ToString(CultureInfo.InvariantCulture));
                    return result;
                }
            }
            throw new Exception("Futures code prefix has not been identified as such.");
        }

        #endregion
    }
}