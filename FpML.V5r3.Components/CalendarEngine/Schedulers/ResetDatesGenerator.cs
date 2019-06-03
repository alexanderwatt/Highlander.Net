#region Using directives

using System;
using System.Collections.Generic;
using Core.Common;
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
            foreach (CalculationPeriod calculationPeriodsInPamentPeriod in listCalculationPeriods)
            {
                switch (resetRelativeTo)
                {
                    case ResetRelativeToEnum.CalculationPeriodStartDate:
                        {
                            DateTime unadjustedResetDate = calculationPeriodsInPamentPeriod.unadjustedStartDate;
                            DateTime adjustedResetDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, unadjustedResetDate, resetDatesAdjustments);
                            adjustedResetDates.Add(adjustedResetDate);
                            break;
                        }
                    case ResetRelativeToEnum.CalculationPeriodEndDate:
                        {
                            DateTime unadjustedResetDate = calculationPeriodsInPamentPeriod.unadjustedEndDate;
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