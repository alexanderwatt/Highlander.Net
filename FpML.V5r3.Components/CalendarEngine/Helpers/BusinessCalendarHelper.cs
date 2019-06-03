#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.CalendarEngine.Helpers
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