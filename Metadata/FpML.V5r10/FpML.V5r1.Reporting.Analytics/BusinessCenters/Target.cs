#region Using directivess

using System;
using System.Globalization;
using Orion.Analytics.Dates;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.BusinessCenters
{
    /// <summary>
    /// TARGET calendar.
    /// </summary>
    /// <remarks>
    /// Holidays:
    /// <list type="table">
    /// <item><description>Saturdays</description></item>
    /// <item><description>Sundays</description></item>
    /// <item><description>New Year's Day, January 1st</description></item>
    /// <item><description>Good Friday (since 2000)</description></item>
    /// <item><description>Easter Monday (since 2000)</description></item>
    /// <item><description>Labour Day, May 1st (since 2000)</description></item>
    /// <item><description>Christmas, December 25th</description></item>
    /// <item><description>Day of Goodwill, December 26th (since 2000)</description></item>
    /// <item><description>New Year's Eve, December 31st (1998, 1999 and 2001 only)</description></item>
    /// </list>
    /// </remarks>	
    /// <seealso href="http://www.national-holidays.com/">National Holidays (website)</seealso>
    public sealed class Target : WesternCalendarBase
    {
        /// <summary>
        /// Parameterless COM constructor - use <see cref="Instance"/> member instead.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for COM compatibility only.
        /// Use the static member <see cref="Instance"/> to get an object
        /// of this type.
        /// </remarks>
        //[ Obsolete("Use TARGET.Instance in .NET applications.") ]
        public Target() 
            : base( "TARGET", CultureInfo.InvariantCulture)
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "EUTA";
        }

        #region FactoryItem pattern

        [ Obsolete() ] // just to ignore the CS0618 warning below
        static Target()
        {
            Instance = new Target();	// CS0618
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
                // New Year's Day
                || (d == 1 && m == Month.January)
                // Good Friday
                || (dd == em-3 && y >= 2000)
                // Easter Monday
                || (dd == em && y >= 2000)
                // Labour Day
                || (d == 1 && m == Month.May && y >= 2000)
                // Christmas
                || (d == 25 && m == Month.December)
                // Day of Goodwill
                || (d == 26 && m == Month.December && y >= 2000)
                // December 31st, 1998 and 1999 only
                || (d == 31 && m == Month.December && (y == 1998 || y == 1999 || y == 2001)))
                return false;
            return true;
        }

    }
}