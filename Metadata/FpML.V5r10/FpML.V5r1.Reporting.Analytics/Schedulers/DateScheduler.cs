#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.Analytics.Helpers;

#endregion

namespace Orion.Analytics.Schedulers
{
    /// <summary>
    /// Simple Date scheduler for coupon periods
    /// </summary>
    static public class DateScheduler
    {

        /// <summary>
        /// Gets the unadjusted date schedule.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        public static List<DateTime> GetUnadjustedDateSchedule(DateTime effectiveDate,
            Period intervalToTerminationDate, Period periodInterval)
        {
            Double divisor = IntervalHelper.Div(intervalToTerminationDate, periodInterval);
            // The divisor has to be a whole number (i.e. the period must roll to the term date interval
            if ((divisor % 1) != 0)
            {
                throw new ArithmeticException("The period frequency will not roll to the supplied termination date interval");
            }
            DateTime termDate = intervalToTerminationDate.Add(effectiveDate);
            var periodDates = new List<DateTime>();
            DateTime periodEndDate = effectiveDate;
            int multiplier = periodInterval.GetPeriodMultiplier();
            do
            {
                periodDates.Add(periodEndDate);
                periodEndDate = DateAdd(periodEndDate, periodInterval.period, multiplier);
            }
            while (periodEndDate < termDate);
            periodDates.Add(termDate);
            return periodDates;
        }


