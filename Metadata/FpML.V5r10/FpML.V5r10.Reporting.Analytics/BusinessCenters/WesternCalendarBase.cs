#region Using directives

using System;
using System.Globalization;
using Orion.Analytics.Dates;
using FpML.V5r10.Reporting.ModelFramework.Business;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// Partial implementation providing the means of
    /// determining the Easter Monday for a given year.
    /// </summary>
    public abstract class WesternCalendarBase : CalendarBase
    {
        /// <summary>
        /// Construct a new BusinessCalendar based on a localized 
        /// culture and its default calendar.
        /// </summary>
        /// <param name="culture">The associated <see cref="System.Globalization.CultureInfo"/>.</param>
        /// <param name="name">The name of this business calendar.</param>
        protected WesternCalendarBase(String name, CultureInfo culture) 
            : base(name, culture, culture.Calendar) 
        {}
		
        #region Business days

        /// <summary>
        /// Returns <c>true</c> iff the date is a business day for the given market.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to check.</param>
        /// <returns><c>true</c> iff the date is a business day for the given market.</returns>
        public override bool IsBusinessDay(DateTime date) 
        {
            return IsBusinessDay( 
                date.Day, (Month)date.Month, date.Year,
                date.DayOfWeek, date.DayOfYear, GetEasterMonday(date.Year)
                );
        }


        /// <summary>
        /// Determines whether [is business day] [the specified day].
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <param name="w">The w.</param>
        /// <param name="dayOfYear">The day of year.</param>
        /// <param name="easterMonday">The easter monday.</param>
        /// <returns>
        /// 	<c>true</c> if [is business day] [the specified day]; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool IsBusinessDay(int day, Month month, int year,
                                              DayOfWeek w, int dayOfYear, int easterMonday);

        #endregion

        #region Easter monday calculations

        /// <summary>
        /// Determine Easter Monday for a given year.
        /// </summary>
        /// <param name="year">The year we are interested in.</param>
        /// <returns>Easter monday expressed relative to first day the of year.</returns>
        protected static int GetEasterMonday(int year) 
        {
            return EasterMonday[ year-1900 ];
        }

        private static readonly byte[] EasterMonday = { 
                                                          107,  98,  90, 103,  95, 114, 106,  91, 111, 102,   // 1900-1909
                                                          87, 107,  99,  83, 103,  95, 115,  99,  91, 111,   // 1910-1919
                                                          96,  87, 107,  92, 112, 103,  95, 108, 100,  91,   // 1920-1929
                                                          111,  96,  88, 107,  92, 112, 104,  88, 108, 100,   // 1930-1939
                                                          85, 104,  96, 116, 101,  92, 112,  97,  89, 108,   // 1940-1949
                                                          100,  85, 105,  96, 109, 101,  93, 112,  97,  89,   // 1950-1959
                                                          109,  93, 113, 105,  90, 109, 101,  86, 106,  97,   // 1960-1969
                                                          89, 102,  94, 113, 105,  90, 110, 101,  86, 106,   // 1970-1979
                                                          98, 110, 102,  94, 114,  98,  90, 110,  95,  86,   // 1980-1989
                                                          106,  91, 111, 102,  94, 107,  99,  90, 103,  95,   // 1990-1999
                                                          115, 106,  91, 111, 103,  87, 107,  99,  84, 103,   // 2000-2009
                                                          95, 115, 100,  91, 111,  96,  88, 107,  92, 112,   // 2010-2019
                                                          104,  95, 108, 100,  92, 111,  96,  88, 108,  92,   // 2020-2029
                                                          112, 104,  89, 108, 100,  85, 105,  96, 116, 101,   // 2030-2039
                                                          93, 112,  97,  89, 109, 100,  85, 105,  97, 109,   // 2040-2049
                                                          101,  93, 113,  97,  89, 109,  94, 113, 105,  90,   // 2050-2059
                                                          110, 101,  86, 106,  98,  89, 102,  94, 114, 105,   // 2060-2069
                                                          90, 110, 102,  86, 106,  98, 111, 102,  94, 107,   // 2070-2079
                                                          99,  90, 110,  95,  87, 106,  91, 111, 103,  94,   // 2080-2089
                                                          107,  99,  91, 103,  95, 115, 107,  91, 111, 103    // 2090-2099
                                                      };

        #endregion

    }
}