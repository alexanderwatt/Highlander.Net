using System;
using System.Collections.Generic;

namespace Orion.CalendarEngine.Dates
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILastTradingDate
    {
        /// <summary>
        /// IRH7,IRZ9 (90 day futures)
        /// 
        /// IBH8,IBU9 (30 day futures)
        /// </summary>
        /// <returns></returns>
        /// <param name="referenceDate">
        /// if 2000, Z8 is futures that expires in December 2008. 
        /// if 2010, Z8 is futures that expires in December 2018. 
        /// </param>
        DateTime GetLastTradingDay(DateTime referenceDate);

        //// <summary>
        ///// Gets the last trading day.
        ///// </summary>
        ///// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        ///// <returns></returns>
        //DateTime GetLastTradingDay(bool mainCycle);

        ///// <summary>
        ///// Gets the futures date following the given contract listed in the
        ///// relevant Exchange.
        ///// </summary>
        ///// <param name="referenceDate"></param>
        ///// <returns></returns>
        //DateTime GetLastTradingDay(DateTime referenceDate);

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        DateTime GetLastTradingDay(int month, int year);

        ///// <summary>
        ///// Gets the futures date following the given IMM contract listed in the
        ///// relevant Exchange.
        ///// </summary>
        ///// <param name="futurescode"></param>
        ///// <param name="referenceDate"></param>
        ///// <returns></returns>
        //DateTime GetLastTradingDay(string futurescode, DateTime referenceDate);

        /// <summary>
        /// Gets the last trading day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="mainCycle">if set to <c>true</c> [main cycle].</param>
        /// <returns></returns>
        List<DateTime> GetLastTradingDays(int year, bool mainCycle);

        /// <summary>
        /// Is the date the last trading date for a contract in the cycle.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="mainCycle"></param>
        /// <returns></returns>
        bool isLastTradingDate(DateTime d, bool mainCycle);

        /// <summary>
        /// next futures date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// relevant Exchange.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="mainCycle"></param>
        /// <returns>A date.</returns>
        string nextFuturesCode(DateTime date, bool mainCycle);

        /// <summary>
        /// next futures date following the given date
        /// returns the 1st delivery date for next contract listed in the
        /// relevant Exchange.
        /// </summary>
        /// <param name="refDate"></param>
        /// <param name="mainCycle"></param>
        /// <returns>A date.</returns>
        DateTime nextLastTradingDate(DateTime refDate, bool mainCycle);

        ///// <summary>
        ///// next futures date following the given date
        ///// returns the 1st delivery date for next contract listed in the
        ///// relevant Exchange.
        ///// </summary>
        ///// <param name="futurescode"></param>
        ///// <param name="referenceDate"></param>
        ///// <param name="mainCycle"></param>
        ///// <returns>A date.</returns>
        //DateTime nextLastTradingDate(string futurescode, bool mainCycle, DateTime referenceDate);

        ///// <summary>
        ///// Gets the next absolute code for that commodity contract.
        ///// </summary>
        ///// <param name="referenceDate"></param>
        ///// <returns>e.g "Z8"</returns>
        //string GetNextAbsoluteCode(DateTime referenceDate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="iteration">e.g. 3 for the third next from the reference date.</param>
        /// <returns>e.g. H9</returns>
        string GetNthMainCycleCode(DateTime referenceDate, int iteration);

        /// <summary>
        /// Gets the next absolute code for that commodity contract.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns>e.g "Z8"</returns>
        string GetNextAbsoluteMainCycleCode(DateTime referenceDate);
    }
}
