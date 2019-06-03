using System;
using Orion.ModelFramework.Business;

namespace Orion.Analytics.DayCounters
{
    ///<summary>
    /// A class representing a daycount used in UK RPI
    /// 
    /// it is calculated as Year(date2) - Year(date1) + (Month(date2) - Month(date1)) / 12
    /// it is equivalent to  
    /// (number of months of the period)/(number of years of the period * 12)
    /// or 30*Int(yearfrac(date1,date2,30/360)/30)
    ///</summary>
    public sealed class ActualMY: DayCounterBase
    {
        /// <summary>
        /// A static instance of this type.
        /// </summary>
        static public readonly ActualMY Instance = new ActualMY();

        ///<summary>
        /// Basic constructor
        ///</summary>
        private ActualMY() : base("ActualMY")
        {
        }

        /// <summary>
        /// The literal name of this day counter.
        /// </summary>
        /// <returns></returns>
        public override string ToFpML()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the period between two dates as a fraction of year.
        /// </summary>
        /// <remarks>
        /// This is the default implementation, which implements year fractions
        /// on a fixed basis. act/act day counters need to override this method.
        /// </remarks>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="refPeriodStart"></param>
        /// <param name="refPeriodEnd"></param>
        /// <returns></returns>
        protected override double YearFractionImpl(DateTime startDate, DateTime endDate, DateTime refPeriodStart,
                                                   DateTime refPeriodEnd)
        {
            double years = endDate.Year - startDate.Year;
            double months = endDate.Month - startDate.Month;
            return years + months/12;
        }
    }
}