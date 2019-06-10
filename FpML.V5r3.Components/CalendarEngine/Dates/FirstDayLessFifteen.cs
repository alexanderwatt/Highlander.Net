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

#region Using Directives

using System;
using System.Collections.Generic;
using Orion.Analytics.Dates;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Dates
{
    ///<summary>
    ///</summary>
    public class FirstDayLessFifteen : LastTradingDate
    {
        /// <summary>
        /// Base constructor, which does not filter for main cycle or not.
        /// </summary>
        public FirstDayLessFifteen()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstDayLessFifteen"/> class.
        /// </summary>
        /// <param name="exchangeCommodityCode">Name of the exchange commodity.</param>
        ///  <param name="expiryMonthAndYear">Name of the expiryMonthAndYeary.</param>
        public FirstDayLessFifteen(string exchangeCommodityCode, string expiryMonthAndYear)
            : base(exchangeCommodityCode, expiryMonthAndYear)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstDayLessFifteen"/> class.
        /// </summary>
        /// <param name="exchangeCommodityName">Name of the exchange commodity e.g. EDZ8.</param>
        public FirstDayLessFifteen(string exchangeCommodityName)
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
            DateTime significantDate = new DateTime(year, month, 1).AddDays(-15);
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
            //if (date.DayOfWeek != DayOfWeek.Friday)
            //    return false;
            var d = date.Day;
            if (d<8 || d>15)
                return false;
            if (!mainCycle) return true;
            int m = date.Month;
            return (m == 3 || m == 6 || m == 9 || m == 12);
        }

        /// <summary>
        /// next ICE date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// International Money Market section of the Chicago Mercantile
        /// Exchange.
        /// </summary>
        /// <param name="refDate"></param>
        /// <param name="mainCycle"></param>
        /// <returns></returns>
        public override DateTime NextLastTradingDate(DateTime refDate, bool mainCycle) 
        {
            int d = refDate.Day;
            int y = refDate.Year;
            int m = refDate.Month;
            int offset = mainCycle ? 3 : 1;
            int skipMonths = offset-(m%offset);
            if (skipMonths != offset || d > 20) 
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
            DateTime result = GetLastTradingDay(m, y);
            if (result<=refDate)
                result = NextLastTradingDate(new DateTime(y, m, 15), mainCycle);
            return result;
        }

        /// <summary>
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
                int realYear = referenceDate.Year - (referenceDate.Year % 10) + CodeAndExpiryMonth.Year + 10;
                unadjustedExpirationDate = new DateTime(realYear, month, 1).AddDays(-15);
            }
            else
            {
                int realYear = referenceDate.Year - (referenceDate.Year % 10) + CodeAndExpiryMonth.Year;
                unadjustedExpirationDate = new DateTime(realYear, month, 1).AddDays(-15); 
            }
            return unadjustedExpirationDate;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <param name="referenceDate">
        /// </param>
        /// <param name="cycle">The ordered number of monthly contracts.</param>
        public DateTime GetNthLastTradingDay(DateTime referenceDate, int cycle)
        {
            var tDate = referenceDate.AddMonths(cycle);
            var result = new DateTime(tDate.Year, tDate.Month, 1).AddDays(-15);
            if (referenceDate >= result)
            {
                var nDate = referenceDate.AddMonths(cycle + 1);
                result = new DateTime(nDate.Year, nDate.Month, 1).AddDays(-15);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        public override string GetNextAbsoluteMainCycleCode(DateTime referenceDate)
        {
            int referenceYear = referenceDate.Year;
            var table = new Dictionary<int, string>
                            {
                                {1, "H"},
                                {2, "H"},
                                {3, "H"},
                                {4, "M"},
                                {5, "M"},
                                {6, "M"},
                                {7, "U"},
                                {8, "U"},
                                {9, "U"},
                                {10, "Z"},
                                {11, "Z"},
                                {12, "Z"}
                            };
            int referenceMonth = referenceDate.Month;
            string absoluteMonthCode = table[referenceMonth];
            //  check if adjustment required (if it is already e.g. 20th of the March - the March futures has already expired)
            //
            if (referenceMonth % 3 == 0)//check if need to advance to next code
            {
                DateTime unadjustedExpirationDate = DateHelper.NthWeekday(2, 5, referenceMonth, referenceYear);
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
                    absoluteMonthCode = table[referenceMonth];
                }
            }
            return $"{absoluteMonthCode}{referenceYear % 10}";
        }
    }
}