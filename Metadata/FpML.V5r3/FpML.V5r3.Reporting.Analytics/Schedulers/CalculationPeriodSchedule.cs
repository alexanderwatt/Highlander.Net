/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

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
        private DateTime _effectiveDate;
        private DateTime _termDate;
        private List<CalculationPeriod> _unadjustedDateScheduleList = new List<CalculationPeriod>();

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
                    if (_unadjustedDateScheduleList.Count > 0)
                    {
                        retval = CalculationPeriodHelper.HasInitialStub(_unadjustedDateScheduleList, PeriodInterval);
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
                    if (_unadjustedDateScheduleList.Count > 0)
                    {
                        retval = CalculationPeriodHelper.HasFinalStub(_unadjustedDateScheduleList, PeriodInterval);
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
        public List<DateTime> UnadjustedStartDates => (from period in _unadjustedDateScheduleList
            where period.unadjustedStartDateSpecified
            select period.unadjustedStartDate).ToList();

        /// <summary>
        /// Gets the unadjusted end dates.
        /// </summary>
        /// <value>The unadjusted end dates.</value>
        public List<DateTime> UnadjustedEndDates => (from period in _unadjustedDateScheduleList
            where period.unadjustedEndDateSpecified
            select period.unadjustedEndDate).ToList();

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="calculationPeriodDates">The calculation period dates.</param>
        /// <returns></returns>
        public List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(CalculationPeriodDates calculationPeriodDates)
        {
            CalculationPeriodDates = calculationPeriodDates;
            PeriodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(calculationPeriodDates.calculationPeriodFrequency);
            _effectiveDate = XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(calculationPeriodDates).unadjustedDate.Value;
            _termDate = XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(calculationPeriodDates).unadjustedDate.Value;
            RollConvention = calculationPeriodDates.calculationPeriodFrequency.rollConvention;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(_effectiveDate, _termDate, calculationPeriodDates);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = PeriodInterval.Add(effectiveDate);
            RollConvention = periodFrequecy.rollConvention;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, periodToTerminationDate, periodFrequecy);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = intervalToTerminationDate.Add(effectiveDate);
            RollConvention = RollConventionEnum.NONE;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, intervalToTerminationDate, periodInterval);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = terminationDate;
            RollConvention = periodFrequency.rollConvention;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, regularPeriodStartDate, periodFrequency, stubPeriodType);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = terminationDate;
            RollConvention = rollConvention;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, regularPeriodStartDate, periodInterval, rollConvention, stubPeriodType);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = terminationDate;
            RollConvention = rollConvention;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, periodInterval, rollConvention, stubPeriodType);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = terminationDate;
            RollConvention = rollConvention;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDatesFromTermDate(effectiveDate, terminationDate, periodInterval, lastRegularPeriodEndDate, rollConvention, stubPeriodType);
            return _unadjustedDateScheduleList;
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
            _termDate = terminationDate;
            RollConvention = RollConventionEnum.NONE;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDatesFromTermDate(terminationDate, periodInterval, noOfCouponPeriods);
            return _unadjustedDateScheduleList;
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
            _effectiveDate = effectiveDate;
            _termDate = terminationDate;
            RollConvention = RollConventionEnum.NONE;
            _unadjustedDateScheduleList = CalculationPeriodHelper.GenerateUnadjustedCalculationDatesFromTermDate(effectiveDate, terminationDate, periodInterval, fullFirstCoupon);
            return _unadjustedDateScheduleList;
        }
    }
}