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

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// Helper class used for interpreting date rules
    /// </summary>
    public static class RuleHelper
    {
        /// <summary>
        /// Determines whether [is rule effective] [the specified dr].
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="dtCheckDate">The dt check date.</param>
        /// <returns>
        /// 	<c>true</c> if [is rule effective] [the specified dr]; otherwise, <c>false</c>.
        /// </returns>
        internal static Boolean IsRuleEffective(DateRule dr, DateTime dtCheckDate)
        {
            DateTime dtStart = dr.startDate;
            DateTime dtEnd = dr.endDate;
            Boolean bIsValidRulePeriod = true;
            if (dtStart > DateTime.MinValue && dtEnd > DateTime.MinValue)
            {
                if (dtCheckDate.Year < dtStart.Year || dtCheckDate.Year > dtEnd.Year)
                    bIsValidRulePeriod = false;
            }
            else if (dtStart > DateTime.MinValue)
            {
                if (dtCheckDate.Year < dtStart.Year)
                    bIsValidRulePeriod = false;
            }
            else if (dtEnd > DateTime.MinValue)
            {
                if (dtCheckDate.Year > dtEnd.Year)
                    bIsValidRulePeriod = false;
            }

            return bIsValidRulePeriod;
        }

        /// <summary>
        /// Applies the offset for exclusive dates.
        /// </summary>
        /// <param name="dtFrom">The dt from.</param>
        /// <param name="dtTo">The dt to.</param>
        internal static void ApplyOffsetForExclusiveDates(ref DateTime dtFrom, ref DateTime dtTo)
        {
            if (DateTime.Compare(dtFrom, dtTo) > 0)
            {
                dtFrom = dtFrom.AddDays(-1);
                dtTo = dtTo.AddDays(1);
            }
            else if (DateTime.Compare(dtFrom, dtTo) < 0)
            {
                dtFrom = dtFrom.AddDays(1);
                dtTo = dtTo.AddDays(-1);
            }
        }

        /// <summary>
        /// Applies the date rule roll convention.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="dtBase">The dt base.</param>
        /// <returns></returns>
        internal static DateTime ApplyDateRuleRollConvention(DateRule dr, DateTime dtBase)
        {
            DateTime dtRolledDate = dtBase;
            if ((dr.applyRollSpecified) && (dr.applyRoll))
                dtRolledDate = RollWeekendDateFwdToMonday(dtBase);
            // Here we roll to before the weekend (i.e Friday)
            else if ((dr.applyRollPriorSpecified) && (dr.applyRollPrior))
            {
                dtRolledDate = RollWeekendDateBackToFriday(dtBase);
            }
            // Here we would roll fwd to monday if a Sunday and rollback to Friday if a Saturday
            else if ((dr.rollBeforeAfterWeekendSpecified) && (dr.rollBeforeAfterWeekend))
            {
                dtRolledDate = RollWeekendDateBeforeAfterWeekend(dtBase);
            }
            // Here we roll to before the weekend (i.e Friday) ONLY if its a Saturday
            else if ((dr.RollPriorIfSaturdaySpecified) && (dr.RollPriorIfSaturday))
            {
                dtRolledDate = RollBeforeWeekendIfSaturday(dtBase);
            }
            return dtRolledDate;
        }

        /// <summary>
        /// Gets the date by month week weekday.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="week">The week.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        internal static DateTime GetDateByMonthWeekWeekday(int month, int week, int dayOfWeek, int year)
        {
            const int cNofDaysInWeek = 7;
            var firstDayOfMonth = new DateTime(year, month, 1);
            DateTime nthInstanceOfDay = firstDayOfMonth;
            if (week > 1)
            {
                int noOfDaysToAdd = (week * cNofDaysInWeek) - cNofDaysInWeek;
                nthInstanceOfDay = nthInstanceOfDay.AddDays(noOfDaysToAdd);
            }
            int dayIncrement = 1;
            if ((int)nthInstanceOfDay.DayOfWeek > dayOfWeek)
            {
                dayIncrement = dayIncrement * -1;
            }
            while ((int)nthInstanceOfDay.DayOfWeek != dayOfWeek)
            {
                nthInstanceOfDay = nthInstanceOfDay.AddDays(dayIncrement);
            }
            DateTime result = nthInstanceOfDay;
            return result;
        }


        /// <summary>
        /// Gets the NTH day in month.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="nthInstance">The NTH instance.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="year">The year.</param>
        /// <param name="offsetBefore">The day of month to start on (default 0)</param>
        /// <param name="offsetAfter">Number of days to move on after (default 0)</param>
        /// <returns></returns>
        internal static DateTime GetNthDayInMonth(int month, int nthInstance, int dayOfWeek, int year, int offsetBefore, int offsetAfter)
        {
            const int daysInWeek = 7;
            if (nthInstance < 1 || nthInstance > 4)
                throw new OverflowException("Invalid instance specified. Must be in range 1-4");
            var workDate = new DateTime(year, month, 1 + offsetBefore);
            while ((int)workDate.DayOfWeek != dayOfWeek)
            {
                workDate = workDate.AddDays(1);
            }
            DateTime firstInstanceOfDay = workDate;
            DateTime nthInstanceOfDay = firstInstanceOfDay;
            if (nthInstance > 1)
            {
                int noOfDaysToAdd = (nthInstance * daysInWeek) - daysInWeek;
                nthInstanceOfDay = nthInstanceOfDay.AddDays(noOfDaysToAdd + offsetAfter);
            }
            DateTime result = nthInstanceOfDay;
           
            return result;
        }

        /// <summary>
        /// Lasts the weekday day in month.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        internal static DateTime LastWeekdayDayInMonth(int dayOfWeek, int month, int year)
        {
            var dtLastDayInMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var dtLastWeekdayDayInMonth = dtLastDayInMonth;
            while ((int)dtLastWeekdayDayInMonth.DayOfWeek != dayOfWeek)
            {
                dtLastWeekdayDayInMonth = dtLastWeekdayDayInMonth.AddDays(-1);
            }
            return dtLastWeekdayDayInMonth;
        }

        /// <summary>
        /// Returns the first day which matches the supplied day of the week between the respective from and to dates in a given month
        /// </summary>
        /// <param name="dtFrom">The dt from.</param>
        /// <param name="dtTo">The dt to.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="bDayOfWeekFound">if set to <c>true</c> [b day of week found].</param>
        /// <returns></returns>
        public static DateTime FirstDayOfWeekBetween(DateTime dtFrom, DateTime dtTo, DayOfWeek dayOfWeek, ref Boolean bDayOfWeekFound)
        {
            DateTime dtWorkDate = dtFrom;
            DateTime dtEndDate = dtTo;
            int increment = ((dtFrom < dtTo) ? 1 : -1) * 1;
            while (Math.Abs(DateTime.Compare(dtWorkDate, dtEndDate)) != 0)
            {
                if (dtWorkDate.DayOfWeek != dayOfWeek)
                    dtWorkDate = dtWorkDate.AddDays(increment);
                else
                {
                    bDayOfWeekFound = true;
                    break;
                }
            }
            if (dtWorkDate.DayOfWeek == dayOfWeek)
                bDayOfWeekFound = true;
            return dtWorkDate;
        }

        /// <summary>
        /// Firsts the weekday between.
        /// </summary>
        /// <param name="dtFrom">The dt from.</param>
        /// <param name="dtTo">The dt to.</param>
        /// <param name="bWeekdayFound">if set to <c>true</c> [b weekday found].</param>
        /// <returns></returns>
        public static DateTime FirstWeekdayBetween(DateTime dtFrom, DateTime dtTo, ref Boolean bWeekdayFound)
        {
            DateTime dtWorkDate = dtFrom;
            DateTime dtEndDate = dtTo;
            int increment = ((dtFrom < dtTo) ? 1 : -1) * 1;
            while (Math.Abs(DateTime.Compare(dtWorkDate, dtEndDate)) != 0)
            {
                if (IsWeekend(dtWorkDate))
                    dtWorkDate = dtWorkDate.AddDays(increment);
                else
                {
                    bWeekdayFound = true;
                    break;
                }
            }
            return dtWorkDate;
        }

        /// <summary>
        /// Determines whether the specified date is weekend.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date is weekend; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsWeekend(DateTime date)
        {
            return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }

        /// <summary>
        /// Rolls the weekend date FWD to monday.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime RollWeekendDateFwdToMonday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ? date.AddDays(2) : date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(1) : date;
        }

        /// <summary>
        /// Rolls the weekend date before after weekend. It rolls to previous Friday if the supplied date falls on a Saturday or 
        /// rolls to the Following Monday if the date falls on a Sunday
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static DateTime RollWeekendDateBeforeAfterWeekend(DateTime dt)
        {
            DateTime dtRollTo = dt;
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    dtRollTo = RollWeekendDateBackToFriday(dt);
                    break;
                case DayOfWeek.Sunday:
                    dtRollTo = RollWeekendDateFwdToMonday(dt);
                    break;
            }
            return dtRollTo;
        }


        /// <summary>
        /// Rolls the before weekend if saturday.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static DateTime RollBeforeWeekendIfSaturday(DateTime dt)
        {
            DateTime dtRollTo = dt;
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    dtRollTo = RollWeekendDateBackToFriday(dt);
                    break;
            }
            return dtRollTo;
        }

        /// <summary>
        /// Rolls the weekend date back to friday.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime RollWeekendDateBackToFriday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ? date.AddDays(-1) : date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-2) : date;
        }

        /// <summary>
        /// Processes the observed significant day date.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="noOfDaysAfter">The no of days after.</param>
        /// <param name="baseDate">The base date.</param>
        /// <returns></returns>
        internal static DateTime ProcessObservedSignificantDayDate(DateRule dr, int noOfDaysAfter, DateTime baseDate)
        {
            DateTime dtDependentObsHoliday = baseDate.AddDays(noOfDaysAfter);
            if (IsWeekend(dtDependentObsHoliday))
            {
                dtDependentObsHoliday = ApplyDateRuleRollConvention(dr, dtDependentObsHoliday);
            }
            return dtDependentObsHoliday;
        }

        /// <summary>
        /// Applies the day in month rule.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="dim">The dim.</param>
        /// <param name="dtCheckDate">The dt check date.</param>
        /// <param name="dtBase">The dt base.</param>
        /// <param name="dtObservedDayDate">The dt observed day date.</param>
        internal static void ApplyDayInMonthRule(DateRule dr, DayInMonth dim, DateTime dtCheckDate, ref DateTime dtBase, ref DateTime dtObservedDayDate)
        {
            // if we have a base holiday where the month and day have been specified
            if (dim.Month > 0 && dim.Day > 0)
            {
                dtBase = new DateTime(dtCheckDate.Year, dim.Month, dim.Day);
                ////Those that occur on a specified date but only on certain years
                //// NOTE**:These attributes don't yet exist but can easily be added to the XSD
                //if (hr.EveryXYearsSpecified)
                //{
                //    if (((dt.Year - hr.StartYear) % hr.EveryXYears) == 0)
                //    {
                //        h.Date = dt;
                //    }
                //}
            }
            // Derive and set business holiday (not necessarily the same as the holiday particularly if we are rolling
            // if the date falls on a weekend. 
            // Here we would roll fwd to monday
            dtObservedDayDate = ApplyDateRuleRollConvention(dr, dtBase);
        }

        /// <summary>
        /// Evals the static method.
        /// </summary>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="staticMethodName">Name of the static method.</param>
        /// <param name="parameterObject">The parameter object.</param>
        /// <returns></returns>
        internal static SignificantDay[] EvalStaticMethod(string nameSpace, string className, string staticMethodName, object[] parameterObject)
        {
            SignificantDay[] sds;
            var assembly = Assembly.GetExecutingAssembly();
            Type myClass = assembly.GetType($"{nameSpace}.{className}");
            if (myClass == null)
            {
                throw new InvalidOperationException($"Class {nameSpace}.{className} could not be loaded.");
            }
            object dteval = myClass.InvokeMember(staticMethodName, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, null, parameterObject);
            var sd = new SignificantDay();
            if (dteval is DateTime)
            {
                sd.Date = (DateTime)dteval;
                sds = new[] { sd };
            }
            else
            {
                if (dteval is SignificantDay day)
                {
                    sds = new[] { day };
                }
                else
                {
                    sds = (SignificantDay[])dteval;
                }
            }
            return sds;
        }


        /// <summary>
        /// Evals the static method.
        /// </summary>
        /// <param name="dateRule">The date rule.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="staticMethodName">Name of the static method.</param>
        /// <param name="parameterObject">The parameter object.</param>
        /// <returns></returns>
        internal static SignificantDay[] EvalStaticMethod(DateRule dateRule, string assemblyName, string nameSpace, string className, string staticMethodName, object[] parameterObject)
        {
            SignificantDay[] sds;
            Assembly assembly = Assembly.Load(assemblyName);
            Type myClass = assembly.GetType($"{nameSpace}.{className}");
            object dteval = myClass.InvokeMember(staticMethodName, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, null, parameterObject);           
            var sd = new SignificantDay();
            if (dteval is DateTime)
            {
                sd.Date = (DateTime)dteval;
                sds = new[] { sd };
            }
            else
            {
                if (dteval is SignificantDay day)
                {
                    sds = new[] { day };
                }
                else
                {
                    sds = (SignificantDay[])dteval; 
                }
            }
            return sds;
        }

        /// <summary>
        /// Easters the specified year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="ruleProfileName">Name of the rule profile.</param>
        /// <returns></returns>
        public static DateTime Easter(int year, string ruleProfileName)
        {
            int y = year;
            int a = y % 19;
            int b = y / 100;
            int c = y % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int easterMonth = (h + l - 7 * m + 114) / 31;
            int p = (h + l - 7 * m + 114) % 31;
            int easterDay = p + 1;
            var est = new DateTime(y, easterMonth, easterDay);
            return est;
        }

        /// <summary>
        /// Calculate the Vernal Equinox for a given year
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="ruleProfileName">Name of the rule profile.</param>
        /// <returns></returns>
        public static DateTime VernalEquinox(int year, string ruleProfileName)
        {
            // This patch calculates astronomical equinox date, which is a good
            // estimate of official equinox date.
            // (by KAWANISHI Tomoya <mailto:tomoya@mm.media.kyoto-u.ac.jp>)
            const double exactVernalEquinoxTime = 20.69115; // at 2000
            const double diffPerYear = 0.242194;
            const int month = 3;
            int y = year;
            double movingAmount = (y - 2000) * diffPerYear;
            int numberOfLeapYear = (y - 2000) / 4 + (y - 2000) / 100 - (y - 2000) / 400;
            //  vernal_equinox_day 
            var day = (int)(exactVernalEquinoxTime + movingAmount - numberOfLeapYear);
            return new DateTime(year, month, day);
        }


        /// <summary>
        /// Calculate the Autumnal Equinox for a given year
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="ruleProfileName">Name of the rule profile.</param>
        /// <returns></returns>
        public static DateTime AutumnalEquinox(int year, string ruleProfileName)
        {
            // This patch calculates astronomical equinox date, which is a good
            // estimate of official equinox date.
            // (by KAWANISHI Tomoya <mailto:tomoya@mm.media.kyoto-u.ac.jp>)
            const double exactAutumnalEquinoxTime = 23.09;
            const double diffPerYear = 0.242194;
            const int month = 9;
            int y = year;
            double movingAmount = (y - 2000) * diffPerYear;
            int numberOfLeapYear = (y - 2000) / 4 + (y - 2000) / 100 - (y - 2000) / 400;
            //  Autumnal Equinox Day 
            var day = (int)(exactAutumnalEquinoxTime + movingAmount - numberOfLeapYear);
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// De-dupes a given set of dates.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <returns></returns>
        public static DateTime[] DeDupeDates(DateTime[] dates)
        {
            var datesList = new List<DateTime>();
            foreach (DateTime date in dates)
            {
                if (!datesList.Contains(date))
                    datesList.Add(date);
            }
            if (datesList.Count > 0)
            {
                datesList.Sort();
                dates = new DateTime[datesList.Count];
                datesList.CopyTo(dates, 0);
            }
            return dates;
        }
    }
}