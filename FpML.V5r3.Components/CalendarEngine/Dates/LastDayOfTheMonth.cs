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
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Dates
{
    ///<summary>
    ///</summary>
    public class LastDayOfTheMonth : LastTradingDate
    {
        /// <summary>
        /// Base constructor, which does not filter for main cycle or not.
        /// </summary>
        public LastDayOfTheMonth()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondFriday"/> class.
        /// </summary>
        /// <param name="exchangeCommodityCode">Name of the exchange commodity.</param>
        ///  <param name="expiryMonthAndYear">Name of the expiry Month And Year.</param>
        public LastDayOfTheMonth(string exchangeCommodityCode, string expiryMonthAndYear)
            : base(exchangeCommodityCode, expiryMonthAndYear)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SecondFriday"/> class.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity e.g. IBZ8.</param>
        public LastDayOfTheMonth(string exchangeCommodityName)
            : base(exchangeCommodityName)
        {}

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public override DateTime GetLastTradingDay(int month, int year)
        {
            //Rather than the last weekday, this use the last day for IB contracts
            //since the RBA dates are very close to the beginning of the month.
            DateTime significantDate = RuleHelper.LastDayInMonth(month, year);//LastWeekdayDayInMonth
            return significantDate;
        }

        /// <summary>
        /// whether or not the given date is an IMM date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="mainCycle"></param>
        /// <returns>true/false</returns>
        public override bool IsLastTradingDate(DateTime date, bool mainCycle)
        {
            if (!mainCycle)
            {
                var lastDay = GetLastTradingDay(date.Month, date.Year);
                return date < lastDay;
            }
            var nextDate = NextLastTradingDate(date, true);
            return date < nextDate;
        }

        /// <summary>
        /// next expiry date following the given date
        /// returns the 1st delivery date for next contract listed.
        /// </summary>
        /// <param name="refDate"></param>
        /// <param name="mainCycle"></param>
        /// <returns></returns>
        public override DateTime NextLastTradingDate(DateTime refDate, bool mainCycle) 
        {
            //int d = refDate.Day;
            int y = refDate.Year;
            int m = refDate.Month;
            if (!mainCycle)
            {
                return GetLastTradingDay(m, y);
            }
            int offset = 3;
            int skipMonths = offset-m%offset;
            if (skipMonths != offset) 
            {
                skipMonths += m;
                if (skipMonths<=12) 
                {
                    m = skipMonths;
                } 
                else 
                {
                    m = skipMonths-12;
                    y += 1;
                }
            }
            var result = GetLastTradingDay(m, y);
            return result;
        }

        /// <summary>
        /// IRH7,IRZ9 (90 day futures)
        /// 
        /// IBH8,IBU9 (30 day futures)
        /// </summary>
        /// <returns></returns>
        /// <param name="referenceDate">
        /// if 2000, IRZ8 is futures that expires in December 2008. 
        /// if 2010, IRZ8 is futures that expires in December 2018. 
        /// </param>
        public override DateTime GetLastTradingDay(DateTime referenceDate)
        {
            var month = (int)LastTradingDayHelper.ParseToCode(CodeAndExpiryMonth.ImmMonthCode);
            DateTime unadjustedExpirationDate;
            if (CodeAndExpiryMonth.Year < referenceDate.Year % 10)
            {
                int realYear = referenceDate.Year - referenceDate.Year % 10 + CodeAndExpiryMonth.Year + 10;
                unadjustedExpirationDate = RuleHelper.LastDayInMonth(month, realYear);
            }
            else
            {
                int realYear = referenceDate.Year - referenceDate.Year % 10 + CodeAndExpiryMonth.Year;
                unadjustedExpirationDate = RuleHelper.LastDayInMonth(month, realYear);
            }
            return unadjustedExpirationDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public override string GetNextAbsoluteMainCycleCode(DateTime referenceDate)
        {
            int referenceYear = referenceDate.Year;
            int referenceMonth = referenceDate.Month;
            string absoluteMonthCode = FuturesMainCycleCodes[referenceMonth - 1];
            //  check if adjustment required
            if (referenceMonth % 3 == 0)//check if need to advance to next code
            {
                DateTime unadjustedExpirationDate = RuleHelper.LastDayInMonth(referenceMonth, referenceYear);
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
                    absoluteMonthCode = FuturesMainCycleCodes[referenceMonth - 1];
                }
            }
            return $"{absoluteMonthCode}{referenceYear % 10}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public override string GetNextCode(DateTime referenceDate)
        {
            int referenceYear = referenceDate.Year;
            int referenceMonth = referenceDate.Month;
            string absoluteMonthCode = FuturesExpiryCodes[referenceMonth - 1];
            //  check if adjustment required (if it is already e.g. 20th of the March - the March futures has already expired)
            //
            var unadjustedExpirationDate = RuleHelper.LastDayInMonth(referenceMonth, referenceYear);
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
    }
}