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

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Rules;

#endregion

namespace Orion.CalendarEngine
{
    /// <summary>
    /// THe base calendar.
    /// </summary>
    public abstract class BaseCalendar : IBusinessCalendar
    {
        #region Private fields

        /// <summary>
        /// static representation of the holiday rules
        /// </summary>
        public List<SignificantDay> SignificantDates { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> CalendarNames { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public string CalendarNamesList { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IDateRuleParser DateRuleParser { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Name(s) of the Date Rule
        /// </summary>
        public List<DateTime> Dates
        {
            get
            {
                var dateList = new List<DateTime>();
                var sigDates = ConvertDates(SignificantDates);
                dateList.AddRange(sigDates.Distinct());
                return dateList;
            }
        }

        /// <summary>
        /// The underlying <see cref="System.Globalization.Calendar"/>.
        /// </summary>
        public Calendar Calendar => DateRuleParser.Calendar[0];

        /// <summary>
        /// The associated <see cref="System.Globalization.CultureInfo"/>.
        /// </summary>
        public CultureInfo Culture => DateRuleParser.Culture[0];

        /// <summary>
        /// The associated <see cref="System.Globalization.RegionInfo"/>.
        /// </summary>
        public RegionInfo Region => new RegionInfo(DateRuleParser.Culture[0].LCID);

        /// <summary>
        /// Name(s) of the calendar(s)
        /// </summary>
        public string NameList
        {
            get
            {
                if (CalendarNamesList.Length == 0)
                {
                    string[] distinctOrderedRules = DateRuleParser.Name.Distinct().OrderBy(a => a).ToArray();
                    CalendarNamesList = string.Join(" ", distinctOrderedRules);
                }
                return CalendarNamesList.Trim();
            }
        }

        /// <summary>
        /// Gets the deduped name list.
        /// </summary>
        /// <value>The deduped name list.</value>
        public string DedupedNameList
        {
            get
            {
                string[] distinctOrderedRules = DateRuleParser.Name.Distinct().OrderBy(a => a).ToArray();
                return string.Join(" ", distinctOrderedRules).Trim();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// For use with non business center calendars.
        /// </summary>
        protected BaseCalendar()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCalendar"/> class.
        /// </summary>
        /// <param name="significantDates">The significant Dates.</param>
        /// <param name="dateRuleParser">The date Rule Parser.</param>
        protected BaseCalendar(List<SignificantDay> significantDates, IDateRuleParser dateRuleParser)
        {
            DateRuleParser = dateRuleParser;
            SignificantDates = significantDates;
        }

        #endregion

        #region Business days

        /// <summary>
        /// Returns <c>true</c> iff the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to check.</param>
        /// <returns><c>true</c> iff the date is a business day for the given market.</returns>
        public abstract bool IsBusinessDay(DateTime date);

        /// <summary>
        /// Returns <c>true</c> iff the date is a holiday for the given market.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to check.</param>
        /// <returns><c>true</c> iff the date is a holiday for the given market.</returns>
        public bool IsHoliday(DateTime date)
        {
            return !IsBusinessDay(date);
        }

        #endregion

        #region Rolling and advancing

        /// <summary>
        /// Find the next business day with respect to the given date and
        /// rolling convention.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>Returns the resulting <see cref="DateTime"/>.</returns>
        public DateTime Roll(DateTime date, BusinessDayConventionEnum businessDayConvention) 
        {
            DateTime rolledDate = date;
            if (businessDayConvention == BusinessDayConventionEnum.FOLLOWING || businessDayConvention == BusinessDayConventionEnum.MODFOLLOWING) 
            {
                while (IsHoliday(rolledDate))
                {
                    rolledDate = Calendar.AddDays(rolledDate, 1);
                }
                if (businessDayConvention == BusinessDayConventionEnum.MODFOLLOWING && rolledDate.Month != date.Month)
                {
                    return Roll(date, BusinessDayConventionEnum.PRECEDING);
                }
            }
            else if (businessDayConvention == BusinessDayConventionEnum.PRECEDING || businessDayConvention == BusinessDayConventionEnum.MODPRECEDING) 
            {
                while (IsHoliday(rolledDate))
                {
                    rolledDate = Calendar.AddDays(rolledDate, -1);
                }
                if (businessDayConvention == BusinessDayConventionEnum.MODPRECEDING && rolledDate.Month != date.Month)
                {
                    return Roll(date, BusinessDayConventionEnum.FOLLOWING);
                }
            } 
            else if ( businessDayConvention != BusinessDayConventionEnum.NONE ) 
            {
                throw new ArgumentOutOfRangeException(nameof(businessDayConvention), "Unknown rolling convention.");
            }

            return rolledDate;
        }


        /// <summary>
        /// Holidays  between dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> HolidaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            // Now just return the dates in the range specified by start and end dates
            var allSignificantDates = GetDatesInRange(startDate, endDate);
            return allSignificantDates;
        }

        /// <summary>
        /// Businesses the days between dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> BusinessDaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            const int cIncrementDay = 1;
            var allSignicantDates = new List<DateTime>();
            List<DateTime> holidays = HolidaysBetweenDates(startDate, endDate);
            DateTime dtbase = startDate;
            while(dtbase <= endDate)
            {
                if (!RuleHelper.IsWeekend(dtbase))
                {
                    if (!holidays.Contains(dtbase))
                        allSignicantDates.Add(dtbase);
                }
                dtbase = dtbase.AddDays(cIncrementDay);
            }
            return allSignicantDates;
        }

        /// <summary>
        /// Years between dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public int YearsBetweenDates(DateTime startDate, DateTime endDate)
        {
            if ((endDate.Month > startDate.Month || endDate.Month == startDate.Month) && (endDate.Day < startDate.Day))
                return endDate.Year - startDate.Year - 1;
            return endDate.Year - startDate.Year;
        }

        /// <summary>
        /// Advances the given date the given offset.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> that is to be advanced.</param>
        /// <param name="offset">The <see cref="Offset"/> that the given date is advanced.</param>
        /// <param name="businessDayConvention">The <see cref="BusinessDayConventionEnum"/> to use.</param>
        /// <returns>Returns the resulting <see cref="DateTime"/>.</returns>
        public DateTime Advance(DateTime date, Offset offset, BusinessDayConventionEnum businessDayConvention)
        {
            return Advance(date, int.Parse(offset.periodMultiplier), offset.period, offset.dayType, businessDayConvention);
        }

        /// <summary>
        /// Advances the given date the given offset.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> that is to be advanced.</param>
        /// <param name="offset">The <see cref="Offset"/> that the given date is advanced.</param>
        /// <returns>Returns the resulting <see cref="DateTime"/>.</returns>
        public DateTime Advance(DateTime date, RelativeDateOffset offset)
        {
            return Advance(date, int.Parse(offset.periodMultiplier), offset.period, offset.dayType, offset.businessDayConvention);
        }

        /// <summary>
        /// Advances the given <see cref="DateTime"/> the given
        /// <paramref name="periodMultiplier"/> of time <paramref name="period"/>.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> that is to be advanced.</param>
        /// <param name="periodMultiplier">The number of time period to advance.</param>
        /// <param name="period">The <see cref="PeriodEnum"/> used.</param>
        /// <param name="dayType">Type of the day.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>
        /// Returns the resulting <see cref="DateTime"/>.
        /// </returns>
        /// <overloads>
        /// Advances the given <see cref="DateTime"/>.
        /// </overloads>
        private DateTime Advance(DateTime date, int periodMultiplier, PeriodEnum period, DayTypeEnum dayType, BusinessDayConventionEnum businessDayConvention)
        {
            #region Assert validity of parameters

            if ((dayType != DayTypeEnum.Business) & (dayType != DayTypeEnum.Calendar))
            {
                throw new ArgumentOutOfRangeException(nameof(dayType), dayType, "Only 'DayTypeEnum.Business' and 'DayTypeEnum.Calendar' day types are currently supported.");
            }
            //  We can only use Business dayType for days intervals.
            //
            if ((dayType == DayTypeEnum.Business) & (period != PeriodEnum.D))
            {
                throw new NotSupportedException();
            }

            #endregion

            // handling the NONE day convention
            if (businessDayConvention == BusinessDayConventionEnum.NONE)
            {
                // No rolling if multiplier is zero
                if (periodMultiplier == 0)
                {
                    return date;
                }
                BusinessDayConventionEnum advConvention = periodMultiplier > 0? BusinessDayConventionEnum.FOLLOWING: BusinessDayConventionEnum.PRECEDING;
                return Advance(date, periodMultiplier, period, dayType, advConvention);
            }
            if (periodMultiplier == 0)
            {
                return Roll(date, businessDayConvention);
            }
            if ((period == PeriodEnum.D) & DayTypeEnum.Business == dayType) //Business days
            {
                DateTime returnValue = date;
                if (periodMultiplier > 0)
                {
                    while (periodMultiplier > 0)
                    {
                        returnValue = returnValue.AddDays(1);
                        while (IsHoliday(returnValue))
                        {
                            returnValue = returnValue.AddDays(1);
                        }
                        periodMultiplier--;
                    }
                }
                else
                {
                    while (periodMultiplier < 0)
                    {
                        returnValue = returnValue.AddDays(-1);
                        while (IsHoliday(returnValue))
                        {
                            returnValue = returnValue.AddDays(-1);
                        }
                        periodMultiplier++;
                    }
                }
                return returnValue;
            }
            var interval = new Period {period = period, periodMultiplier = periodMultiplier.ToString(CultureInfo.InvariantCulture)};
            return Roll(Add(date, interval), businessDayConvention);
        }


        /// <summary>
        /// TODO - remove this function from this class
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private static DateTime Add(DateTime dateTime, Period interval)
        {
            int periodMultiplierAsInt = int.Parse(interval.periodMultiplier);
            switch (interval.period)
            {
                case PeriodEnum.D:
                    {
                        return dateTime.AddDays(periodMultiplierAsInt);
                    }
                case PeriodEnum.W:
                    // This may not be correct for some calendars
                    // where a week has not always 7 days.
                    // Calendar.AddWeeks() is the right way to go
                    //
                    {
                        return dateTime.AddDays(7 * periodMultiplierAsInt);
                    }
                case PeriodEnum.M:
                    // ditto with months resp. Calendar.AddMonths()
                    //
                    {
                        return dateTime.AddMonths(periodMultiplierAsInt);
                    }
                case PeriodEnum.Y:
                    {
                        return dateTime.AddYears(periodMultiplierAsInt);
                    }
                default:
                    {
                        string intervalAsString = IntervalToString(interval);
                        throw new ArgumentOutOfRangeException(nameof(interval), intervalAsString, "specified period type is not supported by this function");
                    }
            }
        }

        /// <summary>
        /// Converts an Interval to a string.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static string IntervalToString(Period interval)
        {
            return $"{interval.periodMultiplier}{interval.period}";
        }

        #endregion

        #region Helpers

        /// <summary>
        /// A string representation of the Calendar.
        /// </summary>
        /// <returns>A String representing the object.</returns>
        public override String ToString()
        {
            return string.Join(" ", DateRuleParser.Name).Trim();
        }

        /// <summary>
        /// A string representation of this Calendar for use with FpML.
        /// </summary>
        /// <returns>An FpML String representing the object.</returns>
        /// <remarks>
        /// This method is used for interaction with FpML.
        /// </remarks>
        public string ToFpML()
        {
            return string.Join(" ", DateRuleParser.FpmlName).Trim();
        }

        /// <summary>
        /// Gets the dates in range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public List<DateTime> GetDatesInRange(DateTime startDate, DateTime endDate)
        {
            var signicantDatesInRange = new List<DateTime>();
            if (Dates.Count > 0)
            {
                //Dates.Sort();
                int startIndex = IndexDateInList(Dates, startDate);
                int endIndex = IndexDateInList(Dates, endDate);
                int itemSpan = endIndex - startIndex;
                if (itemSpan == 0)
                {
                    if (Dates[startIndex].Date == startDate)
                        signicantDatesInRange = Dates.GetRange(startIndex, 1);
                }
                else
                {
                    int lastItemIndex = (endIndex < Dates.Count && Dates[endIndex] == endDate) ? itemSpan + 1 : itemSpan;
                    signicantDatesInRange = Dates.GetRange(startIndex, lastItemIndex);
                }
            }
            return signicantDatesInRange;
        }

        /// <summary>
        /// Indexes the date in list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="date">The date.</param>
        private int IndexDateInList<TEnumT>(List<TEnumT> list, TEnumT date)
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

        private static IEnumerable<DateTime> ConvertDates(IEnumerable<SignificantDay> sigDates)
        {
            var dates = sigDates.Select(sigDate => sigDate.Date).ToList();
            dates.Sort();
            return dates;
        }

        /// <summary>
        /// Returns true if the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The date to check.</param>
        public bool IsSignificantDay(DateTime date)
        {
            return Dates.Contains(date);
        }

        #endregion
    }
}