#region Using directives

using System;
using System.Globalization;
using Orion.Analytics.Dates;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// Sydney calendar (New South Wales, Australia).
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st</description></item>
    /// <item><description>Australia Day, January 26th (possibly moved to Monday)</description></item>
    /// <item><description>Good Friday</description></item>
    /// <item><description>Easter Monday</description></item>
    /// <item><description>ANZAC Day. April 25th (possibly moved to Monday)</description></item>
    /// <item><description>Queen's Birthday, second Monday in June</description></item>
    /// <item><description>Bank Holiday, first Monday in August</description></item>
    /// <item><description>Labour Day, first Monday in October</description></item>
    /// <item><description>Christmas, December 25th (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>Boxing Day, December 26th (possibly moved to Monday or Tuesday)</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Sydney : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Sydney.Instance in .NET applications.") ]
        public Sydney() 
            : base( "Sydney", CultureInfo.CreateSpecificCulture("en-AU"))
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "AUSY";
        }

        #region FactoryItem pattern

        [ Obsolete() ] // just to ignore the CS0618 warning below
        static Sydney()
        {
            Instance = new Sydney();	// CS0618
        }

        /// <summary>
        /// A static instance of this type.
        /// </summary>
        static public readonly IBusinessCalendar Instance;

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
        override protected bool IsBusinessDay(
            int d, Month m, int y,
            DayOfWeek w, int dd, int em )
        {
            if ((w == DayOfWeek.Saturday || w == DayOfWeek.Sunday)

                // 
                || IsMilleniumDay(d, m, y)

                // New Year's Day 1 Jan (might be rolled to 2nd or 3rd of Jan)
                || IsNewYearsDay(d, m, w)
                
                // Australia Day, January 26th (possibly moved to Monday)
                || ((d == 26 || ((d == 27 || d == 28) && w == DayOfWeek.Monday)) && m == Month.January)
                // Good Friday
                || (dd == em-3)
                // Easter Monday
                || (dd == em)
                // ANZAC Day, April 25th (possibly moved to Monday)
                || ((d == 25 || (d == 26 && w == DayOfWeek.Monday)) && m == Month.April)
                // Queen's Birthday, second Monday in June
                || ((d > 7 && d <= 14) && w == DayOfWeek.Monday && m == Month.June)
                // Bank Holiday, first Monday in August
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.August)
                // Labour Day, first Monday in October
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.October)
                // Christmas (possibly moved to Monday or Tuesday)
                || ((d == 25 || (d == 27 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.December)
                // Boxing Day (possibly moved to Monday or Tuesday)
                || ((d == 26 || (d == 28 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.December)
                )
            {
                return false;
            }

            return true;
        }

        private static bool IsMilleniumDay(int d, Month m, int y)
        {
            return d == 31 && m == Month.December && y == 1999; 
        }


        private static bool IsNewYearsDay(int d, Month m, DayOfWeek w)
        {
            if (d == 1 && m == Month.January)
            {
                return true;
            }
            if (d == 2 && m == Month.January && w == DayOfWeek.Monday)//1 jan is on Saturday
            {
                return true;
            }
            if (d == 3 && m == Month.January && w == DayOfWeek.Monday)//1 jan is on Sunday
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        



    }
}