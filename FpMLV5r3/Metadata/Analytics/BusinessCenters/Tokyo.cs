/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Globalization;
using Highlander.Reporting.Analytics.V5r3.Dates;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Business;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.BusinessCenters
{
    /// <summary>
    /// Tokyo calendar.
    /// </summary>
    /// <remarks>
    /// Regular holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st</description></item>
    /// <item><description>Bank Holiday, January 2nd</description></item>
    /// <item><description>Bank Holiday, January 3rd</description></item>
    /// <item><description>Coming of Age Day, 2nd Monday in January since 2000
    ///		(January 15th before)</description></item>
    /// <item><description>National Foundation Day, February 11th</description></item>
    /// <item><description>Vernal Equinox</description></item>
    /// <item><description>Greenery Day, April 29th</description></item>
    /// <item><description>Constitution Memorial Day, May 3rd</description></item>
    /// <item><description>Holiday for a Nation, May 4th</description></item>
    /// <item><description>Children's Day, May 5th</description></item>
    /// <item><description>Marine Day, 3rd Monday in July since 2003
    ///		(July 20th since 1996)</description></item>
    /// <item><description>Respect for the Aged Day, 3rd Monday in September since 2003
    ///		(September 15th before)</description></item>
    /// <item><description>Autumnal Equinox (if a single day falls between Respect 
    ///		for the Aged Day and the Autumnal Equinox, it is also a holiday since 2003)</description></item>
    /// <item><description>Health and Sports Day, 2nd Monday in October since 2000
    ///		(October 10th before)</description></item>
    /// <item><description>National Culture Day, November 3rd</description></item>
    /// <item><description>Labor Thanksgiving Day, November 23rd</description></item>
    /// <item><description>Emperor's Birthday, December 23rd (holiday since 1989)</description></item>
    /// <item><description>New Year's Eve, December 31st</description></item>
    /// </list>
    /// Implemented one-time holidays:
    /// <list type="table">
    /// <item><description>Marriage of Prince Akihito, April 10th 1959</description></item>
    /// <item><description>Rites of Imperial Funeral, February 24th 1989</description></item>
    /// <item><description>Enthronement Ceremony, November 12th 1990</description></item>
    /// <item><description>Marriage of Prince Naruhito, June 9th 1993</description></item>
    /// </list>
    /// Holidays falling on a Sunday are observed on the Monday following
    /// except for the Bank Holidays associated with the New Year.
    /// <para>
    /// Equinoxes are calculated using the astronomical equinox date.
    /// </para>
    /// </remarks>	
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Tokyo : CalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Tokyo.Instance in .NET applications.") ]
        public Tokyo() 
            : base( "Tokyo", CultureInfo.CreateSpecificCulture("ja-JP"))
        {
        }

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "JPTO";
        }

        /// <summary>Marriage of Prince Akihito, April 10th 1959.</summary>
        private static readonly DateTime MarriageOfPrinceAkihito;

        /// <summary>Rites of Imperial Funeral, February 24th 1989.</summary>
        private static readonly DateTime RitesOfImperialFuneral;

        /// <summary>Enthronement Ceremony, November 12th 1990.</summary>
        private static readonly DateTime EnthronementCeremony;

        /// <summary>Marriage of Prince Naruhito, June 9th 1993.</summary>
        private static readonly DateTime MarriageOfPrinceNaruhito;

        #region FactoryItem pattern

        [ Obsolete ] // just to ignore the CS0618 warning below
        static Tokyo()
        {
            Instance = new Tokyo();	// CS0618
            // supplied by KAWANISHI Tomoya <mailto:tomoya@mm.media.kyoto-u.ac.jp>)
            MarriageOfPrinceAkihito = new DateTime(1959, 4, 10, Instance.Calendar);
            RitesOfImperialFuneral = new DateTime(1989, 2, 24, Instance.Calendar);
            EnthronementCeremony = new DateTime(1990, 11, 12, Instance.Calendar);
            MarriageOfPrinceNaruhito = new DateTime(1993, 6, 9, Instance.Calendar);
        }

        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly IBusinessCalendar Instance;

        #endregion

        /// <summary>
        /// Returns <c>true</c> iff the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The System.DateTime to check.</param>
        /// <returns><c>true</c> iff the date is a business day for the given market.</returns>
        public override bool IsBusinessDay(DateTime date) 
        {
            DateTime dn = date.Date;	// normalized date
            int d = date.Day;
            Month m = (Month)date.Month;
            DayOfWeek w = date.DayOfWeek;
            // see generated tables below 
            int y = date.Year;
            int ve = VernalEquinox(y);
            int ae = AutumnalEquinox(y);
            if ((w == DayOfWeek.Saturday || w == DayOfWeek.Sunday)
                // New Year's Day
                || (d == 1  && m == Month.January)
                // Bank Holiday
                || (d == 2  && m == Month.January)
                // Bank Holiday
                || (d == 3  && m == Month.January)
                // Coming of Age Day (2nd Monday in January)
                // was January 15th before 2000
                || ((d == 15 || (d == 16 && w == DayOfWeek.Monday)) && m == Month.January && y < 2000)
                || (w == DayOfWeek.Monday && (d >= 8 && d <= 14) && m == Month.January && y >= 2000)
                // National Foundation Day
                || ((d == 11 || (d == 12 && w == DayOfWeek.Monday)) && m == Month.February)
                // Vernal Equinox 
                || ((d == ve || (d == (ve+1) && w == DayOfWeek.Monday)) && m == Month.March)
                // Greenery Day
                || ((d == 29 || (d == 30 && w == DayOfWeek.Monday)) && m == Month.April)
                // Constitution Memorial Day
                || (d == 3  && m == Month.May)
                // Holiday for a Nation
                || (d == 4  && m == Month.May)
                // Children's Day
                || ((d == 5  || (d == 6 && w == DayOfWeek.Monday)) && m == Month.May)
                // Marine Day
                || ((d == 20 || (d == 21 && w == DayOfWeek.Monday)) && m == Month.July)
                // Marine Day (3rd Monday in July),
                // was July 20th until 2003, not a holiday before 1996
                || (w == DayOfWeek.Monday && (d >= 15 && d <= 21) && m == Month.July && y >= 2003)
                || ((d == 20 || (d == 21 && w == DayOfWeek.Monday)) && m == Month.July && y >= 1996 && y < 2003)
                // Respect for the Aged Day (3rd Monday in September),
                // was September 15th before 2003
                || (w == DayOfWeek.Monday && (d >= 15 && d <= 21) && m == Month.September && y >= 2003)
                || ((d == 15 || (d == 16 && w == DayOfWeek.Monday)) && m == Month.September && y < 2003)
                // If a single day falls between Respect for the Aged Day
                // and the Autumnal Equinox, it is holiday
                || (w == DayOfWeek.Tuesday && d+1 == ae && d >= 16 && d <= 22 && m == Month.September && y >= 2003)
                // Autumnal Equinox 
                || ((d == ae || (d == ae+1 && w == DayOfWeek.Monday)) && m == Month.September)
                // Health and Sports Day (2nd Monday in October)
                // was October 10th before 2000
                || ((d == 10 || (d == 11 && w == DayOfWeek.Monday)) && m == Month.October && y < 2000)
                || (w == DayOfWeek.Monday && (d >= 8 && d <= 14) && m == Month.October && y >= 2000)
                // National Culture Day
                || ((d == 3  || (d == 4 && w == DayOfWeek.Monday)) && m == Month.November)
                // Labor Thanksgiving Day
                || ((d == 23 || (d == 24 && w == DayOfWeek.Monday)) && m == Month.November)
                // Emperor's Birthday (since 1989)
                || ((d == 23 || (d == 24 && w == DayOfWeek.Monday)) && m == Month.December && y >= 1989)
                // Bank Holiday
                || (d == 31 && m == Month.December)
                // "One-shot holidays", compare against date component dn=date.Date
                || dn == MarriageOfPrinceAkihito
                || dn == RitesOfImperialFuneral
                || dn == EnthronementCeremony
                || dn == MarriageOfPrinceAkihito )
                return false;
            return true;
        }

        /// <summary>
        /// Determine Vernal Equinox day for a given year.
        /// </summary>
        /// <param name="year">The year we are interested in.</param>
        /// <returns>Vernal Equinox day in March.</returns>
        private static int VernalEquinox(int year) 
        {
            return _vernalEquinox[year-1900];
        }

        /// <summary>
        /// Determine Autumnal Equinox day for a given year.
        /// </summary>
        /// <param name="year">The year we are interested in.</param>
        /// <returns>Autumnal Equinox day in September.</returns>
        private static int AutumnalEquinox(int year) 
        {
            return _autumnalEquinox[year-1900];
        }

        private static readonly byte[] _vernalEquinox = {
                                                            22, 20, 20, 21, 21, 20, 20, 21, 21, 20,     // 1900-1910
                                                            20, 21, 21, 20, 20, 21, 21, 20, 20, 21,     // 1910-1920
                                                            21, 20, 20, 21, 21, 20, 20, 21, 21, 20,     // 1920-1930
                                                            20, 20, 21, 20, 20, 20, 21, 20, 20, 20,     // 1930-1940
                                                            21, 20, 20, 20, 21, 20, 20, 20, 21, 20,     // 1940-1950
                                                            20, 20, 21, 20, 20, 20, 21, 20, 20, 20,     // 1950-1960
                                                            21, 20, 20, 20, 20, 20, 20, 20, 20, 20,     // 1960-1970
                                                            20, 20, 20, 20, 20, 20, 20, 20, 20, 20,     // 1970-1980
                                                            20, 20, 20, 20, 20, 20, 20, 20, 20, 20,     // 1980-1990
                                                            20, 20, 20, 19, 20, 20, 20, 19, 20, 20,     // 1990-2000
                                                            20, 20, 21, 21, 20, 20, 21, 21, 20, 20,     // 2000-2010
                                                            21, 21, 20, 20, 21, 21, 20, 20, 21, 21,     // 2010-2020
                                                            20, 20, 21, 21, 20, 20, 20, 21, 20, 20,     // 2020-2030
                                                            20, 21, 20, 20, 20, 21, 20, 20, 20, 21,     // 2030-2040
                                                            20, 20, 20, 21, 20, 20, 20, 21, 20, 20,     // 2040-2050
                                                            20, 21, 20, 20, 20, 21, 20, 20, 20, 20,     // 2050-2060
                                                            20, 20, 20, 20, 20, 20, 20, 20, 20, 20,     // 2060-2070
                                                            20, 20, 20, 20, 20, 20, 20, 20, 20, 20,     // 2070-2080
                                                            20, 20, 20, 20, 20, 20, 20, 20, 20, 20,     // 2080-2090
                                                            20, 20, 19, 20, 20, 20, 19, 20, 20, 20    // 2090-2100
                                                        };

        private static readonly byte[] _autumnalEquinox = {
                                                              24, 23, 23, 23, 23, 23, 23, 23, 23, 23,     // 1900-1910
                                                              23, 23, 23, 23, 23, 23, 23, 22, 23, 23,     // 1910-1920
                                                              23, 22, 23, 23, 23, 22, 23, 23, 23, 22,     // 1920-1930
                                                              23, 23, 23, 22, 23, 23, 23, 22, 23, 23,     // 1930-1940
                                                              23, 22, 23, 23, 23, 22, 23, 23, 23, 22,     // 1940-1950
                                                              22, 23, 23, 22, 22, 23, 23, 22, 22, 23,     // 1950-1960
                                                              23, 22, 22, 23, 23, 22, 22, 23, 23, 22,     // 1960-1970
                                                              22, 23, 23, 22, 22, 23, 23, 22, 22, 23,     // 1970-1980
                                                              23, 22, 22, 22, 23, 22, 22, 22, 23, 22,     // 1980-1990
                                                              22, 22, 23, 22, 22, 22, 23, 22, 22, 22,     // 1990-2000
                                                              23, 23, 23, 23, 23, 23, 23, 23, 23, 23,     // 2000-2010
                                                              23, 23, 22, 23, 23, 23, 22, 23, 23, 23,     // 2010-2020
                                                              22, 23, 23, 23, 22, 23, 23, 23, 22, 23,     // 2020-2030
                                                              23, 23, 22, 23, 23, 23, 22, 23, 23, 23,     // 2030-2040
                                                              22, 23, 23, 23, 22, 22, 23, 23, 22, 22,     // 2040-2050
                                                              23, 23, 22, 22, 23, 23, 22, 22, 23, 23,     // 2050-2060
                                                              22, 22, 23, 23, 22, 22, 23, 23, 22, 22,     // 2060-2070
                                                              23, 23, 22, 22, 23, 23, 22, 22, 22, 23,     // 2070-2080
                                                              22, 22, 22, 23, 22, 22, 22, 23, 22, 22,     // 2080-2090
                                                              22, 23, 22, 22, 22, 23, 22, 22, 22, 23    // 2090-2100
                                                          };

        ///// <summary>
        ///// Generate equinox table suitable for inclusion into C# code.
        ///// </summary>
        ///// <param name="firstYear">First year in table.</param>
        ///// <param name="lastYear">Last year - not in table.</param>
        ///// <returns>A string containing the generated source code.</returns>
        //public static string EquinoxTables(int firstYear, int lastYear)
        //{

        //    // This patch calculates astronomical equinox date, which is a good
        //    // estimate of official equinox date.
        //    // (by KAWANISHI Tomoya <mailto:tomoya@mm.media.kyoto-u.ac.jp>)
		
        //    const double exact_vernal_equinox_time = 20.69115; // at 2000
        //    const double exact_autumnal_equinox_time = 23.09;
        //    const double diff_per_year = 0.242194;

        //    StringBuilder vernalEquinox =
        //        new StringBuilder("private static byte[] vernalEquinox = {");
        //    StringBuilder autumnalEquinox =
        //        new StringBuilder("private static byte[] autumnalEquinox = {");

        //    for(int yy=firstYear; yy<lastYear; yy+=10)
        //    {
        //        StringBuilder ve10 = new StringBuilder("\r\n    ");
        //        StringBuilder ae10 = new StringBuilder("\r\n    ");

        //        int y=yy;
        //        for(; y<yy+10&&y<lastYear; y++)
        //        {
        //            double moving_amount = (y-2000)*diff_per_year;
        //            int number_of_leap_year = (y-2000)/4+(y-2000)/100-(y-2000)/400;

        //            //  vernal_equinox_day 
        //            int ve = (int)(exact_vernal_equinox_time + moving_amount - number_of_leap_year);
        //            ve10.Append(ve);

        //            // autumnal_equinox_day
        //            int ae = (int)(exact_autumnal_equinox_time + moving_amount - number_of_leap_year);
        //            ae10.Append(ae);

        //            if(y<lastYear-1)
        //            {
        //                ve10.Append(", ");
        //                ae10.Append(", ");
        //            }
        //        }
        //        vernalEquinox.AppendFormat("{0}    // {1}-{2}", ve10, yy, y-1);
        //        autumnalEquinox.AppendFormat("{0}    // {1}-{2}", ae10, yy, y-1);
        //    }

        //    return String.Format("{0}\r\n}};\r\n\r\n{1}\r\n}};\r\n",
        //        vernalEquinox, autumnalEquinox);
        //}
    }
}