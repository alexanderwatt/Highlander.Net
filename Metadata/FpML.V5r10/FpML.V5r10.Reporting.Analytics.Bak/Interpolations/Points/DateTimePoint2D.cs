#region Using directives

using System;
using System.Diagnostics;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.DayCounters;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// A datetime point.
    /// </summary>
    [DebuggerDisplay("Value = {Value}, Name = {Name}")]
    public class DateTimePoint2D : Point2D
    {
        /// <summary>
        /// A datetime point.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="termDate"></param>
        public DateTimePoint2D(DateTime baseDate, DateTime expiryDate, DateTime termDate)
            : this(baseDate, expiryDate, termDate, 0.0)
        {}

        /// <summary>
        /// A datetime point.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="termDate"></param>
        /// <param name="value"></param>
        public DateTimePoint2D(DateTime baseDate, DateTime expiryDate, DateTime termDate, double value)
            : this(baseDate, expiryDate, termDate, Actual365.Instance, value)
        {}

        /// <summary>
        /// A datetime point.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="termDate"></param>
        /// <param name="dayCounter"></param>
        /// <param name="value"></param>
        public DateTimePoint2D(DateTime baseDate, DateTime expiryDate, DateTime termDate, IDayCounter dayCounter, double value)
            : base(dayCounter.YearFraction(baseDate, expiryDate), dayCounter.YearFraction(baseDate, termDate), value)
        {}

        /// <summary>
        /// A datetime point.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="indexTenor"></param>
        /// <param name="value"></param>
        public DateTimePoint2D(DateTime baseDate, DateTime expiryDate, string indexTenor, double value)
            : this(baseDate, expiryDate, PeriodHelper.Parse(indexTenor).Add(expiryDate), value)
        {}

        /// <summary>
        /// A datetime point.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="expiryTenor"></param>
        /// <param name="maturityTenor"></param>
        /// <param name="value"></param>
        public DateTimePoint2D(DateTime baseDate, string expiryTenor, string maturityTenor, double value)
            : this(baseDate, PeriodHelper.Parse(expiryTenor).Add(baseDate), PeriodHelper.Parse(maturityTenor).Add(baseDate), value)
        {}

        /// <summary>
        /// A datetime point.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="expiryTenor"></param>
        /// <param name="maturityTenor"></param>
        public DateTimePoint2D(DateTime baseDate, string expiryTenor, string maturityTenor)
            : this(baseDate, expiryTenor, maturityTenor, 0.0)
        {}
    }

}
