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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class CalculationPeriodHelper
    {
        /// <summary>
        /// Sets the adjusted dates.
        /// </summary>
        /// <param name="calculationPeriod">The calculation period.</param>
        /// <param name="adjustedStartDate">The adjusted start date.</param>
        /// <param name="adjustedEndDate">The adjusted end date.</param>
        public static void SetAdjustedDates(CalculationPeriod calculationPeriod, DateTime adjustedStartDate, DateTime adjustedEndDate)
        {
            calculationPeriod.adjustedStartDate = adjustedStartDate;
            calculationPeriod.adjustedStartDateSpecified = true;
            calculationPeriod.adjustedEndDate = adjustedEndDate;
            calculationPeriod.adjustedEndDateSpecified = true;
        }

        /// <summary>
        /// Sets the unadjusted dates.
        /// </summary>
        /// <param name="calculationPeriod">The calculation period.</param>
        /// <param name="unadjustedStartDate">The unadjusted start date.</param>
        /// <param name="unadjustedEndDate">The unadjusted end date.</param>
        public static void SetUnadjustedDates(CalculationPeriod calculationPeriod, DateTime unadjustedStartDate, DateTime unadjustedEndDate)
        {
            calculationPeriod.unadjustedStartDate = unadjustedStartDate;
            calculationPeriod.unadjustedStartDateSpecified = true;
            calculationPeriod.unadjustedEndDate = unadjustedEndDate;
            calculationPeriod.unadjustedEndDateSpecified = true;
        }

        /// <summary>
        /// Adds the period.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="numberOfIntervals">The number of intervals.</param>
        /// <returns></returns>
        private static DateTime AddPeriod(DateTime date, Period interval, int numberOfIntervals)
        {
            return interval.Multiply(numberOfIntervals).Add(date);
        }


        /// <summary>
        /// Merges the calculation periods.
        /// </summary>
        /// <param name="backPeriods">The back periods.</param>
        /// <param name="forwardPeriods">The forward periods.</param>
        /// <returns></returns>
        internal static List<CalculationPeriod> MergeCalculationPeriods(List<CalculationPeriod> backPeriods, List<CalculationPeriod> forwardPeriods)
        {
            var mergedPeriods = new List<CalculationPeriod>();
            if (backPeriods.Count > 0)
                mergedPeriods.AddRange(backPeriods);
            if (forwardPeriods.Count > 0)
                mergedPeriods.AddRange(forwardPeriods);
            return mergedPeriods;
        }

        /// <summary>
        /// Creates the unadjusted calculation period.
        /// </summary>
        /// <param name="unadjustedStartDate">The unadjusted start date.</param>
        /// <param name="unadjustedEndDate">The unadjusted end date.</param>
        /// <returns></returns>
        public static CalculationPeriod CreateUnadjustedCalculationPeriod(DateTime unadjustedStartDate, DateTime unadjustedEndDate)
        {
            var calculationPeriod = new CalculationPeriod
                                        {
                                            unadjustedStartDateSpecified = true,
                                            unadjustedEndDateSpecified = true,
                                            unadjustedStartDate = unadjustedStartDate,
                                            unadjustedEndDate = unadjustedEndDate
                                        };
            return calculationPeriod;
        }

        /// <summary>
        /// Calculations the period frequency to interval.
        /// </summary>
        /// <param name="calculationPeriodFrequency">The calculation period frequency.</param>
        /// <returns></returns>
        public static Period CalculationPeriodFrequencyToInterval(CalculationPeriodFrequency calculationPeriodFrequency)
        {
            var periodInterval = new Period
                                     {
                                         period = EnumHelper.Parse<PeriodEnum>(calculationPeriodFrequency.period, true),
                                         periodMultiplier = calculationPeriodFrequency.periodMultiplier
                                     };
            return periodInterval;
        }

        /// <summary>
        /// Sets the initial irregular stub period.
        /// </summary>
        /// <param name="regularPeriods">The regular periods.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="stubType">Type of the stub.</param>
        /// <returns></returns>
        internal static List<CalculationPeriod> SetInitialIrregularStubPeriod(List<CalculationPeriod> regularPeriods, DateTime startDate, StubPeriodTypeEnum? stubType)
        {
            CalculationPeriod firstRegularPeriod = regularPeriods[0];
            if (firstRegularPeriod.unadjustedStartDate > startDate)
            {
                CalculationPeriod initialStub = CreateUnadjustedCalculationPeriod(startDate, firstRegularPeriod.unadjustedEndDate);
                if (stubType == StubPeriodTypeEnum.LongInitial)
                {
                    regularPeriods.Remove(firstRegularPeriod);
                    regularPeriods.Add(initialStub);
                }
                else
                {
                    //initialStub.unadjustedEndDate = firstRegularPeriod.unadjustedStartDate;
                    regularPeriods.Add(initialStub);
                }
            }

            return regularPeriods;
        }

        /// <summary>
        /// Gets the first regular period start date.
        /// </summary>
        /// <param name="calculationPeriodFrequency">The calculation period frequency.</param>
        /// <param name="startDate">The start date.</param>
        /// <returns></returns>
        public static DateTime GetFirstRegularPeriodStartDate(CalculationPeriodFrequency calculationPeriodFrequency, DateTime startDate)
        {
            Period periodInterval = CalculationPeriodFrequencyToInterval(calculationPeriodFrequency);
            return GetFirstRegularPeriodStartDate(periodInterval, calculationPeriodFrequency.rollConvention, startDate);
        }

        /// <summary>
        /// Gets the first regular period start date.
        /// </summary>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="startDate">The start date.</param>
        /// <returns></returns>
        public static DateTime GetFirstRegularPeriodStartDate(Period periodInterval, RollConventionEnum rollConvention, DateTime startDate)
        {
            DateTime advDate = periodInterval.Add(startDate);
            DateTime regularPeriodStartDate = advDate;
            if (rollConvention != RollConventionEnum.NONE)
            {
                regularPeriodStartDate = RollConventionEnumHelper.AdjustDate(rollConvention, advDate);
            }
            return regularPeriodStartDate;
        }

        /// <summary>
        /// Gets the last regular period end date.
        /// </summary>
        /// <param name="frequencyToLastRegularPeriodEndDate">The frequency to last regular period end date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static DateTime GetLastRegularPeriodEndDate(CalculationPeriodFrequency frequencyToLastRegularPeriodEndDate, DateTime endDate)
        {
            Period periodInterval = CalculationPeriodFrequencyToInterval(frequencyToLastRegularPeriodEndDate);
            return GetLastRegularPeriodEndDate(periodInterval, frequencyToLastRegularPeriodEndDate.rollConvention, endDate);
        }

        /// <summary>
        /// Gets the last regular period end date.
        /// </summary>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static DateTime GetLastRegularPeriodEndDate(Period periodInterval, RollConventionEnum rollConvention, DateTime endDate)
        {
            int periodMultiplierAsInt = int.Parse(periodInterval.periodMultiplier);
            if (periodMultiplierAsInt > 0)
                periodMultiplierAsInt = periodMultiplierAsInt * -1;
            periodInterval.periodMultiplier = periodMultiplierAsInt.ToString(CultureInfo.InvariantCulture);
            DateTime advDate = periodInterval.Add(endDate);
            DateTime regularPeriodEndDate = advDate;
            if (rollConvention != RollConventionEnum.NONE)
            {
                regularPeriodEndDate = RollConventionEnumHelper.AdjustDate(rollConvention, advDate);
            }
            return regularPeriodEndDate;
        }


        /// <summary>
        /// Gets the forward regular periods.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="calculationPeriodFrequency">The calculation period frequency.</param>
        /// <param name="hasIrregularPeriod">if set to <c>true</c> [has irregular period].</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GetForwardRegularPeriods(DateTime startDate, DateTime endDate, CalculationPeriodFrequency calculationPeriodFrequency, out Boolean hasIrregularPeriod)
        {
            return GetForwardRegularPeriods(startDate, endDate, CalculationPeriodFrequencyToInterval(calculationPeriodFrequency), calculationPeriodFrequency.rollConvention, out hasIrregularPeriod);
        }

        /// <summary>
        /// Gets the forward regular periods.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="hasIrregularPeriod">if set to <c>true</c> [has irregular period].</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GetForwardRegularPeriods(DateTime startDate, DateTime endDate, Period periodInterval, RollConventionEnum rollConvention, out Boolean hasIrregularPeriod)
        {
            DateTime periodStartDate = startDate;
            hasIrregularPeriod = false;
            DateTime periodEndDate = AddPeriod(periodStartDate, periodInterval, 1);
            var regularPeriods = new List<CalculationPeriod>();
            while (true)
            {
                DateTime unadjustedStartDate = periodStartDate;
                DateTime unadjustedEndDate = periodEndDate;
                if (rollConvention != RollConventionEnum.NONE)
                {
                    unadjustedStartDate = RollConventionEnumHelper.AdjustDate(rollConvention, periodStartDate);
                    unadjustedEndDate = RollConventionEnumHelper.AdjustDate(rollConvention, periodEndDate);
                }
                CalculationPeriod calculationPeriod = CreateUnadjustedCalculationPeriod(unadjustedStartDate, unadjustedEndDate);
                if (calculationPeriod.unadjustedEndDate < endDate)
                {
                    regularPeriods.Add(calculationPeriod);
                    periodStartDate = periodEndDate;
                    periodEndDate = AddPeriod(periodStartDate, periodInterval, 1);
                }
                //last period - is not a stub
                else if (calculationPeriod.unadjustedEndDate == endDate)
                {
                    regularPeriods.Add(calculationPeriod);
                    break;
                }
                else
                {
                    hasIrregularPeriod = true;
                    break;
                }
            }
            return regularPeriods;
        }

        /// <summary>
        /// Gets the backward regular periods.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="calculationPeriodFrequency">The calculation period frequency.</param>
        /// <param name="hasIrregularPeriod">if set to <c>true</c> [has irregular period].</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GetBackwardRegularPeriods(DateTime startDate, DateTime endDate, CalculationPeriodFrequency calculationPeriodFrequency, out Boolean hasIrregularPeriod)
        {
            return GetBackwardRegularPeriods(startDate, endDate, CalculationPeriodFrequencyToInterval(calculationPeriodFrequency), calculationPeriodFrequency.rollConvention, out hasIrregularPeriod);
        }

        /// <summary>
        /// Gets the backward regular periods.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="hasIrregularPeriod">if set to <c>true</c> [has irregular period].</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GetBackwardRegularPeriods(DateTime startDate, DateTime endDate, Period periodInterval, RollConventionEnum rollConvention, out Boolean hasIrregularPeriod)
        {
            DateTime periodEndDate = startDate;
            hasIrregularPeriod = false;
            DateTime periodStartDate = AddPeriod(periodEndDate, periodInterval, -1);
            var regularPeriods = new List<CalculationPeriod>();
            while (true)
            {
                DateTime unadjustedStartDate = periodStartDate;
                DateTime unadjustedEndDate = periodEndDate;
                if (rollConvention != RollConventionEnum.NONE)
                {
                    unadjustedStartDate = RollConventionEnumHelper.AdjustDate(rollConvention, periodStartDate);
                    unadjustedEndDate = RollConventionEnumHelper.AdjustDate(rollConvention, periodEndDate);
                }
                CalculationPeriod calculationPeriod = CreateUnadjustedCalculationPeriod(unadjustedStartDate, unadjustedEndDate);
                if (calculationPeriod.unadjustedStartDate > endDate)
                {
                    regularPeriods.Insert(0, calculationPeriod);
                    periodEndDate = periodStartDate;
                    periodStartDate = AddPeriod(periodEndDate, periodInterval, -1);
                }
                //last period - is not a stub
                else if (calculationPeriod.unadjustedStartDate == endDate)
                {
                    regularPeriods.Insert(0, calculationPeriod);
                    break;
                }
                else
                {
                    hasIrregularPeriod = true;
                    break;
                }
            }
            return regularPeriods;
        }

        /// <summary>
        /// Sets the final irregular stub period.
        /// </summary>
        /// <param name="regularPeriods">The regular periods.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        private static List<CalculationPeriod> SetFinalIrregularStubPeriod(List<CalculationPeriod> regularPeriods, DateTime endDate, StubPeriodTypeEnum? stubPeriodType)
        {
            if (stubPeriodType == null)
                throw new ArgumentNullException(nameof(stubPeriodType));
            CalculationPeriod lastRegularPeriod = regularPeriods[regularPeriods.Count - 1];
            if (lastRegularPeriod.unadjustedEndDate < endDate)
            {
                CalculationPeriod finalStub = CreateUnadjustedCalculationPeriod(lastRegularPeriod.unadjustedStartDate, endDate);
                if (stubPeriodType == StubPeriodTypeEnum.LongFinal)
                {
                    regularPeriods.Remove(lastRegularPeriod);
                    regularPeriods.Add(finalStub);
                }
                else
                {
                    finalStub.unadjustedStartDate = lastRegularPeriod.unadjustedEndDate;
                    regularPeriods.Add(finalStub);
                }
            }
            return regularPeriods;
        }

        /// <summary>
        /// Determines whether [is long stub] [the specified calculation period].
        /// </summary>
        /// <param name="calculationPeriod">The calculation period.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns>
        /// 	<c>true</c> if [is long stub] [the specified calculation period]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsLongStub(CalculationPeriod calculationPeriod, Period periodInterval)
        {
            Boolean retval = false;
            if (calculationPeriod != null)
            {
                DateTime normalEndDate = AddPeriod(calculationPeriod.unadjustedStartDate, periodInterval, 1);
                retval = (calculationPeriod.unadjustedEndDate > normalEndDate);
            }
            return retval;
        }

        /// <summary>
        /// Determines whether [is short stub] [the specified calculation period].
        /// </summary>
        /// <param name="calculationPeriod">The calculation period.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns>
        /// 	<c>true</c> if [is short stub] [the specified calculation period]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsShortStub(CalculationPeriod calculationPeriod, Period periodInterval)
        {
            Boolean retval = false;
            if (calculationPeriod != null)
            {
                DateTime normalEndDate = AddPeriod(calculationPeriod.unadjustedStartDate, periodInterval, 1);
                retval = (calculationPeriod.unadjustedEndDate < normalEndDate);
            }
            return retval;
        }

        /// <summary>
        /// Determines whether [has initial stub] [the specified calculation periods].
        /// </summary>
        /// <param name="calculationPeriods">The calculation periods.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns>
        /// 	<c>true</c> if [has initial stub] [the specified calculation periods]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean HasInitialStub(List<CalculationPeriod> calculationPeriods, Period periodInterval)
        {
            Boolean retval = false;
            if (calculationPeriods != null && calculationPeriods.Count > 0)
            {
                CalculationPeriod initialPeriod = calculationPeriods[0];
                DateTime normalEndDate = AddPeriod(initialPeriod.unadjustedStartDate, periodInterval, 1);
                retval = (initialPeriod.unadjustedEndDate != normalEndDate);
            }
            return retval;
        }

        /// <summary>
        /// Determines whether [has final stub] [the specified calculation periods].
        /// </summary>
        /// <param name="calculationPeriods">The calculation periods.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns>
        /// 	<c>true</c> if [has final stub] [the specified calculation periods]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean HasFinalStub(List<CalculationPeriod> calculationPeriods, Period periodInterval)
        {
            Boolean retval = false;
            if (calculationPeriods != null && calculationPeriods.Count > 0)
            {
                CalculationPeriod finalPeriod = calculationPeriods[calculationPeriods.Count - 1];
                DateTime normalStartDate = AddPeriod(finalPeriod.unadjustedEndDate, periodInterval, -1);
                retval = (finalPeriod.unadjustedStartDate != normalStartDate);
            }
            return retval;
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="calculationPeriodDates">The calculation period dates.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, DateTime terminationDate, CalculationPeriodDates calculationPeriodDates)
        {
            DateTime startDate = effectiveDate;
            DateTime endDate = terminationDate;
            var unadjustedDatePeriods = new List<CalculationPeriod>();
            AdjustableDate firstPeriodStartDate = calculationPeriodDates.firstPeriodStartDate;
            // Use the firstPeriodStartDate as the start date if it has been specified
            if (firstPeriodStartDate?.unadjustedDate != null && firstPeriodStartDate.unadjustedDate.Value != effectiveDate)
                startDate = firstPeriodStartDate.unadjustedDate.Value;
            DateTime firstRegularPeriodDate = startDate;
            if (calculationPeriodDates.firstRegularPeriodStartDateSpecified)
            {
                firstRegularPeriodDate = calculationPeriodDates.firstRegularPeriodStartDate;
            }
            StubPeriodTypeEnum? stubPeriodType = null;
            stubPeriodType = calculationPeriodDates.stubPeriodTypeSpecified ? calculationPeriodDates.stubPeriodType : stubPeriodType;
            if (calculationPeriodDates.firstRegularPeriodStartDateSpecified && calculationPeriodDates.lastRegularPeriodEndDateSpecified)
            {
                //Dates must converge
                List<CalculationPeriod> periods = GetForwardRegularPeriods(calculationPeriodDates.firstRegularPeriodStartDate, endDate, calculationPeriodDates.calculationPeriodFrequency, out _);
                if ((periods.Count > 0) && (periods[periods.Count - 1].unadjustedEndDate != calculationPeriodDates.lastRegularPeriodEndDate))
                {
                    throw new ArgumentOutOfRangeException("Irregular period found between the first regular period start and last regular period end");
                }

                unadjustedDatePeriods = GenerateUnadjustedCalculationDates(startDate, endDate, firstRegularPeriodDate, calculationPeriodDates.calculationPeriodFrequency, stubPeriodType);

            }
            else if (calculationPeriodDates.firstRegularPeriodStartDateSpecified)
            {
                unadjustedDatePeriods = GenerateUnadjustedCalculationDates(startDate, endDate, firstRegularPeriodDate, calculationPeriodDates.calculationPeriodFrequency, stubPeriodType);
            }
            else if (calculationPeriodDates.lastRegularPeriodEndDateSpecified)
            {
                Period periodInterval = CalculationPeriodFrequencyToInterval(calculationPeriodDates.calculationPeriodFrequency);
                unadjustedDatePeriods = GenerateUnadjustedCalculationDatesFromTermDate(startDate, endDate, periodInterval, calculationPeriodDates.lastRegularPeriodEndDate, calculationPeriodDates.calculationPeriodFrequency.rollConvention, stubPeriodType);
            }
            return unadjustedDatePeriods;
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="regularPeriodStartDate">The regular period start date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, DateTime terminationDate, DateTime regularPeriodStartDate, Period periodInterval, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            DateTime startDate = effectiveDate;
            DateTime endDate = terminationDate;
            List<CalculationPeriod> forwardRegularPeriods = GetForwardRegularPeriods(regularPeriodStartDate, endDate, periodInterval, rollConvention, out var bContainsIrregularFinalPeriod);
            if (bContainsIrregularFinalPeriod)
            {
                if (forwardRegularPeriods.Count == 0)
                {
                    CalculationPeriod calculationPeriod = CreateUnadjustedCalculationPeriod(regularPeriodStartDate, endDate);
                    forwardRegularPeriods.Add(calculationPeriod);
                }
                else
                {
                    forwardRegularPeriods = SetFinalIrregularStubPeriod(forwardRegularPeriods, endDate, stubPeriodType ?? StubPeriodTypeEnum.LongFinal);
                }
            }
            var backwardRegularPeriods = new List<CalculationPeriod>();
            if (regularPeriodStartDate > startDate)
            {
                var irregularInitialStubType = StubPeriodTypeEnum.ShortInitial;
                backwardRegularPeriods = GetBackwardRegularPeriods(regularPeriodStartDate, startDate, periodInterval, rollConvention, out var bContainsIrregularInitialPeriod);
                if ((bContainsIrregularInitialPeriod && stubPeriodType != null) && (stubPeriodType != StubPeriodTypeEnum.ShortInitial && stubPeriodType != StubPeriodTypeEnum.LongInitial))
                {
                    DateTime normalPeriodStart = AddPeriod(startDate, periodInterval, 1);
                    if (regularPeriodStartDate > normalPeriodStart)
                        irregularInitialStubType = StubPeriodTypeEnum.LongInitial;
                }
                else if ((bContainsIrregularInitialPeriod && stubPeriodType != null) && (stubPeriodType == StubPeriodTypeEnum.ShortInitial || stubPeriodType == StubPeriodTypeEnum.LongInitial))
                {
                    irregularInitialStubType = (StubPeriodTypeEnum)stubPeriodType;
                }

                if (bContainsIrregularInitialPeriod)
                {
                    if (backwardRegularPeriods.Count == 0)
                    {
                        CalculationPeriod calculationPeriod = CreateUnadjustedCalculationPeriod(startDate, regularPeriodStartDate);
                        backwardRegularPeriods.Add(calculationPeriod);
                    }
                    else
                    {
                        backwardRegularPeriods = SetInitialIrregularStubPeriod(backwardRegularPeriods, startDate, irregularInitialStubType);
                    }
                }
            }
            return MergeCalculationPeriods(backwardRegularPeriods, forwardRegularPeriods);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="regularPeriodStartDate">The regular period start date.</param>
        /// <param name="calculationPeriodFrequency">The calculation period frequency.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, DateTime terminationDate, DateTime regularPeriodStartDate, CalculationPeriodFrequency calculationPeriodFrequency, StubPeriodTypeEnum? stubPeriodType)
        {
            Period periodInterval = CalculationPeriodFrequencyToInterval(calculationPeriodFrequency);
            return GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, regularPeriodStartDate, periodInterval, calculationPeriodFrequency.rollConvention, stubPeriodType);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="periodToTerminationDate">The period to termination date.</param>
        /// <param name="periodFrequency">The period frequency.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, CalculationPeriodFrequency periodToTerminationDate, CalculationPeriodFrequency periodFrequency)
        {
            Period intervalToTerminationDate = CalculationPeriodFrequencyToInterval(periodToTerminationDate);
            Period periodInterval = CalculationPeriodFrequencyToInterval(periodFrequency);
            return GenerateUnadjustedCalculationDates(effectiveDate, intervalToTerminationDate, periodInterval, periodFrequency.rollConvention);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, Period intervalToTerminationDate, Period periodInterval)
        {
            return GenerateUnadjustedCalculationDates(effectiveDate, intervalToTerminationDate, periodInterval, RollConventionEnum.NONE);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="intervalToTerminationDate">The interval to termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, Period intervalToTerminationDate, Period periodInterval, RollConventionEnum rollConvention)
        {
            DateTime startDate = effectiveDate;
            // Adjust the effective date 
            if (rollConvention != RollConventionEnum.NONE)
            {
                startDate = RollConventionEnumHelper.AdjustDate(rollConvention, effectiveDate);
            }            
            Double divisor = IntervalHelper.Div(intervalToTerminationDate, periodInterval); 
            // The divisor has to be a whole number (i.e. the period must roll to the term date interval
            if ((divisor % 1) != 0)
            {
                throw new ArithmeticException("The period frequency will not roll to the supplied termination date interval");
            }
            DateTime terminationDate = intervalToTerminationDate.Add(startDate);
            StubPeriodTypeEnum? stubPeriodType = null;
            return GenerateUnadjustedCalculationDates(startDate, terminationDate, effectiveDate, periodInterval, rollConvention, stubPeriodType);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            DateTime regularPeriodStartDate = GetFirstRegularPeriodStartDate(periodInterval, rollConvention, effectiveDate);
            return GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, regularPeriodStartDate, periodInterval, rollConvention, stubPeriodType);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="intervalToFirstRegularPeriodStartDate">The interval to first regular period start date.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDates(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, Period intervalToFirstRegularPeriodStartDate, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            DateTime firstRegularPeriodStartDate = GetFirstRegularPeriodStartDate(intervalToFirstRegularPeriodStartDate, rollConvention, effectiveDate);
            return GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, firstRegularPeriodStartDate, periodInterval, rollConvention, stubPeriodType);
        }

        /// <summary>
        /// Generates the unadjusted calculation dates from term date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="lastRegularPeriodEndDate">The last regular period end date.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="stubPeriodType">Type of the stub period.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDatesFromTermDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, DateTime lastRegularPeriodEndDate, RollConventionEnum rollConvention, StubPeriodTypeEnum? stubPeriodType)
        {
            var unadjustedPeriodDates = new List<CalculationPeriod>();
            DateTime lastRegularPeriodStartDate = AddPeriod(lastRegularPeriodEndDate, periodInterval, -1);
            if (lastRegularPeriodStartDate > effectiveDate)
            {
                List<CalculationPeriod> regularPeriods = GetBackwardRegularPeriods(lastRegularPeriodStartDate, effectiveDate, periodInterval, rollConvention, out _);

                if (regularPeriods.Count > 0)
                {
                    DateTime firstRegularPeriodStartDate = regularPeriods[0].unadjustedStartDate;
                    unadjustedPeriodDates = GenerateUnadjustedCalculationDates(effectiveDate, terminationDate, firstRegularPeriodStartDate, periodInterval, rollConvention, stubPeriodType);
                }
            }
            return unadjustedPeriodDates;
        }

        /// <summary>
        /// Generates the unadjusted calculation dates from term date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="noOfCouponPeriods">The no of coupon periods.</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDatesFromTermDate(DateTime effectiveDate, Period periodInterval, int noOfCouponPeriods)
        {
            var unadjustedPeriodDates = new List<CalculationPeriod>();
            DateTime unadjustedEndDate = effectiveDate;
            if (noOfCouponPeriods < 1)
            {
                throw new ArgumentException("the number of coupon periods must be greater than zero");
            }
            for (int couponPeriod = 1; couponPeriod <= noOfCouponPeriods; couponPeriod++)
            {
                DateTime unadjustedStartDate = AddPeriod(effectiveDate, periodInterval, (couponPeriod * -1));
                CalculationPeriod calculationPeriod = CreateUnadjustedCalculationPeriod(unadjustedStartDate, unadjustedEndDate);
                unadjustedPeriodDates.Insert(0, calculationPeriod);
                unadjustedEndDate = unadjustedStartDate;
            }
            return unadjustedPeriodDates;
        }

        /// <summary>
        /// Generates the unadjusted calculation dates from term date.
        /// </summary>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="terminationDate">The termination date.</param>
        /// <param name="periodInterval">The period interval.</param>
        /// <param name="fullFirstCoupon">if set to <c>true</c> [full first coupon].</param>
        /// <returns></returns>
        public static List<CalculationPeriod> GenerateUnadjustedCalculationDatesFromTermDate(DateTime effectiveDate, DateTime terminationDate, Period periodInterval, Boolean fullFirstCoupon)
        {
            Boolean bReachedFirstCoupon = false;
            var unadjustedPeriodDates = new List<CalculationPeriod>();
            DateTime unadjustedEndDate = terminationDate;
            int couponPeriod = 1;
            while (!bReachedFirstCoupon)
            {
                DateTime unadjustedStartDate = AddPeriod(terminationDate, periodInterval, (couponPeriod * -1));

                if ((!fullFirstCoupon) && (DateTime.Compare(unadjustedStartDate, effectiveDate) < 0))
                {
                    bReachedFirstCoupon = true;
                    unadjustedStartDate = effectiveDate;
                }
                else if ((!fullFirstCoupon) && (DateTime.Compare(unadjustedStartDate, effectiveDate) == 0))
                {
                    bReachedFirstCoupon = true;
                }
                else if ((DateTime.Compare(unadjustedStartDate, effectiveDate) <= 0))
                {
                    bReachedFirstCoupon = true;
                }

                CalculationPeriod calculationPeriod = CreateUnadjustedCalculationPeriod(unadjustedStartDate, unadjustedEndDate);
                unadjustedPeriodDates.Insert(0, calculationPeriod);
                unadjustedEndDate = unadjustedStartDate;
                couponPeriod++;
            }
            return unadjustedPeriodDates;
        }


        /// <summary>
        /// Gets the value of a property in the supplied calculation periods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="periods">The periods.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static List<T> GetCalculationPeriodsProperty<T>(List<CalculationPeriod> periods, string propertyName)
        {
            return periods.Select(period => GetCalculationPeriodProperty<T>(period, propertyName)).ToList();
        }

        /// <summary>
        /// Gets the value of supplied calculation period property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="period">The period.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static T GetCalculationPeriodProperty<T>(CalculationPeriod period, string propertyName)
        {
            T value = default;
            Type periodType = period.GetType();
            PropertyInfo p = periodType.GetProperty(propertyName);
            if (p != null)
            {
                value = (T)p.GetValue(period, null);
            }
            return value;
        }
    }
}