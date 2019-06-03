#region Using directives

using System;
using System.Diagnostics;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// A datetime point.
    /// </summary>
    [DebuggerDisplay("Value = {Value}, Name = {Name}")]
    public class DateTimePoint1D : Point1D
    {
        ///// <summary>
        ///// A basic ctor.
        ///// </summary>
        ///// <param name="dateTime"></param>
        //public DateTimePoint1D(DateTime dateTime) : this(DateTime.Today, dateTime, 0.0)
        //{}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="termDate"></param>
        public DateTimePoint1D(DateTime baseDate, DateTime termDate)
            : this(baseDate, termDate, 0.0)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime dateTime, double value)
            : this(DateTime.Today, dateTime, value)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="termDate"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime baseDate, DateTime termDate, double value)
            : base(Actual365.Instance.YearFraction(baseDate, termDate), value)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="tenor"></param>
        public DateTimePoint1D(DateTime baseDate, string tenor)
            : this(baseDate, tenor, 0.0)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="tenor"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime baseDate, string tenor, double value)
            : this(baseDate, PeriodHelper.Parse(tenor).Add(baseDate), value)
        {}

        /// <summary>
        /// A basic ctor.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="termDate"></param>
        /// <param name="dayCounter"></param>
        /// <param name="value"></param>
        public DateTimePoint1D(DateTime baseDate, DateTime termDate, IDayCounter dayCounter, double value)
            : base(dayCounter.YearFraction(baseDate, termDate), value)
        {}
    }

}
