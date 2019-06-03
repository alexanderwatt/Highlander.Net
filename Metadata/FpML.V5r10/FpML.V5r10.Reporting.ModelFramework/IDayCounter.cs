#region Using directives

using System;
using FpML.V5r10.Reporting.ModelFramework.Business;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// This interface provides methods for determining the length of a time
    /// period according to given market convention, both as a number
    /// of days and as a year fraction.
    /// </summary>
    /// <remarks>
    /// This interface is mainly for COM interop.
    /// </remarks>
    public interface IDayCounter  
    {
        /// <summary>
        /// A stringified representation of this DayCounter for use with FpML.
        /// </summary>
        /// <remarks>
        /// This method is used for interaction with FpML.
        /// </remarks>
        /// <returns>An FpML String representing the object.</returns>
        string ToFpML();


        /// <summary>
        /// The number of days between two dates.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The number of days</returns>
        int DayCount(DateTime startDate, DateTime endDate);

        /// <summary>
        /// The period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The period between two dates as a fraction of year.</returns>
        double YearFraction(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="refPeriodStart">Begin of the reference period or 0.</param>
        /// <param name="refPeriodEnd">End of the erefernce period or 0.</param>
        /// <returns>The period between two dates as a fraction of year.</returns>
        double YearFraction(DateTime startDate, DateTime endDate, DateTime refPeriodStart, DateTime refPeriodEnd);

        /// <summary>
        /// Day count basis
        /// </summary>
        double Basis { get; }

        /// <summary>
        /// Day count convention enum
        /// </summary>
        DayCountConvention DayCountConvention { get; }
    }
}