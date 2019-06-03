#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Schedulers;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Schedulers
{
    /// <summary>
    /// Simple Date scheduler for coupon periods
    /// </summary>
    public static class AdjustedDateScheduler
    {
        /// <summary>
        /// Adjusteds the dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCalender">The business calenders.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns></returns>
        public static DateTime[] AdjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval,
            string rollConvention, IBusinessCalendar businessCalender, BusinessDayConventionEnum dateAdjustmentConvention)
        {
            DateTime[] dateSchedule = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out _, out _);
            List<DateTime> adjustedDateSchedule = GetAdjustedDateSchedule(dateSchedule, dateAdjustmentConvention, businessCalender);
            return adjustedDateSchedule.ToArray();
        }

        /// <summary>
        /// Adjusteds the dates from effective date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="businessCalender">The business calender.</param>
        /// <param name="dateAdjustmentConvention">The date adjustment convention.</param>
        /// <returns></returns>
        public static DateTime[] AdjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval,
            string rollConvention, IBusinessCalendar businessCalender, BusinessDayConventionEnum dateAdjustmentConvention)
        {
            DateTime[] dateSchedule = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out _, out _);
            List<DateTime> adjustedDateSchedule = GetAdjustedDateSchedule(dateSchedule, dateAdjustmentConvention, businessCalender);          
            return adjustedDateSchedule.ToArray();
        }

        /// <summary>
        /// Gets the adjusted calculation period start dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodDate">The first regular period date.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <param name="businessCalendar">The businessCalendar.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <returns>A vertival range of dates.</returns>
        public static DateTime[] GetAdjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate,
                                                                    string periodInterval, string rollConvention, DateTime firstRegularPeriodDate, string stubPeriodType,
                                                                    IBusinessCalendar businessCalendar, string businessDayConvention)
        {
            const string dateToReturn = "unadjustedStartDate";
            StubPeriodTypeEnum? stubType = null;
            if (!string.IsNullOrEmpty(stubPeriodType))
                stubType = (StubPeriodTypeEnum)Enum.Parse(typeof(StubPeriodTypeEnum), stubPeriodType, true);
            var periods = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, firstRegularPeriodDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), stubType);
            var dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(periods, dateToReturn);
            var newDates = new DateTime[dates.Count];
            var index = 0;
            foreach (var date in dates)
            {
                var newDate = BusinessCalendarHelper.Advance(businessCalendar, date, "Calendar", "0D", businessDayConvention);
                newDates[index] = newDate;
                index++;
            }
            var result = newDates;
            return result;
        }

        /// <summary>
        /// Gets the adjusted date schedule.
        /// </summary>
        /// <param name="businessCalendar">The businessCalendar.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <returns></returns>
        public static List<DateTime> GetAdjustedDateSchedule(DateTime effectiveDate, Period intervalToTerminationDate,
                                                             Period periodInterval, BusinessDayAdjustments businessDayAdjustments, IBusinessCalendar businessCalendar)
        {
            List<DateTime> unadjustedPeriodDates = DateScheduler.GetUnadjustedDateSchedule(effectiveDate, intervalToTerminationDate, periodInterval);
            IEnumerable<DateTime> adjustedPeriodDates
                = unadjustedPeriodDates.Select(a => businessCalendar.Roll(a, businessDayAdjustments.businessDayConvention));
            return adjustedPeriodDates.Distinct().ToList();
        }

        /// <summary>
        /// Gets the adjusted date schedule.
        /// </summary>
        /// <param name="unadjustedPeriodDates">The unadjusted Dates.</param>
        /// <param name="businessDayConvention">The business day convention.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        public static List<DateTime> GetAdjustedDateSchedule(IEnumerable<DateTime> unadjustedPeriodDates, BusinessDayConventionEnum businessDayConvention, IBusinessCalendar paymentCalendar)
        {
            IEnumerable<DateTime> adjustedPeriodDates
                = unadjustedPeriodDates.Select(a => paymentCalendar.Roll(a, businessDayConvention));
            return adjustedPeriodDates.ToList();
        }

        /// <summary>
        /// Gets the adjusted date schedule.
        /// </summary>
        /// <param name="unadjustedPeriodDates">The unadjusted Dates.</param>
        /// <param name="relativeDateOffset">The relative Date Offset.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        public static List<DateTime> GetAdjustedDateSchedule(IEnumerable<DateTime> unadjustedPeriodDates, RelativeDateOffset relativeDateOffset, IBusinessCalendar paymentCalendar)
        {
            IEnumerable<DateTime> adjustedPeriodDates
                = unadjustedPeriodDates.Select(a => paymentCalendar.Advance(a, relativeDateOffset, relativeDateOffset.businessDayConvention));
            return adjustedPeriodDates.ToList();
        }
    }
}