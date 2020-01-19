/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.CalendarEngine.Helpers;
using FpML.V5r10.CalendarEngine.Rules;

#endregion

namespace FpML.V5r10.CalendarEngine
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