        ///<summary>
        ///</summary>
        ///<param name="startDate"></param>
        ///<param name="periodType"></param>
        ///<param name="periodMultiplier"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static DateTime DateAdd(DateTime startDate, PeriodEnum periodType, int periodMultiplier)
        {
            switch (periodType)
            {
                case PeriodEnum.D:
                    return startDate.AddDays(periodMultiplier);
                case PeriodEnum.W:
                    return startDate.AddDays(periodMultiplier * 7);
                case PeriodEnum.M:
                    return startDate.AddMonths(periodMultiplier);
                case PeriodEnum.Y:
                    return startDate.AddYears(periodMultiplier);
                default:
                    throw new ArgumentOutOfRangeException("periodType");
            }
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange">This must have 3 columns: interval, interval, rollconventionenum.</param>
        /// <param name="startDate"></param>
        /// <param name="businessDayAdjustment"></param>
        /// <returns></returns>
        public static DateTime[] GetUnadjustedMetaDatesSchedule(string[,] metaScheduleDefinitionRange,
                                                       DateTime startDate,
                                                       string businessDayAdjustment)
        {
            var metaScheduleDefinition = new List<ThreeStringsRangeItem>();
            var index = metaScheduleDefinitionRange.GetLowerBound(1);
            for (var i = metaScheduleDefinitionRange.GetLowerBound(0); i <= metaScheduleDefinitionRange.GetUpperBound(0); i++)
            {
                var output = new ThreeStringsRangeItem
                {
                    Value1 = metaScheduleDefinitionRange[i, index],
                    Value2 = metaScheduleDefinitionRange[i, index + 1],
                    Value3 = metaScheduleDefinitionRange[i, index + 2]
                };
                metaScheduleDefinition.Add(output);
            }
            var resultAsListOfDates = GetUnadjustedDatesSchedule(metaScheduleDefinition,
                                                       startDate,
                                                       businessDayAdjustment);
            var result = resultAsListOfDates;
            return result;
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange"></param>
        /// <param name="startDate"></param>
        /// <param name="businessDayAdjustment"></param>
        /// <returns></returns>
        public static DateTime[] GetUnadjustedDatesSchedule(List<ThreeStringsRangeItem> metaScheduleDefinitionRange,
                                                  DateTime startDate,
                                                  string businessDayAdjustment)
        {
            var metaScheduleDefinition = metaScheduleDefinitionRange.Select(item => new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse(item.Value1), PeriodHelper.Parse(item.Value2), RollConventionEnumHelper.Parse(item.Value3))).ToList();
            List<DateTime> resultAsListOfDates = DatesMetaSchedule.GetUnadjustedDates(metaScheduleDefinition, startDate);
            var result = resultAsListOfDates.ToArray();
            return result;
        }

        /// <summary>
        /// Gets the unadjusted dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodStartDate">The first regular period start date.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <returns></returns>
        static public DateTime[] UnadjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            var result = GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);          
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="directionDateGeneration"></param>
        ///<param name="effectiveDate"></param>
        ///<param name="terminationDate"></param>
        ///<param name="periodInterval"></param>
        ///<param name="rollDayConvention"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static DateTime[] GetUnajustedDates(int directionDateGeneration, DateTime effectiveDate, DateTime terminationDate, Period periodInterval, RollConventionEnum rollDayConvention)
        {
            DateTime firstRegularPeriodStartDate;
            DateTime lastRegularPeriodEndDate;
            switch (directionDateGeneration)
            {
                case 1:
                    {
                        var result =
                            new List<DateTime>(GetUnadjustedDatesFromEffectiveDate(effectiveDate,
                                                                                                     terminationDate,
                                                                                                     periodInterval,
                                                                                                     rollDayConvention,
                                                                                                     out
                                                                                                         firstRegularPeriodStartDate,
                                                                                                     out
                                                                                                         lastRegularPeriodEndDate));

                        //  remove extra(faulty-generated) date from the end if a swap is shorter than a period
                        //
                        if (periodInterval.Add(effectiveDate) > terminationDate)
                        {
                            result.RemoveAt(result.Count - 1); //remove last element
                        }
                        // if there is a long stub at the back 
                        //
                        if (result[0] != lastRegularPeriodEndDate &&
                            periodInterval.Add(lastRegularPeriodEndDate) < result[result.Count - 1]) // 
                        {
                            //  it it is  long - make it short
                            //
                            DateTime realLastRegularPeriodEndDate = periodInterval.Add(lastRegularPeriodEndDate);

                            result.Insert(result.Count - 1, realLastRegularPeriodEndDate);
                        }
                        return result.ToArray();
                    }
                case 2:
                    {
                        var result =
                            new List<DateTime>(GetUnadjustedDatesFromTerminationDate(effectiveDate,
                                                                                                       terminationDate,
                                                                                                       periodInterval,
                                                                                                       rollDayConvention,
                                                                                                       out
                                                                                                           firstRegularPeriodStartDate,
                                                                                                       out
                                                                                                           lastRegularPeriodEndDate));

                        // if there is a long stub at the front 
                        //
                        if (result[0] != firstRegularPeriodStartDate &&
                            periodInterval.Subtract(firstRegularPeriodStartDate) > result[0])
                            // add a check if a period is short ...
                        {
                            //  it it is long - make it short
                            //
                            DateTime realFirstRegularPeriodStartDate = periodInterval.Subtract(firstRegularPeriodStartDate);

                                result.Insert(1, realFirstRegularPeriodStartDate);
                        }
                        return result.ToArray();
                    }
                default:
                    {
                        const string message =
                            "Argument value is out of range. Only 1 and 2 are the valid values for this argument";

                        throw new ArgumentOutOfRangeException("directionDateGeneration", directionDateGeneration,
                                                              message);
                    }
            }
        }

        /// <summary>
        /// Gets the unadjusted dates from termination date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodStartDate">The first regular period start date.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <returns></returns>
        static public DateTime[] GetUnadjustedDatesFromTerminationDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, RollConventionEnum rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            firstRegularPeriodStartDate = effectiveDate;
            DateTime firstRollDate = terminationDate;
            var periodDates = new List<DateTime> {firstRollDate};
            DateTime nextRollDate = periodInterval.Subtract(firstRollDate);
            DateTime rollConventionDate = ApplyRollConventionToDate(rollConvention, nextRollDate);
            lastRegularPeriodEndDate = DateTime.Compare(nextRollDate, rollConventionDate) == 0 ? firstRollDate : rollConventionDate;
            periodDates.Add(rollConventionDate);
            Boolean reachedEnd = false;
           
