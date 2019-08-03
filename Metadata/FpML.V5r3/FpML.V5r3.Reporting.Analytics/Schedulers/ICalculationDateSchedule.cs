/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.Analytics.Schedulers
{
    /// <summary>
    /// Defines the date schedule for calculation periods
    /// </summary>
    public interface ICalculationDateSchedule
    {
        /// <summary>
        /// Gets a value indicating whether this instance has initial stub.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has initial stub; otherwise, <c>false</c>.
        /// </value>
        Boolean HasInitialStub { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has final stub.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has final stub; otherwise, <c>false</c>.
        /// </value>
        Boolean HasFinalStub { get; }

        /// <summary>
        /// Gets the period interval.
        /// </summary>
        /// <value>The period interval.</value>
        Period PeriodInterval { get; }

        /// <summary>
        /// Gets the unadjusted start dates.
        /// </summary>
        /// <value>The unadjusted start dates.</value>
        List<DateTime> UnadjustedStartDates { get; }

        /// <summary>
        /// Gets the unadjusted end dates.
        /// </summary>
        /// <value>The unadjusted end dates.</value>
        List<DateTime> UnadjustedEndDates { get; }

        /// <summary>
        /// Gets the roll convention.
        /// </summary>
        /// <value>The roll convention.</value>
        RollConventionEnum RollConvention { get; }

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="calculationPeriodDates">The calculation period dates.</param>
        /// <returns></returns>
        List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(CalculationPeriodDates calculationPeriodDates);

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="periodToTerminationDate">The period to termination date.</param>
        /// <param name="rollPeriodFrequecy">The roll period frequecy.</param>
        /// <returns></returns>
        List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, CalculationPeriodFrequency periodToTerminationDate, CalculationPeriodFrequency rollPeriodFrequecy);

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, Period intervalToTerminationDate, Period periodInterval);

        /// <summary>
        /// Gets the unadjusted calculation date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="regularPeriodStartDate">The regular period start date.</param>
        /// <param name="periodFrequency">The period frequency.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, DateTime terminationDate, DateTime regularPeriodStartDate, CalculationPeriodFrequency periodFrequency, StubPeriodTypeEnum? stubPeriodType);

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
        List<CalculationPeriod> GetUnadjustedCalculationDateSchedule(DateTime effectiveDate, DateTime terminationDate, DateTime regularPeriodStartDate, Period periodInterval, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType);

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
        List<CalculationPeriod> GetUnadjustedCalculationDateScheduleFromTermDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, DateTime lastRegularPeriodEndDate, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType);

        /// <summary>
        /// Gets the unadjusted calculation date schedule from term date.
        /// </summary>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="noOfCouponPeriods">The no of coupon periods.</param>
        /// <returns></returns>
        List<CalculationPeriod> GetUnadjustedCalculationDateScheduleFromTermDate(DateTime terminationDate, Period periodInterval, int noOfCouponPeriods);

        /// <summary>
        /// Gets the unadjusted calculation date schedule from term date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="fullFirstCoupon">if set to <c>true</c> [full first coupon].</param>
        /// <returns></returns>
        List<CalculationPeriod> GetUnadjustedCalculationDateScheduleFromTermDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, Boolean fullFirstCoupon);
    }
}