#region Usings

using System;
using System.Collections.Generic;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Rules;

#endregion

namespace Orion.CalendarEngine
{
    /// <summary>
    /// Base class for Business Calendars
    /// </summary>
    public class BusinessCalendar : BaseCalendar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessCalendar"/> class with no holidays other than weekends.
        /// </summary>
        public BusinessCalendar()
            : base(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessCalendar"/> class.
        /// </summary>
        /// <param name="dateRuleParser">Name(s) of the calendar.</param>
        /// <param name="businessCentreHolidays">The list of significant days for this joint set of holidays..</param>
        public BusinessCalendar(List<SignificantDay> businessCentreHolidays, IDateRuleParser dateRuleParser)
            : base(businessCentreHolidays, dateRuleParser)
        {
        }

        /// <summary>
        /// Returns true if the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The date to check.</param>
        public override bool IsBusinessDay(DateTime date)
        {
            Boolean bRet = date.DayOfWeek != DayOfWeek.Saturday 
                        && date.DayOfWeek != DayOfWeek.Sunday 
                        && !IsSignificantDay(date);

            return bRet;
        }
    }
}