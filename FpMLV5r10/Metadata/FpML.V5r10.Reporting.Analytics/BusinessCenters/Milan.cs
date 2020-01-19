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
using Highlander.Numerics.Dates;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.BusinessCenters
{
    /// <summary>
    /// Milan calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st</description></item>
    /// <item><description>Epiphany, January 6th</description></item>
    /// <item><description>Easter Monday</description></item>
    /// <item><description>Liberation Day, April 25th</description></item>
    /// <item><description>Labour Day, May 1st</description></item>
    /// <item><description>Republic Day, June 2nd (since 2000)</description></item>
    /// <item><description>Assumption, August 15th</description></item>
    /// <item><description>All Saint's Day, November 1st</description></item>
    /// <item><description>Immaculate Conception, December 8th</description></item>
    /// <item><description>Christmas, December 25th</description></item>
    /// <item><description>St. Stephen's Day, December 26th</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Milan : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Milan.Instance in .NET applications.") ]
        public Milan() 
            : base( "Milan", CultureInfo.CreateSpecificCulture("it-IT"))
            // alt. culture: de-CH
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ITMI";
        }

        #region FactoryItem pattern

        [ Obsolete ] // just to ignore the CS0618 warning below
        static Milan()
        {
            Instance = new Milan();	// CS0618
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
        /// <param name="em">The day of Easter Monady in the year, between 1 and 366.</param>
        /// <returns><c>True</c> when the given day is a business day.</returns>
        protected override bool IsBusinessDay(
            int d, Month m, int y,
            DayOfWeek w, int dd, int em )
        {
            if ((w == DayOfWeek.Saturday || w == DayOfWeek.Sunday)
                // New Year's Day
                || (d == 1 && m == Month.January)
                // Epiphany
                || (d == 6 && m == Month.January)
                // Easter Monday
                || (dd == em)
                // Liberation Day
                || (d == 25 && m == Month.April)
                // Labour Day
                || (d == 1 && m == Month.May)
                // Republic Day
                || (d == 2 && m == Month.June && y >= 2000)
                // Assumption
                || (d == 15 && m == Month.August)
                // All Saints' Day
                || (d == 1 && m == Month.November)
                // Immaculate Conception
                || (d == 8 && m == Month.December)
                // Christmas
                || (d == 25 && m == Month.December)
                // St. Stephen's Day
                || (d == 26 && m == Month.December)
                )
                return false;
            return true;
        }
    }
}