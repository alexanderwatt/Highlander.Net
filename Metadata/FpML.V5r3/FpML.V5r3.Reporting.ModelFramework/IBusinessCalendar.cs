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

#region Using directives

using System;
using System.Globalization;
using System.Collections.Generic;

using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework
{
    /// <summary>
    /// This interface provides methods for determining whether a date is a business 
    /// day or a holiday for a given market, and for incrementing/decrementing a 
    /// date of a given number of business days.
    /// </summary>
    /// <remarks>
    /// This interface is mainly for COM interop.
    /// </remarks>
    public interface IBusinessCalendar 
    {
        /// <summary>
        /// Returns <c>true</c> iff the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to check.</param>
        /// <returns><c>true</c> iff the date is a business day for the given market.</returns>
        bool IsBusinessDay(DateTime date);

        /// <summary>
        /// Returns <c>true</c> iff the date is a holiday for the given market.
        /// </summary>
        /// <param name="date">The System.DateTime to check.</param>
        /// <returns><c>true</c> iff the date is a holiday for the given market.</returns>
        bool IsHoliday(DateTime date);

        /// <summary>
        /// Holidays between.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        List<DateTime> HolidaysBetweenDates(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Businesses the days between dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        List<DateTime> BusinessDaysBetweenDates(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Find the next business day with respect to the given date and
        /// rolling convention.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        DateTime Roll(DateTime date, BusinessDayConventionEnum businessDayConvention);

        /// <summary>
        /// A string representation of this DayCounter for use with FpML.
        /// </summary>
        /// <remarks>
        /// This method is used for interaction with FpML.
        /// </remarks>
        /// <returns>An FpML String representing the object.</returns>
        string ToFpML();

        /// <summary>
        /// Advances the specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns></returns>
        DateTime Advance(DateTime date, Offset offset, BusinessDayConventionEnum businessDayConvention);

        /// <summary>
        /// The underlying <see cref="System.Globalization.Calendar"/>.
        /// </summary>
        Calendar Calendar { get ; }

        /// <summary>
        /// The associated <see cref="System.Globalization.CultureInfo"/>.
        /// </summary>
        CultureInfo Culture { get ; }

        /// <summary>
        /// Gets the name list.
        /// </summary>
        /// <value>The name list.</value>
        string NameList { get; }

        /// <summary>
        /// Gets the deduped name list.
        /// </summary>
        /// <value>The deduped name list.</value>
        string DedupedNameList { get; }
    }
}