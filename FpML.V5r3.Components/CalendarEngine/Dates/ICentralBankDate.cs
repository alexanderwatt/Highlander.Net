#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Orion.CalendarEngine.Dates
{
    /// <summary>
    /// The base central bank calendar interface
    /// </summary>
    public interface ICentralBankDate
    {
        /// <summary>
        /// Gets the Central Bank days.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        List<DateTime> GetCentralBankDays(int year);

        /// <summary>
        /// Gets the Central Bank day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        DateTime? GetCentralBankDay(int month, int year);

        /// <summary>
        /// Gets the Central Bank days.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns></returns>
        List<DateTime> GetCentralBankDays(DateTime startDate, DateTime endDate, Boolean mainCycle);

        /// <summary>
        /// Gets the central bank days.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="noOfMonths">The no of months.</param>
        /// <returns></returns>
        List<DateTime> GetCentralBankDays(DateTime referenceDate, int noOfMonths);

    }
}