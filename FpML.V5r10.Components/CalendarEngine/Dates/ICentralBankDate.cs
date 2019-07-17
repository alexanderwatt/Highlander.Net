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

#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Orion.CalendarEngine.Dates
{
    /// <summary>
    /// The base central bank calendar interface
    /// </summary>
    public interface ICentralBankDate
    {
        /// <summary>
        /// Gets the Central Bank days.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        List<DateTime> GetCentralBankDays(int year);

        /// <summary>
        /// Gets the Central Bank day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        DateTime? GetCentralBankDay(int month, int year);

        /// <summary>
        /// Gets the Central Bank days.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns></returns>
        List<DateTime> GetCentralBankDays(DateTime startDate, DateTime endDate, Boolean mainCycle);

        /// <summary>
        /// Gets the central bank days.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="noOfMonths">The no of months.</param>
        /// <returns></returns>
        List<DateTime> GetCentralBankDays(DateTime referenceDate, int noOfMonths);

    }
}