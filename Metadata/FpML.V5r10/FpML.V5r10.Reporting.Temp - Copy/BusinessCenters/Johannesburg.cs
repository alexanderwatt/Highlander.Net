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
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// Johannesburg calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st (possibly moved to Monday)</description></item>
    /// <item><description>Good Friday</description></item>
    /// <item><description>Family Day, Easter Monday</description></item>
    /// <item><description>Human Rights Day, March 21st (possibly moved to Monday)</description></item>
    /// <item><description>Freedom Day, April 27th (possibly moved to Monday)</description></item>
    /// <item><description>Workers Day, May 1st (possibly moved to Monday)</description></item>
    /// <item><description>Youth Day, June 16th (possibly moved to Monday)</description></item>
    /// <item><description>National Women's Day, August 9th (possibly moved to Monday)</description></item>
    /// <item><description>Heritage Day, September 24th (possibly moved to Monday)</description></item>
    /// <item><description>Day of Reconciliation, December 16th (possibly moved to Monday)</description></item>
    /// <item><description>Christmas, December 25th</description></item>
    /// <item><description>Day of Goodwill, December 26th (possibly moved to Monday)</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Johannesburg : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Johannesburg.Instance in .NET applications.") ]
        public Johannesburg() 
            : base( "Johannesburg", CultureInfo.CreateSpecificCulture("af-ZA"))
            // alternative culture: en-ZA
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ZAJO";
        }

        #region FactoryItem pattern

        [ Obsolete ] // just to ignore the CS0618 warning below
        static Johannesburg()
        {
            Instance = new Johannesburg();	// CS0618
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
                // Family Day (Easter Monday)
                || (dd == em)
                // Human Rights Day, March 21st (possibly moved to Monday)
                || ((d == 21 || ((d == 22 || d == 23)&& w == DayOfWeek.Monday)) && m == Month.March)
                // Freedom Day, April 27th (possibly moved to Monday)
                || ((d == 27 || ((d == 28 || d == 29) && w == DayOfWeek.Monday)) && m == Month.April)
                // Workers Day, May 1st (possibly moved to Monday)
                || ((d == 1 || ((d == 2 || d == 3) && w == DayOfWeek.Monday)) && m == Month.May)
                // Youth Day, June 16th (possibly moved to Monday)
                || ((d == 16 || ((d == 17 || d == 18) && w == DayOfWeek.Monday)) && m == Month.June)
                // National Women's Day, August 9th (possibly moved to Monday)
                || ((d == 9 || ((d == 10 || d == 11) && w == DayOfWeek.Monday)) && m == Month.August)
                // Heritage Day, September 24th (possibly moved to Monday)
                || ((d == 24 || ((d == 25 || d == 26) && w == DayOfWeek.Monday)) && m == Month.September)
                // Day of Reconciliation, December 16th (possibly moved to Monday)
                || ((d == 16 || ((d == 17 || d == 18) && w == DayOfWeek.Monday)) && m == Month.December)
                // Christmas 
                || (d == 25 && m == Month.December)
                // Day of Goodwill (possibly moved to Monday)
                || ((d == 26 || ((d == 27 || d == 28) && w == DayOfWeek.Monday)) && m == Month.December)
                )
                return false;
            return true;
        }
    }
}