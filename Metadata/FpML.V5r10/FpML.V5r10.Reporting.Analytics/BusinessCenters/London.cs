#region Using directives

using System;
using System.Globalization;
using Orion.Analytics.Dates;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// London calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st (possibly moved to Monday)</description></item>
    /// <item><description>Good Friday</description></item>
    /// <item><description>Easter Monday</description></item>
    /// <item><description>Early Bank Holiday, first Monday of May</description></item>
    /// <item><description>Spring Bank Holiday, last Monday of May</description></item>
    /// <item><description>June 3rd, 2002 only (Golden Jubilee Bank Holiday)</description></item>
    /// <item><description>June 4rd, 2002 only (special Spring Bank Holiday)</description></item>
    /// <item><description>Summer Bank Holiday, last Monday of August</description></item>
    /// <item><description>Christmas, December 25th (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>Boxing Day, December 26th (possibly moved to Monday or Tuesday)</description></item>
    /// <item><description>December 31st, 1999 only</description></item>
    /// </list>
    /// </remarks>
    /// <seealso href="http://www.dti.gov.uk/er/bankhol.htm">Bank and public
    /// holidays in England, Wales and Northern Ireland 2002-2005</seealso>
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class London : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        [ Obsolete("Use London.Instance in .NET applications.") ]
        public London() 
            : base( "London", CultureInfo.CreateSpecificCulture("en-GB"))
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "GBLO";
        }

        #region FactoryItem pattern

        [ Obsolete() ] // just to ignore the CS0618 warning below
        static London()
        {
            Instance = new London();	// CS0618
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
                // Easter Monday
                || (dd == em)
                // first Monday of May (Early May Bank Holiday)
                || (d <= 7 && w == DayOfWeek.Monday && m == Month.May)
                // last Monday of May (Spring Bank Holiday)
                || (d >= 25 && w == DayOfWeek.Monday && m == Month.May && y != 2002)
                // June 3rd, 2002 only (Golden Jubilee Bank Holiday)
                // June 4rd, 2002 only (special Spring Bank Holiday)
                || ((d == 3 || d == 4) && m == Month.June && y == 2002)
                // last Monday of August (Summer Bank Holiday)
                || (d >= 25 && w == DayOfWeek.Monday && m == Month.August)
                // Christmas (possibly moved to Monday or Tuesday)
                || ((d == 25 || (d == 27 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.December)
                // Boxing Day (possibly moved to Monday or Tuesday)
                || ((d == 26 || (d == 28 && (w == DayOfWeek.Monday || w == DayOfWeek.Tuesday))) && m == Month.December)
                // December 31st, 1999 only
                || (d == 31 && m == Month.December && y == 1999)
                )
                return false;
            return true;
        }
    }
}