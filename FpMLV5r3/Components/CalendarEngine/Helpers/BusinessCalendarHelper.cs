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
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Utilities.NamedValues;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.CalendarEngine.V5r3.Helpers
{
    /// <summary>
    /// Business Calendar Wrapper helper class
    /// </summary>
    public static class BusinessCalendarHelper
    {
        /// <summary>
        /// Creates a user defined calendar.
        /// </summary>
        /// <param name="calendarProperties">THe calendar properties must include a valid FpML business center name.</param>
        /// <param name="holidaysDates">The dates that are in the defined calendar.</param>
        /// <returns></returns>
        public static BusinessCenterCalendar CreateCalendar(NamedValueSet calendarProperties, List<DateTime> holidaysDates)
        {
            //Build the calendar.
            var bc = new BusinessCenterCalendar();
            var center = calendarProperties.GetValue<string>("BusinessCenter", true);
            bc.BusinessCenter = center;
            //bc.Location = locationCalendarYear.RDMLocation;
            //bc.LocationName = locationCalendarYear.LocationName;
            bc.Holidays = holidaysDates.Select(date => new DateRule {Item = date}).ToArray();
            return bc;
        }

        /// <summary>
        /// Advances the specified calendars.
        /// </summary>
        /// <param name="businessCalendar">The calendars.</param>
        /// <param name="date">The date.</param>
        /// <param name="dayTypeString">The day type string.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        public static DateTime Advance(IBusinessCalendar businessCalendar, DateTime date, string dayTypeString, string periodInterval, string businessDayConvention)
        {
            //IBusinessCalendar calendar = GetCalendar(calendars, ruleFullFilePath);
            Period interval = PeriodHelper.Parse(periodInterval);
            var dayType = DayTypeEnum.Calendar;
            if (dayTypeString.Length > 0)
                dayType = (DayTypeEnum)Enum.Parse(typeof(DayTypeEnum), dayTypeString, true);
            Offset offset = OffsetHelper.FromInterval(interval, dayType);
            BusinessDayConventionEnum dayConvention = BusinessDayConventionHelper.Parse(businessDayConvention);
            return businessCalendar.Advance(date, offset, dayConvention);
        }

        /// <summary>
        /// Rolls the specified date using the underlying calendars.
        /// </summary>
        /// <param name="businessCalendar">The calendars.</param>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        public static DateTime Roll(IBusinessCalendar businessCalendar, DateTime date, string businessDayConvention)
        {
            BusinessDayConventionEnum dayConvention = BusinessDayConventionHelper.Parse(businessDayConvention);
            return businessCalendar.Roll(date, dayConvention);
        }
    }
}