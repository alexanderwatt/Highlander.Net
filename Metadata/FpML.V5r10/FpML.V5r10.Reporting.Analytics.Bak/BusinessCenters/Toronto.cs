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

#region Using directives

using System;
using System.Globalization;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Dates;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// Toronto calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st (possibly moved to Monday)</description></item>
    /// <item><description>Good Friday</description></item>
    /// <item><description>Easter Monday</description></item>
    /// <item><description>Victoria Day, The Monday on or preceding 24 May</description></item>
    /// <item><description>Canada Day, July 1st (possibly moved to Monday)</description></item>
    /// <item><description>Provincial Holiday, first Monday of August</description></item>
    /// <item><description>Labor Day, first Monday of September</description></item>
    /// <item><description>Thanksgiving Day, second Monday of October</description></item>
    /// <item><description>Remembrance Day, November 11th (possibly moved to Monday)</description></item>
    /// <item><description>Christmas, December 25th (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>Boxing Day, December 26th (possibly moved to Monday or Tuesday)</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Toronto : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Toronto.Instance in .NET applications.") ]
        public Toronto() 
            : base( "Toronto", CultureInfo.CreateSpecificCulture("en-CA"))
        {}

        #region FactoryItem pattern

        [ Obsolete() ] // just to ignore the CS0618 warning below
        static Toronto()
        {
            Instance = new Toronto();	// CS0618
        }

        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly IBusinessCalendar Instance;

        #endregion

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "CATO";
        }

        /// <summary>
        /// Implementation of the public <see cref="IsBusinessDay"/> method.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by concrete
        /// calendar implementations.
        /// </remarks>
        /// <param name="d">The day value, between 1 and 31.</param>
        /// <param name="m">The month, one of <see cref="Month"/>.</param>
        /// <param name="y">The year, between 1 and 9999.</param>
        /// <param name="w">A <see cref="DayOfWeek"/> enumerated constant that
        /// indicates the day of the week. 
        /// This property value ranges from zero, indicating Sunday,
        /// to six, indicating Saturday.</param>
        /// <param name="dd">The day of the year, between 1 and 366.</param>
        /// <param name="em">The day of Easter Monady in the year, between 1 and 366.</param>
        /// <returns><c>True</c> when the given day is a business day.</returns>
        protected override bool IsBusinessDay(
            int d, Month m, int y,
            DayOfWeek w, int dd, int em )
        {
            if ((w == DayOfWeek.Saturday || w == DayOfWeek.Sunday)
                // New Year's Day (possibly moved to Monday)
                || ((d == 1 || ((d == 2 || d == 3) && w == DayOfWeek.Monday)) && m == Month.January)
                // Good Friday
                || (dd == em-3)
                // Easter Monday
                || (dd == em)
                // The Monday on or preceding 24 May (Victoria Day)
                || (d > 17 && d <= 24 && w == DayOfWeek.Monday && m == Month.May)
                // July 1st, possibly moved to Monday (Canada Day)
                || ((d == 1 || ((d == 2 || d == 3) && w == DayOfWeek.Monday)) &&  m == Month.July)
                // first Monday of August (Provincial Holiday)
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.August)
                // first Monday of September (Labor Day)
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.September)
                // second Monday of October (Thanksgiving Day)
                || (d > 7 && d <= 14 && w == DayOfWeek.Monday && m == Month.October)
                // November 11th (Remembrance Day, possibly moved to Monday)
                || ((d == 11 || (d == 12 && w == DayOfWeek.Monday)) && m == Month.November)
                // Christmas (possibly moved to Monday or Tuesday)
                || ((d == 25 || (d == 27 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.December)
                // Boxing Day (possibly moved to Monday or Tuesday)
                || ((d == 26 || (d == 28 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.December)
                )
                return false;
            return true;
        }
    }
}