/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Schedulers
{
    ///<summary>
    ///</summary>
    public class ResetDatesGenerator
    {
        /// <summary>
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="interestRateStream"></param>
        /// <param name="listCalculationPeriods"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static List<DateTime> GetAdjustedResetDates(ICoreCache cache, InterestRateStream interestRateStream,
            List<CalculationPeriod> listCalculationPeriods, IBusinessCalendar fixingCalendar, string nameSpace)
        {
            var adjustedResetDates = new List<DateTime>();
            ResetDates resetDates = interestRateStream.resetDates;
            Period resetFrequency = IntervalHelper.FromFrequency(resetDates.resetFrequency);
            Period calculationPeriodFrequency = IntervalHelper.FromFrequency(interestRateStream.calculationPeriodDates.calculationPeriodFrequency);
            if (resetFrequency.period != calculationPeriodFrequency.period)
            {
                throw new NotSupportedException(
                    $"Reset period type ({resetFrequency.period}) and calculation period type ({calculationPeriodFrequency.period}) are different. This is not supported.");
            }

            if (int.Parse(resetFrequency.periodMultiplier) != int.Parse(calculationPeriodFrequency.periodMultiplier))
            {
                throw new NotSupportedException(
                    $"Reset period frequency ({resetFrequency.period}) is not equal to calculation period frequency ({calculationPeriodFrequency.period}). This is not supported.");
            }
            BusinessDayAdjustments resetDatesAdjustments = resetDates.resetDatesAdjustments;
            ResetRelativeToEnum resetRelativeTo = resetDates.resetRelativeTo;
            if (fixingCalendar == null)
            {
                fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, resetDatesAdjustments.businessCenters, nameSpace);
            }
            foreach (CalculationPeriod calculationPeriodsInPaymentPeriod in listCalculationPeriods)
            {
                switch (resetRelativeTo)
                {
                    case ResetRelativeToEnum.CalculationPeriodStartDate:
                        {
                            DateTime unadjustedResetDate = calculationPeriodsInPaymentPeriod.unadjustedStartDate;
                            DateTime adjustedResetDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, unadjustedResetDate, resetDatesAdjustments);
                            adjustedResetDates.Add(adjustedResetDate);
                            break;
                        }
                    case ResetRelativeToEnum.CalculationPeriodEndDate:
                        {
                            DateTime unadjustedResetDate = calculationPeriodsInPaymentPeriod.unadjustedEndDate;
                            DateTime adjustedResetDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, unadjustedResetDate, resetDatesAdjustments);
                            adjustedResetDates.Add(adjustedResetDate);
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException("resetRelativeTo");
                        }
                }
            }
            return adjustedResetDates;
        }
    }
}