#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.Analytics.Schedulers
{
    /// <summary>
    /// Calculation Period Scheduler
    /// </summary>
    public class CalculationPeriodSchedule : ICalculationDateSchedule
    {
        private DateTime EffectiveDate;
        private DateTime TermDate;
        private List<CalculationPeriod> UnadjustedDateScheduleList = new List<CalculationPeriod>();

        ///<summary>
        /// Returns the calculation period dates.
        ///</summary>
        public CalculationPeriodDates CalculationPeriodDates { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has initial stub.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has initial stub; otherwise, <c>false</c>.
        /// </value>
        public Boolean HasInitialStub
        {
            get 
            {
                Boolean retval = false;
                if (PeriodInterval != null)
                {
                    if (UnadjustedDateScheduleList.Count > 0)
                    {
                        retval = CalculationPeriodHelper.HasInitialStub(UnadjustedDateScheduleList, PeriodInterval);
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has final stub.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has final stub; otherwise, <c>false</c>.
        /// </value>
        public Boolean HasFinalStub
        {
            get
            {
                Boolean retval = false;
                if (PeriodInterval != null)
                {
                    if (UnadjustedDateScheduleList.Count > 0)
                    {
                        retval = CalculationPeriodHelper.HasFinalStub(UnadjustedDateScheduleList, PeriodInterval);
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Gets the period interval.
        /// </summary>
        /// <value>The period interval.</value>
        public Period PeriodInterval { get; private set; }

        /// <summary>
        /// Gets the roll convention.
        /// </summary>
        /// <value>The roll convention.</value>
        public RollConventionEnum RollConvention { get; private set; }

        /// <summary>
        /// Gets the unadjusted start dates.
        /// </summary>
        /// <value>The unadjusted start dates.</value>
        public List<DateTime> UnadjustedStartDates
        {
            get
            {
                return (from period in UnadjustedDateScheduleList
                        where period.unadjustedStartDateSpecified
                        select period.unadjustedStartDate).ToList();
            }
        }

        /// <summary>
        /// Gets the unadjusted end dates.
        /// </summary>
        /// <value>The unadjusted end dates.</value>
        public List<DateTime> UnadjustedEndDates
        {
            get
            {
                return (from period in UnadjustedDateScheduleList
                        where period.unadjustedEndDateSpecified
                        select period.unadjustedEndDate).ToList();
            }
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="calculationPeriodDates">The calculation period dates.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(CalculationPeriodDates calculationPeriodDates)
        {
            CalculationPeriodDates = calculationPeriodDates;
            PeriodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(calculationPeriodDates.calculationPeriodFrequency);
            EffectiveDate = XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(calculationPeriodDates).unadjustedDate.Value;
            TermDate = XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(calculationPeriodDates).unadjustedDate.Value;
            RollConvention = calculationPeriodDates.calculationPeriodFrequency.rollConvention;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(EffectiveDate, TermDate, calculationPeriodDates);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="periodToTerminationDate">The period to termination date.</param>
        /// <param name="periodFrequecy">The period frequecy.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, CalculationPeriodFrequency periodToTerminationDate, CalculationPeriodFrequency periodFrequecy)
        {
            CalculationPeriodDates = null;
            PeriodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(periodFrequecy);
            EffectiveDate = effectiveDate;
            TermDate = PeriodInterval.Add(effectiveDate);
            RollConvention = periodFrequecy.rollConvention;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, periodToTerminationDate, periodFrequecy);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, Period intervalToTerminationDate, Period periodInterval)
        {
            CalculationPeriodDates = null;
            PeriodInterval = periodInterval;
            EffectiveDate = effectiveDate;
            TermDate = intervalToTerminationDate.Add(effectiveDate);
            RollConvention = RollConventionEnum.NONE;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, intervalToTerminationDate, periodInterval);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="regularPeriodStartDate">The regular period start date.</param>
        /// <param name="periodFrequency">The period frequency.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, DateTime terminationDate, DateTime regularPeriodStartDate, CalculationPeriodFrequency periodFrequency, StubPeriodTypeEnum? stubPeriodType)
        {
            CalculationPeriodDates = null;
            PeriodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(periodFrequency);
            EffectiveDate = effectiveDate;
            TermDate = terminationDate;
            RollConvention = periodFrequency.rollConvention;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, regularPeriodStartDate, periodFrequency, stubPeriodType);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="regularPeriodStartDate">The regular period start date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, DateTime terminationDate, DateTime regularPeriodStartDate, Period periodInterval, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            CalculationPeriodDates = null;
            PeriodInterval = periodInterval;
            EffectiveDate = effectiveDate;
            TermDate = terminationDate;
            RollConvention = rollConvention;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, regularPeriodStartDate, periodInterval, rollConvention, stubPeriodType);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            CalculationPeriodDates = null;
            PeriodInterval = periodInterval;
            EffectiveDate = effectiveDate;
            TermDate = terminationDate;
            RollConvention = rollConvention;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, periodInterval, rollConvention, stubPeriodType);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule from term date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateScheduleFromTermDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, DateTime lastRegularPeriodEndDate, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            CalculationPeriodDates = null;
            PeriodInterval = periodInterval;
            EffectiveDate = effectiveDate;
            TermDate = terminationDate;
            RollConvention = rollConvention;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDatesFromTermDate(effectiveDate, terminationDate, periodInterval, lastRegularPeriodEndDate, rollConvention, stubPeriodType);
            return UnadjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule from term date.
        /// </summary>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="noOfCouponPeriods">The no of coupon periods.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateScheduleFromTermDate(DateTime terminationDate, Period periodInterval, int noOfCouponPeriods)
        {
            CalculationPeriodDates = null;
            PeriodInterval = periodInterval;
            TermDate = terminationDate;
            RollConvention = RollConventionEnum.NONE;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDatesFromTermDate(terminationDate, periodInterval, noOfCouponPeriods);
            return UnadjustedDateScheduleList;

        }

        /// <summary>
        /// Gets the unadjusted calculation date schedule from term date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="fullFirstCoupon">if set to <c>true</c> [full first coupon].</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateScheduleFromTermDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, Boolean fullFirstCoupon)
        {
            CalculationPeriodDates = null;
            PeriodInterval = periodInterval;
            EffectiveDate = effectiveDate;
            TermDate = terminationDate;
            RollConvention = RollConventionEnum.NONE;
            UnadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDatesFromTermDate(effectiveDate, terminationDate, periodInterval, fullFirstCoupon);
            return UnadjustedDateScheduleList;
        }

    }
}