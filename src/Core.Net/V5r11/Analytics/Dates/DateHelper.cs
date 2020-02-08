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
using System.Linq;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Dates
{
    /// <summary>
    /// Month names for western calendars.
    /// </summary>
    /// <remarks>
    /// Use only for referring to a month in source code. Use the
    /// internationalization features to print out dates or write
    /// UI code.
    /// </remarks>
    public enum Month
    {
        /// <summary>January</summary>
        January = 1,
        /// <summary>February</summary>
        February = 2,
        /// <summary>March</summary>
        March = 3,
        /// <summary>April</summary>
        April = 4,
        /// <summary>May</summary>
        May = 5,
        /// <summary>June</summary>
        June = 6,
        /// <summary>July</summary>
        July = 7,
        /// <summary>August</summary>
        August = 8,
        /// <summary>September</summary>
        September = 9,
        /// <summary>October</summary>
        October = 10,
        /// <summary>November</summary>
        November = 11,
        /// <summary>December</summary>
        December = 12
    }

    /// <summary>
    /// Helper class for financial dates
    /// </summary>
    public static class DateHelper
    {
        /// <summary>
        /// n-th given weekday in the given month and year
        /// E.g., the 4th Thursday of March, 1998 was March 26th,
        /// 1998.
        /// see http://www.cpearson.com/excel/DateTimeWS.htm
        /// </summary>
        /// <param name="nth"></param>
        /// <param name="dayOfWeek"></param>
        /// <param name="m"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static DateTime NthWeekday(int nth, int dayOfWeek, int m, int y)//(0 < nth && nth > 6) 
        {
            DayOfWeek first = new DateTime(y, m, 1).DayOfWeek;
            int skip = nth - (dayOfWeek >= (int)first ? 1 : 0);
            return new DateTime(y, m, 1 + dayOfWeek - (int)first + skip * 7);
        }

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(int month, int year)
        {
            DateTime significantDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return significantDate;
        }

        public static DateTime NextCouponDate(DateTime settlement, DateTime maturity, int couponFreq)
        {
            DateTime nextCouponDate = maturity;
            int i = 0;
            while (nextCouponDate > settlement)
            {
                nextCouponDate = maturity.AddMonths(-12 / couponFreq * i);
                i++;
            }
            nextCouponDate = nextCouponDate.AddMonths(12 / couponFreq);
            return nextCouponDate;
        }

        public static DateTime LastCouponDate(DateTime settlement, DateTime maturity, int couponFreq)
        {
            DateTime nextCouponDate = NextCouponDate(settlement, maturity, couponFreq);
            DateTime lastCouponDate = nextCouponDate.AddMonths(-12 / couponFreq);
            return lastCouponDate;
        }

        public static DateTime DateRoll(DateTime startDate, int numRolls, string dwmy, string rollMethod, object[,] holidays)
        {
            DateTime endDate = startDate;
            int fpFlag = 0;
            if (rollMethod == "F" | rollMethod == "MF") fpFlag = 1;
            if (rollMethod == "P" | rollMethod == "MP") fpFlag = -1;
            if (dwmy == "D") endDate = startDate.AddDays(numRolls * fpFlag);
            if (dwmy == "W") endDate = startDate.AddDays(numRolls * 7 * fpFlag);
            if (dwmy == "M") endDate = startDate.AddMonths(numRolls * fpFlag);
            if (dwmy == "Y") endDate = startDate.AddYears(numRolls * fpFlag);
            int adjust = 0;
            if (rollMethod == "F" | rollMethod == "MF")
            {
                while (HolCheck(endDate.AddDays(adjust), holidays) | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Saturday
                            | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Sunday) adjust++;
                if (rollMethod == "MF" && endDate.AddDays(adjust).Month != endDate.Month)
                {
                    adjust = 0;
                    while (HolCheck(endDate.AddDays(adjust), holidays) | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Saturday
                            | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Sunday) adjust--;
                }
            }
            if (rollMethod == "P" | rollMethod == "MP")
            {
                while (HolCheck(endDate.AddDays(adjust), holidays) | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Saturday
                            | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Sunday) adjust--;
                if (rollMethod == "MP" && endDate.AddDays(adjust).Month != endDate.Month)
                {
                    adjust = 0;
                    while (HolCheck(endDate.AddDays(adjust), holidays) | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Saturday
                            | endDate.AddDays(adjust).DayOfWeek == DayOfWeek.Sunday) adjust++;
                }
            }
            endDate = endDate.AddDays(adjust);
            return endDate;
        }

        public static DateTime BizDayRoll(DateTime startDate, int numDays, object[,] holidays)
        {
            DateTime endDate = startDate;
            for (int i = 1; i <= numDays; i++)
            {
                endDate = DateRoll(endDate, 1, "D", "F", holidays);
            }
            return endDate;
        }

        public static bool HolCheck(DateTime date, object[,] holidays)
        {
            return holidays.Cast<DateTime>().Any(hol => date == hol);
        }

        public static DateTime[] SFEBillDates(DateTime expiryMonthYear, object[,] holidays)
        {
            var dates = new DateTime[3];
            dates[0] = new DateTime(expiryMonthYear.Year, expiryMonthYear.Month, 1);
            int count = 0;
            if (dates[0].DayOfWeek == DayOfWeek.Friday) count++;
            while (count < 2)
            {
                dates[0] = dates[0].AddDays(1);
                if (dates[0].DayOfWeek == DayOfWeek.Friday) count++;
            }
            dates[1] = DateRoll(dates[0], 1, "D", "P", holidays);
            dates[2] = DateRoll(dates[0], 7, "D", "P", holidays);
            return dates;
        }

        public static DateTime[] SFEBondDates(DateTime expiryMonthYear, object[,] holidays)
        {
            var dates = new DateTime[3];
            dates[0] = new DateTime(expiryMonthYear.Year, expiryMonthYear.Month, 15);
            dates[0] = DateRoll(dates[0], 0, "D", "F", holidays);
            dates[1] = dates[0];
            dates[2] = DateRoll(dates[0], 1, "D", "P", holidays);
            return dates;
        }

        public static DateTime[] CMEEuroDates(DateTime expiryMonthYear, object[,] holidays)
        {
            var dates = new DateTime[3];
            dates[0] = new DateTime(expiryMonthYear.Year, expiryMonthYear.Month, 1);
            int count = 0;
            if (dates[0].DayOfWeek == DayOfWeek.Wednesday) count++;
            while (count < 3)
            {
                dates[0] = dates[0].AddDays(1);
                if (dates[0].DayOfWeek == DayOfWeek.Wednesday) count++;
            }
            dates[1] = dates[0].AddDays(-2);
            dates[2] = dates[0].AddDays(-2);
            return dates;
        }

        public static DateTime[] NZBillDates(DateTime expiryMonthYear, object[,] holidays)
        {
            var dates = new DateTime[3];
            dates[0] = new DateTime(expiryMonthYear.Year, expiryMonthYear.Month, 9);
            int count = 0;
            if (dates[0].DayOfWeek == DayOfWeek.Wednesday) count++;
            while (count < 1)
            {
                dates[0] = dates[0].AddDays(1);
                if (dates[0].DayOfWeek == DayOfWeek.Wednesday) count++;
            }
            dates[0] = dates[0].AddDays(1);
            dates[1] = dates[0].AddDays(-1);
            dates[2] = dates[0];
            return dates;
        }

        public static DateTime[] CBOT5YrNoteDates(DateTime expiryMonthYear, object[,] holidays)
        {
            var dates = new DateTime[3];
            dates[1] = new DateTime(expiryMonthYear.Year, expiryMonthYear.Month, DateTime.DaysInMonth(expiryMonthYear.Year, expiryMonthYear.Month));
            dates[1] = DateRoll(dates[1], 0, "D", "P", holidays);
            dates[0] = dates[1];
            for (int i = 1; i <= 3; i++)
            {
                dates[0] = DateRoll(dates[0], 1, "D", "F", holidays);
            }
            dates[2] = dates[1].AddMonths(-1);
            dates[2] = new DateTime(dates[2].Year, dates[2].Month, DateTime.DaysInMonth(dates[2].Year, dates[2].Month));
            bool found = false;
            int count = 0;
            while (found == false)
            {
                count++;
                dates[2] = DateRoll(dates[2], 1, "D", "P", holidays);
                if (dates[2].DayOfWeek == DayOfWeek.Friday && count > 2) found = true;
            }
            return dates;
        }

        public static DateTime[] CBOT10YrNoteDates(DateTime expiryMonthYear, object[,] holidays)
        {
            var dates = new DateTime[3];
            dates[0] = new DateTime(expiryMonthYear.Year, expiryMonthYear.Month, DateTime.DaysInMonth(expiryMonthYear.Year, expiryMonthYear.Month));
            //Object[,] Holidays = new Object[1, 1];
            //Holidays[0, 0] = new DateTime(1, 1, 1);
            dates[0] = DateRoll(dates[0], 0, "D", "P", holidays);
            dates[1] = dates[0];
            for (int i = 1; i <= 7; i++)
            {
                dates[1] = DateRoll(dates[1], 1, "D", "P", holidays);
            }
            dates[2] = dates[0].AddMonths(-1);
            dates[2] = new DateTime(dates[2].Year, dates[2].Month, DateTime.DaysInMonth(dates[2].Year, dates[2].Month));
            bool found = false;
            int count = 0;
            while (found == false)
            {
                count++;
                dates[2] = DateRoll(dates[2], 1, "D", "P", holidays);
                if (dates[2].DayOfWeek == DayOfWeek.Friday && count > 2) found = true;
            }
            return dates;
        }

        // Always uses Monday-to-Sunday weeks
        public static DateTime GetStartOfWeek(DateTime input)
        {
            // Using +6 here leaves Monday as 0, Tuesday as 1 etc.
            int dayOfWeek = (((int)input.DayOfWeek) + 6) % 7;
            return input.Date.AddDays(-dayOfWeek);
        }

        public static int GetWeeks(DateTime start, DateTime end)
        {
            start = GetStartOfWeek(start);
            end = GetStartOfWeek(end);
            int days = (int)(end - start).TotalDays;
            return days / 7 + 1; // Adding 1 to be inclusive
        }
    }
}
