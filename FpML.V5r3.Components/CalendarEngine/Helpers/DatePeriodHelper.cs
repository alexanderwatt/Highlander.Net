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
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework;
using Orion.Analytics.BusinessCenters;

#endregion

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// Helper class for financial dates
    /// </summary>
    public static class DatePeriodHelper
    {
        /// <summary>
        /// Adds the periods.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <param name="dayTypes">The day types.</param>
        /// <param name="periodIntervals">The period intervals.</param>
        /// <param name="businessDayConventions">The business day conventions.</param>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        public static List<DateTime> AddPeriods(DateTime[] dates, string[] dayTypes, string[] periodIntervals, string[] businessDayConventions, IBusinessCalendar calendar)
        {
            var periodDates = new List<DateTime>();
            DateTime dateAdv;
            if ((dates.Length == dayTypes.Length) && (dates.Length == periodIntervals.Length) && (dates.Length == businessDayConventions.Length))
            {
                for (int index = 0; index < dates.Length; index++)
                {
                    dateAdv = AddPeriod(dates[index], periodIntervals[index], calendar, businessDayConventions[index], dayTypes[index]);
                    if (!periodDates.Contains(dateAdv))
                        periodDates.Add(dateAdv);
                }
            }
            else
            {
                foreach (DateTime date in dates)
                {
                    foreach (string intervalStr in periodIntervals)
                    {
                        foreach (string dayType in dayTypes)
                        {
                            foreach (string dayConvention in businessDayConventions)
                            {
                                dateAdv = AddPeriod(date, intervalStr, calendar, dayConvention, dayType);
                                if (!periodDates.Contains(dateAdv))
                                    periodDates.Add(dateAdv);
                            }
                        }
                    }
                }
            }
            return periodDates;
        }

        /// <summary>
        /// Adds the period.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="tenorString">The tenor string.</param>
        /// <param name="calendar">The calendar.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="dayType">Type of the day.</param>
        /// <returns></returns>
        public static DateTime AddPeriod(DateTime startDate, string tenorString, IBusinessCalendar calendar, string rollConvention, string dayType)
        {
            const string defaultRollConvention = "FOLLOWING";

            if (calendar==null)
            {
                calendar = new Hell();
            }
            if (String.IsNullOrEmpty(rollConvention))
            {
                rollConvention = defaultRollConvention;
            }
            Period rollPeriod = PeriodHelper.Parse(tenorString);
            if (String.IsNullOrEmpty(dayType))
            {
                dayType = DayTypeStringFromRollPeriodInterval(rollPeriod);
            }
            var dayTypeEnum = (DayTypeEnum)Enum.Parse(typeof(DayTypeEnum), dayType, true);
            BusinessDayConventionEnum businessDayConvention = BusinessDayConventionHelper.Parse(rollConvention);
            DateTime endDate = calendar.Advance(startDate, OffsetHelper.FromInterval(rollPeriod, dayTypeEnum), businessDayConvention);         
            return endDate;
        }

        /// <summary>
        ///  Derives the Day type string from roll period.interval.
        /// </summary>
        /// <param name="rollPeriod">The roll period.</param>
        /// <returns></returns>
        private static string DayTypeStringFromRollPeriodInterval(Period rollPeriod)
        {
            string dayType = rollPeriod.period == PeriodEnum.D ? "Business" : "Calendar";
            return dayType;
        }

        ///// <summary>
        ///// Converts to business calendar.
        ///// </summary>
        ///// <param name="centers">The centers.</param>
        ///// <returns></returns>
        //private static IBusinessCalendar ConvertToBusinessCalendar(BusinessCenters centers)
        //{
        //    IBusinessCalendar bc = null;

        //    string[] calendarNames = centers.businessCenter.Select(businessCenter => businessCenter.Value).ToArray();

        //    if (calendarNames.Count() > 0)
        //    {
        //        bc = BusinessCalendarHelper.GetCalendar(calendarNames);
        //    }
        //    return bc;
        //}
    }
}