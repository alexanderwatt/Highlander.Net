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

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.CalendarEngine.Helpers;

namespace FpML.V5r10.CalendarEngine.Dates
{
    /// <summary>
    /// Implements the BOE calendar interface
    /// </summary>
    public class BOEDate: CentralBankDate
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEDate"/> class.
        /// </summary>
        public BOEDate()
            : base(CentralBanks.BOE)
        {}

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected override DateTime? DateRules(int month, int year)
        {
            if (!IsValidMonth(month)) return null;
            var date = RuleHelper.GetNthDayInMonth(month, 2, 2, year, 0, 0);
            return date;
        }

        /// <summary>
        /// Gets the date rules.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        protected override List<DateTime> DateRules(int year)
        {
            return ValidMeetingMonths.Select(month => RuleHelper.GetNthDayInMonth(month, 2, 2, year, 0, 0)).ToList();
        }
    }
}