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
using Orion.Analytics.Dates;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// Helsinki calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st</description></item>
    /// <item><description>Epiphany, January 6th</description></item>
    /// <item><description>Good Friday</description></item>
    /// <item><description>Easter Monday</description></item>
    /// <item><description>Ascension Thursday</description></item>
    /// <item><description>Labor Day, May 1st</description></item>
    /// <item><description>Midsummer Eve (Friday between June 18-24)</description></item>
    /// <item><description>Independence Day, December 6th</description></item>
    /// <item><description>Christmas Eve, December 24th</description></item>
    /// <item><description>Christmas, December 25th</description></item>
    /// <item><description>Boxing Day, December 26th</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Helsinki : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Helsinki.Instance in .NET applications.") ]
        public Helsinki() 
            : base( "Helsinki", CultureInfo.CreateSpecificCulture("fi-FI"))
            // alt. culture: sv-FI
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "FIHE";
        }

        #region FactoryItem pattern

        [ Obsolete() ] // just to ignore the CS0618 warning below
        static Helsinki()
        {
            Instance = new Helsinki();	// CS0618
        }

        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly IBusinessCalendar Instance;

        #endregion

        /// <summary>
        /// Implementation of the public <see cref="IsBusinessDay"/> method.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by concrete calendar implementations.
        /// </remarks>
        /// <param name="d">The day value, between 1 and 31.</param>
        /// <param name="m">The month, one of <see cref="Month"/>.</param>
        /// <param name="y">The year, between 1 and 9999.</param>
        /// <param name="w">A <see cref="DayOfWeek"/> enumerated constant that
        /// indicates the day of the week. 
        /// This property value ranges from zero, indicating Sunday,
        /// to six, indicating Saturday.</param>
        /// <param name="dd">The day of the year, between 1 and 366.</param>
        /// <param name="em">The day of Easter Monday in the year, between 1 and 366.</param>
        /// <returns><c>True</c> when the given day is a business day.</returns>
        protected override bool IsBusinessDay(
            int d, Month m, int y,
            DayOfWeek w, int dd, int em )
        {
            if (w == DayOfWeek.Saturday || w == DayOfWeek.Sunday
                // New Year's Day
                || (d == 1 && m == Month.January)
                // Epiphany
                || (d == 6 && m == Month.January)
                // Good Friday
                || (dd == em-3)
                // Easter Monday
                || (dd == em)
                // Ascension Thursday
                || (dd == em+38)
                // Labor Day
                || (d == 1 && m == Month.May)
//				// Midsummer Eve
//				|| (d == 21 && m == Month.June)
                // Midsummer Eve (Friday between June 18-24)
                || (w == DayOfWeek.Friday && (d >= 18 && d <= 24) && m == Month.June)
                // Independence Day
                || (d == 6 && m == Month.December)
                // Christmas Eve
                || (d == 24 && m == Month.December)
                // Christmas
                || (d == 25 && m == Month.December)
                // Boxing Day
                || (d == 26 && m == Month.December)
                )
                return false;
            return true;
        }
    }
}