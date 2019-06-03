#region Using directives

using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Schedulers
{
    ///<summary>
    ///</summary>
    public class FixingDatesGenerator
    {
        /// <summary>
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="interestRateStream"></param>
        /// <param name="listAdjustedResetDates"></param>
        /// <param name="businessCalendar"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static List<DateTime> GetAdjustedFixingDates(ICoreCache cache, InterestRateStream interestRateStream, List<DateTime> listAdjustedResetDates, IBusinessCalendar businessCalendar, string nameSpace)
        {
            var result = new List<DateTime>();
            RelativeDateOffset fixingDatesOffset = interestRateStream.resetDates.fixingDates;
            int numberOfDays = int.Parse(fixingDatesOffset.periodMultiplier);
            // Only NON-POSITIVE offset expressed in BUSINESS DAYS is supported.
            //
            if (!(fixingDatesOffset.dayType == DayTypeEnum.Business & fixingDatesOffset.period == PeriodEnum.D & numberOfDays <= 0))
            {
                string exceptionMessage =
                    $"[{fixingDatesOffset.dayType} {fixingDatesOffset.period} {numberOfDays} days] fixing day offset is not supported.";
                throw new NotSupportedException(exceptionMessage);
            }
            var businessDayAdjustments = new BusinessDayAdjustments
                                                                {
                                                                    businessCenters = fixingDatesOffset.businessCenters,
                                                                    businessDayConvention =
                                                                        fixingDatesOffset.businessDayConvention
                                                                };
            if (businessCalendar == null)
            {
                businessCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, businessDayAdjustments.businessCenters, nameSpace);
            }
            foreach (DateTime adjustredResetDate in listAdjustedResetDates)
            {
                int offsetInBusinessDays = numberOfDays;
                DateTime adjustedFixingDate = adjustredResetDate;
                while (offsetInBusinessDays++ < 0)
                {
                    // Adjust fixing date for one days back.
                    //
                    do
                    {
                        adjustedFixingDate = adjustedFixingDate.AddDays(-1);

                    } while (!businessCalendar.IsBusinessDay(adjustedFixingDate));

                    // adjustedFixingDate has been adjusted for 1 BUSINESS DAY
                }
                result.Add(adjustedFixingDate);
            }
            return result;
        }
    }
}