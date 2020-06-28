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

#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Highlander.CalendarEngine.V5r3.Dates
{
    /// <summary>
    /// The supported yearly BHK date cycles
    /// </summary>
    public enum CentralBankCycles
    {
        /// <summary>
        /// A monthly cycle
        /// </summary>
        Monthly,

        /// <summary>
        /// A quarterly cycle.
        /// </summary>
        Quarterly
    };

    /// <summary>
    /// The supported central banks.
    /// </summary>
    public enum CentralBanks
    {
        /// <summary>
        /// Reserve Bank of Australia
        /// </summary>
        RBA,

        /// <summary>
        /// Bank of Japan
        /// </summary>
        BOJ,

        /// <summary>
        /// Bank of Hong Kong
        /// </summary>
        BHK,

        /// <summary>
        /// Bank of New Zealand
        /// </summary>
        BNZ,

        /// <summary>
        /// Bank of England
        /// </summary>
        BOE,

        /// <summary>
        /// European Central Bank
        /// </summary>
        ECB,

        /// <summary>
        /// Federal Open Market Committee
        /// </summary>
        FOMC
    }

    /// <summary>
    /// Implements the central bank calendar interface
    /// </summary>
    public abstract class CentralBankDate: ICentralBankDate
    {
        #region Central Bank Members

        /// <summary>
        /// 
        /// </summary>
        public CentralBanks CentralBankName { get; protected set; }
        
        /// <summary>
        /// 
        /// </summary>
        public List<int> ValidMeetingMonths { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public List<int> ValidQuarterlyMonths { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CentralBankDate"/> class.
        /// </summary>
        /// <param name="centralBankName">The central Bank Name.</param>
        protected CentralBankDate(CentralBanks centralBankName)
        {
            CentralBankName = centralBankName;
            ValidMeetingMonths = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            ValidQuarterlyMonths = new List<int> { 3, 6, 9, 12 };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CentralBankDate"/> class.
        /// </summary>
        /// <param name="centralBankName">The central Bank Name.</param>
        /// <param name="validMeetingMonths">The valid Meeting Months.</param>
        /// <param name="validQuarterlyMonths">The valid quarterly Months.</param>
        protected CentralBankDate(CentralBanks centralBankName, List<int> validMeetingMonths, List<int> validQuarterlyMonths)
        {
            CentralBankName = centralBankName;
            ValidMeetingMonths = validMeetingMonths;
            ValidQuarterlyMonths = validQuarterlyMonths;
        }

        /// <summary>
        /// Gets the central bank days.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public List<DateTime> GetCentralBankDays(int year)
        {
            var date = DateRules(year);
            return date;
        }

        /// <summary>
        /// Gets the Central Bank day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public DateTime? GetCentralBankDay(int month, int year)
        {
            var date = DateRules(month, year);
            return date;
        }

        /// <summary>
        /// Gets the Central Bank days.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> GetCentralBankDays(DateTime startDate, DateTime endDate)
        {
            return GetCentralBankDays(startDate, endDate, true);
        }

        /// <summary>
        /// Gets the Central Bank days.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="monthlyCycle">if set to <c>true</c> [monthly cycle].</param>
        /// <returns></returns>
        public List<DateTime> GetCentralBankDays(DateTime startDate, DateTime endDate, Boolean monthlyCycle)
        {
            var allSignicantDates = GenerateDates(startDate, endDate, monthlyCycle);
            return allSignicantDates;
        }

        /// <summary>
        /// Returns the Central Bank days for a given year
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="monthlyCycle">if set to <c>true</c> [monthly cycle].</param>
        /// <returns></returns>
        public List<DateTime> DaysInYear(int year, Boolean monthlyCycle)
        {
            var years = new List<int> { year };
            return DaysInYears(years, monthlyCycle);
        }

        /// <summary>
        /// Returns the Central Bank days for a given set of years
        /// </summary>
        /// <param name="years">The years.</param>
        /// <returns></returns>
        public List<DateTime> DaysInYears(List<int> years)
        {
            return DaysInYears(years, true);
        }

        /// <summary>
        /// Returns the Central Bank days for a given set of years
        /// </summary>
        /// <param name="years">The years.</param>
        /// <param name="monthlyCycle">if set to <c>true</c> [monthly cycle].</param>
        /// <returns></returns>
        public List<DateTime> DaysInYears(List<int> years, Boolean monthlyCycle)
        {
            var dates = new List<DateTime>();
            if (years != null)
            {
                foreach (var year in years)
                {
                    if (monthlyCycle)
                    {
                        var cbYearDays = DayByMonths(ValidMeetingMonths, year);
                        dates.AddRange(cbYearDays);
                    }
                    else
                    {
                        var cbYearDays = DayByMonths(ValidQuarterlyMonths, year);
                        dates.AddRange(cbYearDays);
                    }
                }
            }
            dates.Sort();
            return dates;
        }

        /// <summary>
        /// Returns the central bank dates for a given set of months in a year
        /// </summary>
        /// <param name="months">The months.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public List<DateTime> DayByMonths(List<int> months, int year)
        {
            var dates = new List<DateTime>();
            foreach (var month in months)
            {
                var dt = GetCentralBankDay(month, year);
                if ((dt != null) && (!dates.Contains((DateTime)dt)))
                    dates.Add((DateTime)dt);
            }
            dates.Sort();
            return dates;
        }

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected abstract DateTime? DateRules(int month, int year);

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected abstract List<DateTime> DateRules(int year);

        /// <summary>
        /// Gets the central bank days.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="noOfMonths">The no of months.</param>
        /// <returns></returns>
        public List<DateTime> GetCentralBankDays(DateTime referenceDate, int noOfMonths)
        {
            var dtEnd = referenceDate.AddMonths(noOfMonths);
            return GenerateDates(referenceDate, dtEnd, true);
        }


        /// <summary>
        /// Gets the central bank days.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="noOfMonths">The no of months.</param>
        /// <param name="monthlyCycle">if set to <c>true</c> [monthly cycle].</param>
        /// <returns></returns>
        public List<DateTime> GetCentralBankDays(DateTime referenceDate, int noOfMonths, bool monthlyCycle)
        {
            var dtEnd = referenceDate.AddMonths(noOfMonths);
            return GenerateDates(referenceDate, dtEnd, monthlyCycle);
        }

        /// <summary>
        /// Yearses the between dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        protected static int YearsBetweenDates(DateTime startDate, DateTime endDate)
        {
            if ((endDate.Month > startDate.Month || endDate.Month == startDate.Month) && (endDate.Day < startDate.Day))
                return endDate.Year - startDate.Year - 1;
            return endDate.Year - startDate.Year;
        }

        /// <summary>
        /// Gets the dates in range.
        /// </summary>
        /// <param name="signicantDates">The signicant dates.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        protected static List<DateTime> GetDatesInRange(List<DateTime> signicantDates, DateTime startDate, DateTime endDate)
        {
            var signicantDatesInRange = new List<DateTime>();
            if (signicantDates.Count > 0)
            {
                signicantDates.Sort();
                var startIndex = IndexDateInList(signicantDates, startDate);
                var endIndex = IndexDateInList(signicantDates, endDate);

                var itemSpan = endIndex - startIndex;

                if (itemSpan == 0)
                {
                    if (signicantDates[startIndex] == startDate)
                        signicantDatesInRange = signicantDates.GetRange(startIndex, 1);
                }
                else
                {
                    var lastItemIndex = (endIndex < signicantDates.Count && signicantDates[endIndex] == endDate) ? itemSpan + 1 : itemSpan;
                    signicantDatesInRange = signicantDates.GetRange(startIndex, lastItemIndex);
                }
            }
            return signicantDatesInRange;
        }

        /// <summary>
        /// Indexes the date in list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        protected static int IndexDateInList(List<DateTime> list, DateTime date)
        {
            list.Sort();
            int index;
            if (list.Count == 0)
            {
                index = -1;
            }
            else
            {
                index = list.BinarySearch(date);
                if (index < 0)
                {
                    index = ~index;
                }
            }
            return index;
        }

        /// <summary>
        /// Generates the date vector for the year requested.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="monthlyCycle"></param>
        /// <returns></returns>
        protected List<DateTime> GenerateDates(DateTime startDate, DateTime endDate, Boolean monthlyCycle)
        {
            var result = new List<DateTime>();
            var years = new List<int>();
            var numYears =  YearsBetweenDates(startDate, endDate);
            years.Add(startDate.Year);
            if (numYears == 0) return GetDatesInRange(DaysInYears(years, monthlyCycle), startDate, endDate);
            for(int i = 1; i <= numYears; i++)
            {
                 years.Add(startDate.Year + i);
            }
            var dates = GetDatesInRange(DaysInYears(years, monthlyCycle), startDate, endDate) ;
            result.AddRange(dates);
            return result;
        }

        /// <summary>
        /// Returns whether the month is a valid month for that year.
        /// Currently the functionality is the same for each year.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns></returns>
        public bool IsValidMonth(int month)
        {
            if (ValidMeetingMonths.Contains(month)) return true;
            return false;
        }

        #endregion
    }
}