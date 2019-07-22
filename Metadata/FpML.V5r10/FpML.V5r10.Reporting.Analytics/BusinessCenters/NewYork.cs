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
    /// New York calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st 
    /// (possibly moved to Monday if actually on Sunday, 
    /// or to Friday if on Saturday)</description></item>
    /// <item><description>Martin Luther King's birthday, third Monday in January</description></item>
    /// <item><description>Washington's birthday, third Monday in February</description></item>
    /// <item><description>Memorial Day, last Monday in May</description></item>
    /// <item><description>Independence Day, July 4th
    /// (moved to Monday if Sunday or Friday if Saturday</description></item>
    /// <item><description>Labour Day, 
    /// first Monday of September</description></item>
    /// <item><description>Veteran's Day, November 11th 
    /// (moved to Monday if Sunday or Friday if Saturday)</description></item>
    /// <item><description>Thanksgiving Day, 
    /// fourth Thursday in November</description></item>
    /// <item><description>Christmas, December 25th
    /// (moved to Monday if Sunday or Friday if Saturday)</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class NewYork : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use NewYork.Instance in .NET applications.") ]
        public NewYork() 
            : base( "NewYork", CultureInfo.CreateSpecificCulture("en-US"))
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "USNY";
        }

        #region FactoryItem pattern

        [ Obsolete ] // just to ignore the CS0618 warning below
        static NewYork()
        {
            Instance = new NewYork();	// CS0618
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
                // New Year's Day (possibly moved to Monday if on Sunday)
                || ((d == 1 || (d == 2 && w == DayOfWeek.Monday)) && m == Month.January)
                // (or to Friday if on Saturday)
                || (d == 31 && w == DayOfWeek.Friday && m == Month.December)
                // Martin Luther King's birthday (third Monday in January)
                || ((d >= 15 && d <= 21) && w == DayOfWeek.Monday && m == Month.January)
                // Washington's birthday (third Monday in February)
                || ((d >= 15 && d <= 21) && w == DayOfWeek.Monday && m == Month.February)
                // Memorial Day (last Monday in May)
                || (d >= 25 && w == DayOfWeek.Monday && m == Month.May)
                // Independence Day (Monday if Sunday or Friday if Saturday)
                || ((d == 4 || (d == 5 && w == DayOfWeek.Monday) ||
                     (d == 3 && w == DayOfWeek.Friday)) && m == Month.July)
                // Labor Day (first Monday in September)
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.September)
                // Columbus Day (second Monday in October)
                || ((d >= 8 && d <= 14) && w == DayOfWeek.Monday && m == Month.October)
                // Veteran's Day (Monday if Sunday or Friday if Saturday)
                || ((d == 11 || (d == 12 && w == DayOfWeek.Monday) ||
                     (d == 10 && w == DayOfWeek.Friday)) && m == Month.November)
                // Thanksgiving Day (fourth Thursday in November)
                || ((d >= 22 && d <= 28) && w == DayOfWeek.Thursday && m == Month.November)
                // Christmas (Monday if Sunday or Friday if Saturday)
                || ((d == 25 || (d == 26 && w == DayOfWeek.Monday) ||
                     (d == 24 && w == DayOfWeek.Friday)) && m == Month.December)
                )
                return false;
            return true;
        }
    }
}