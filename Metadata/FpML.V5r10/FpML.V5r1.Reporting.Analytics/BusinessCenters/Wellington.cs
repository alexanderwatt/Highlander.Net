#region Using directives

using System;
using System.Globalization;
using Orion.Analytics.Dates;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// Wellington, New Zealand, calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>Day after New Year's Day, January 2st (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>Anniversary (Wellington) Day, Monday nearest January 22nd</description></item>
    /// <item><description>Waitangi Day. February 6th</description></item>
    /// <item><description>Good Friday</description></item>
    /// <item><description>Easter Monday</description></item>
    /// <item><description>ANZAC Day. April 25th</description></item>
    /// <item><description>Queen's Birthday, first Monday in June</description></item>
    /// <item><description>Labour Day, fourth Monday in October</description></item>
    /// <item><description>Christmas, December 25th (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>Boxing Day, December 26th (possibly moved to Monday or Tuesday)</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.piperpat.co.nz/utility/holidays.html">NZ Public Holidays</seealso>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Wellington : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use Wellington.Instance in .NET applications.") ]
        public Wellington() 
            : base( "Wellington", CultureInfo.CreateSpecificCulture("en-NZ"))
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "NZWE";
        }

        #region FactoryItem pattern

        [ Obsolete() ] // just to ignore the CS0618 warning below
        static Wellington()
        {
            Instance = new Wellington();	// CS0618
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
                // New Year's Day (possibly moved to Monday or Tuesday)
                || ((d == 1 || (d == 3 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.January)
                // Day after New Year's Day (possibly Monday or Tuesday)
                || ((d == 2 || (d == 4 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.January)
                // Anniversary Day, Monday nearest January 22nd
                || ((d >= 19 && d <= 25) && w == DayOfWeek.Monday && m == Month.January)
                // Waitangi Day. February 6th
                || (d == 6 && m == Month.February)
                // Good Friday
                || (dd == em-3)
                // Easter Monday
                || (dd == em)
                // ANZAC Day. April 25th
                || (d == 25 && m == Month.April)
                // Queen's Birthday, first Monday in June
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.June)
                // Labour Day, fourth Monday in October
                || ((d >= 22 && d <= 28) && w == DayOfWeek.Monday && m == Month.October)
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