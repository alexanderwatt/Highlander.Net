#region Using directives

using System;
using FpML.V5r10.Reporting.ModelFramework.Business;

#endregion

namespace Orion.Analytics.DayCounters
{
    /// <summary>
    /// Actual/Actual (ISDA) day count convention.
    /// </summary>
    /// <seealso href="http://www.isda.org/c_and_a/pdf/mktc1198.pdf">
    /// ISDA: EMU and market conventions
    /// </seealso>
    public sealed class ActualActualISDA : DayCounterBase 
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        public static readonly ActualActualISDA Instance = new ActualActualISDA();

        private ActualActualISDA()
            : base("ActualActualISDA") 
        {}

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            return "ACT/ACT.ISDA";
        }

        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="refPeriodStart"></param>
        /// <param name="refPeriodEnd"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is the default implementation, which implements year fractions
        /// on a fixed basis. act/act day counters need to override this method.
        /// </remarks>
        protected override double YearFractionImpl(
            DateTime startDate, DateTime endDate, 
            DateTime refPeriodStart, DateTime refPeriodEnd) 
        {
            int y1 = startDate.Year, y2 = endDate.Year;
            double dib1 = (DateTime.IsLeapYear(y1) ? 366.0 : 365.0),
                   dib2 = ( DateTime.IsLeapYear(y2) ? 366.0 : 365.0);

            double sum = y2 - y1 - 1;
            sum += ActualDays(startDate, new DateTime(y1+1, 1, 1)) / dib1;
            sum += ActualDays(new DateTime(y2, 1, 1), endDate) / dib2;
            return sum;
        }
    }
}