            while (!reachedEnd)
            {
                rollConventionDate = periodInterval.Subtract(rollConventionDate);              
                rollConventionDate = ApplyRollConventionToDate(rollConvention, rollConventionDate);
                if (rollConventionDate >= effectiveDate)
                {
                    if (rollConventionDate.Month == effectiveDate.Month && rollConventionDate.Year == effectiveDate.Year)
                    {
                        periodDates.Add(effectiveDate);                      
                        reachedEnd = true;
                        firstRegularPeriodStartDate = rollConventionDate.Day == effectiveDate.Day ? periodDates[periodDates.Count - 1] : periodDates[periodDates.Count - 2];
                    }
                    else
                    {
                        periodDates.Add(rollConventionDate);
                    }
                }
                else
                {
                    reachedEnd = true;                 
                    firstRegularPeriodStartDate = periodDates[periodDates.Count - 1];                  
                    periodDates.Add(effectiveDate);
                }
            }
            periodDates.Sort();            
            return periodDates.ToArray();
        }

        /// <summary>
        /// Gets the unadjusted dates from effective date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodStartDate">The first regular period start date.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <returns></returns>
        static public DateTime[] UnadjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            var result = GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);           
            return result;
        }

        /// <summary>
        /// Gets the unadjusted dates from effective date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodStartDate">The first regular period start date.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <returns></returns>
        static public DateTime[] GetUnadjustedDatesFromEffectiveDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, RollConventionEnum rollConvention, out DateTime firstRegularPeriodStartDate, out DateTime lastRegularPeriodEndDate)
        {
            lastRegularPeriodEndDate = terminationDate;           
            DateTime firstRollDate = effectiveDate;         
            var periodDates = new List<DateTime> {firstRollDate};
            DateTime nextRollDate = periodInterval.Add(firstRollDate);         
            DateTime rollConventionDate = ApplyRollConventionToDate(rollConvention, nextRollDate);
            firstRegularPeriodStartDate = DateTime.Compare(nextRollDate, rollConventionDate) == 0 ? firstRollDate : rollConventionDate;
            periodDates.Add(rollConventionDate);
            Boolean reachedEnd = false;            
            while (!reachedEnd)
            {
                rollConventionDate = periodInterval.Add(rollConventionDate);                
                rollConventionDate = ApplyRollConventionToDate(rollConvention, rollConventionDate);
                if (rollConventionDate <= terminationDate)
                {
                    if (rollConventionDate.Month == terminationDate.Month && rollConventionDate.Year == terminationDate.Year)
                    {
                        periodDates.Add(terminationDate);                        
                        reachedEnd = true;
                        lastRegularPeriodEndDate = rollConventionDate.Day == terminationDate.Day ? periodDates[periodDates.Count - 1] : periodDates[periodDates.Count - 2];
                    }
                    else
                    {
                        periodDates.Add(rollConventionDate);
                    }
                }
                else
                {
                    reachedEnd = true;                    
                    lastRegularPeriodEndDate = periodDates[periodDates.Count - 1];                    
                    periodDates.Add(terminationDate);
                }
            }
            periodDates.Sort();
            return periodDates.ToArray();
        }

        /// <summary>
        /// Applies the roll convention to date.
        /// </summary>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        static public DateTime ApplyRollConventionToDate(RollConventionEnum rollConvention, DateTime referenceDate)
        {
            int result = 0;
            int daysInRefDateMonth = DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);
            if (rollConvention >= RollConventionEnum.Item1 && rollConvention <= RollConventionEnum.Item30)
            {
                int.TryParse(rollConvention.ToString().Replace("Item", string.Empty), out result);
            }
            else if (rollConvention == RollConventionEnum.EOM)
            {
                result = daysInRefDateMonth;
            }
            else if (rollConvention == RollConventionEnum.NONE)
            {
                result = referenceDate.Day;
            }
            if (result == 0)
            {
                throw new ArgumentOutOfRangeException("rollConvention", rollConvention, "supplied value is not supported.");
            }
            if (result > daysInRefDateMonth)
            {
                result = daysInRefDateMonth;
            }
            return new DateTime(referenceDate.Year, referenceDate.Month, result);
        }

        /// <summary>
        /// Gets the unadjusted dates from maturity date. This assumes rgular rolling from maturity date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="lastCouponDate">The last coupon date.</param>
        /// <param name="nextCouponDate">The next coupon date.</param>
        /// <returns></returns>
        static public DateTime[] GetUnadjustedCouponDatesFromMaturityDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, RollConventionEnum rollConvention, out DateTime lastCouponDate, out DateTime nextCouponDate)
        {
            var firstRollDate = terminationDate;
            var periodDates = new List<DateTime> { firstRollDate };
            var nextRollDate = periodInterval.Subtract(firstRollDate);
            var rollConventionDate = ApplyRollConventionToDate(rollConvention, nextRollDate);
            periodDates.Add(rollConventionDate);
            var reachedEnd = false;
            while (!reachedEnd)
            {
                rollConventionDate = periodInterval.Subtract(rollConventionDate);
                rollConventionDate = ApplyRollConventionToDate(rollConvention, rollConventionDate);
                periodDates.Add(rollConventionDate);

                if (rollConventionDate <= effectiveDate)
                {
                    reachedEnd = true;
                }
            }
            periodDates.Sort();
            lastCouponDate = periodDates[0];
            nextCouponDate = periodDates[1];
            return periodDates.ToArray();
        }

        /// <summary>
        /// Gets the unadjusted calculation period start dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="firstRegularPeriodDate">The first regular period date.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns>A vertival range of dates.</returns>
        static public DateTime[] GetUnadjustedCalculationPeriodDates(DateTime effectiveDate, DateTime terminationDate, string periodInterval, string rollConvention, DateTime firstRegularPeriodDate, string stubPeriodType)
        {
            const string dateToReturn = "unadjustedStartDate";
            StubPeriodTypeEnum? stubType = null;
            if (!string.IsNullOrEmpty(stubPeriodType))
                stubType = (StubPeriodTypeEnum)Enum.Parse(typeof(StubPeriodTypeEnum), stubPeriodType, true);

            var periods = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, firstRegularPeriodDate, PeriodHelper.Parse(periodInterval), RollConventionEnumHelper.Parse(rollConvention), stubType);           
            var dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(periods, dateToReturn);            
            var result = dates.ToArray();            
            return result;
        }

        /// <summary>
        /// Returns the unadjusted the calculation dates
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="intervalToFirstRegularPeriodStart">The interval to first regular period start.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        static public DateTime[] UnadjustedCalculationDatesFromFirstRegularInterval(DateTime effectiveDate, DateTime terminationDate, string intervalToFirstRegularPeriodStart, string periodInterval, string rollConvention, string stubPeriodType)
        {
            const string dateToReturn = "unadjustedStartDate";
            StubPeriodTypeEnum? stubType = null;
            if (!string.IsNullOrEmpty(stubPeriodType))
                stubType = (StubPeriodTypeEnum)Enum.Parse(typeof(StubPeriodTypeEnum), stubPeriodType, true);
            var periods = CalculationPeriodHelper.GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, PeriodHelper.Parse(periodInterval), PeriodHelper.Parse(intervalToFirstRegularPeriodStart), RollConventionEnumHelper.Parse(rollConvention), stubType);           
            var dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(periods, dateToReturn);            
            var result = dates.ToArray();           
            return result;
        }
    }
}