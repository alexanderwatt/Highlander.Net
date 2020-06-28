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
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.V5r3;
using Highlander.Core.Common;

namespace Highlander.CalendarEngine.V5r3.Schedulers
{
    /// <summary>
    /// Calculation Period Scheduler
    /// </summary>
    public class AdjustedCalculationPeriodSchedule : CalculationPeriodSchedule
    {
        /// <summary>
        /// Gets the adjusted calculation date schedule.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        /// <param name="businessDayAdjustments">The necessary date adjustment details.</param>
        /// <param name="nameSpace">The clients namespace</param>
        public List<CalculationPeriod> GetAdjustedCalculationDateSchedule(ICoreCache cache, DateTime effectiveDate,
            Period intervalToTerminationDate, Period periodInterval, 
            BusinessDayAdjustments businessDayAdjustments, string nameSpace)
        {
            List<CalculationPeriod> adjustedDateScheduleList =
                GetUnadjustedCalculationDateSchedule(effectiveDate, intervalToTerminationDate, periodInterval);
            foreach (CalculationPeriod period in adjustedDateScheduleList)
            {
                period.adjustedStartDateSpecified = true;
                period.adjustedStartDate =
                    AdjustedDateHelper.ToAdjustedDate(cache, period.unadjustedStartDate, businessDayAdjustments, nameSpace);
                period.adjustedEndDateSpecified = true;
                period.adjustedEndDate =
                    AdjustedDateHelper.ToAdjustedDate(cache, period.unadjustedEndDate, businessDayAdjustments, nameSpace);
            }
            return adjustedDateScheduleList;
        }

        /// <summary>
        /// Gets the adjusted calculation date schedule.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        /// <param name="businessDayAdjustments">The necessary date adjustment details.</param>
        /// <param name="discountingTypeEnum"></param>
        /// <param name="nameSpace">The clients namespace</param>
        public List<PaymentCalculationPeriod> GetAdjustedPaymentCalculationDateSchedule(ICoreCache cache, DateTime effectiveDate,
        Period intervalToTerminationDate, Period periodInterval, BusinessDayAdjustments businessDayAdjustments,
        DiscountingTypeEnum discountingTypeEnum, string nameSpace)
        {
            var adjustedPaymentCalculationDateScheduleList =
                new List<PaymentCalculationPeriod>();
            List<CalculationPeriod> adjustedDateScheduleList = GetAdjustedCalculationDateSchedule(cache, effectiveDate,
                                                                                                  intervalToTerminationDate,
                                                                                                  periodInterval,
                                                                                                  businessDayAdjustments, 
                                                                                                  nameSpace);
            foreach (CalculationPeriod period in adjustedDateScheduleList)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod
                                                   {
                                                       adjustedPaymentDate =
                                                           discountingTypeEnum == DiscountingTypeEnum.Standard
                                                               ? period.adjustedEndDate
                                                               : period.adjustedStartDate,
                                                       adjustedPaymentDateSpecified = true
                                                   };
                var calc = new object[1];
                calc[0] = period;
                paymentCalculationPeriod.Items = calc;
                adjustedPaymentCalculationDateScheduleList.Add(paymentCalculationPeriod);
            }
            return adjustedPaymentCalculationDateScheduleList;
        }
    }
}