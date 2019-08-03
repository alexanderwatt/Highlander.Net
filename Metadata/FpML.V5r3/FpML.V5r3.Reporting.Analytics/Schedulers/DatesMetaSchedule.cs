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

#region Using Directives

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.Analytics.Schedulers
{
    ///<summary>
    /// A dates meta scheduler.
    ///</summary>
    public class DatesMetaSchedule
    {
        /// <summary>
        /// Returns a list of unadjusted dates produced by a simple date scheduler.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="interval"></param>
        /// <param name="rollConventionEnum"></param>
        /// <param name="backwardGeneration"></param>
        /// <returns></returns>
        public static List<DateTime> GetUnadjustedDates2(DateTime startDate, DateTime endDate,
                                                         Period interval, RollConventionEnum rollConventionEnum,
                                                         bool backwardGeneration)
        {
            var results = new List<DateTime>();
            var monthsInTheInterval = interval.GetPeriodMultiplier();
            if (backwardGeneration)//from end date to start date
            {
                var rollDate = endDate;
                while (rollDate > startDate)
                {
                    results.Add(rollDate);
                    var nextRollDate = rollDate.AddMonths(-monthsInTheInterval);
                    // roll adjust
                    //
                    var rollDateRollConventionAdjusted = RollConventionEnumHelper.AdjustDate(rollConventionEnum, nextRollDate);
                    rollDate = rollDateRollConventionAdjusted;
                }
                results.Add(startDate);
                results.Reverse();
            }
            else//from start date to end date.
            {
                var rollDate = startDate;
                while (rollDate < endDate)
                {
                    results.Add(rollDate);
                    var nextRollDate = rollDate.AddMonths(monthsInTheInterval);
                    // roll adjust
                    //
                    var rollDateRollConventionAdjusted = RollConventionEnumHelper.AdjustDate(rollConventionEnum, nextRollDate);
                    rollDate = rollDateRollConventionAdjusted;
                }
                results.Add(endDate);
            }
            return RemoveDuplicates(results);
        }


        //<Interval, Interval, RollConventionEnum>

        /// <summary>
        /// With custom roll schedule
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="rollsMetaSchedule"></param>
        /// <param name="backwardGeneration"></param>
        /// <returns></returns>
        public static List<DateTime> GetUnadjustedDates3(DateTime startDate, DateTime endDate,
                                                         List<MetaScheduleItem> rollsMetaSchedule,
                                                         bool backwardGeneration)
        {
            var results = new List<DateTime>();
            if (backwardGeneration)//from end date to start date
            {
                //  reverse the meta schedule
                //
                var reversedRollsMetaScheduleParam = new List<MetaScheduleItem>(rollsMetaSchedule);
                reversedRollsMetaScheduleParam.Reverse();
                var offsetFromPrevScheduleItem = 0;
                foreach (var metaScheduleItem in reversedRollsMetaScheduleParam)
                {
                    var numberOfRolls = (int)IntervalHelper.Div(metaScheduleItem.Period, metaScheduleItem.RollFrequency);
                    if (metaScheduleItem.RollFrequency.period != PeriodEnum.M)
                    {
                        throw new System.Exception("Month periods are the only ones supported.");
                    }
                    var rollsEveryNMonths = int.Parse(metaScheduleItem.RollFrequency.periodMultiplier);
                    var rollNumber = 1;
                    var offsetInMonths = 0;
                    while (numberOfRolls-- > 0)
                    {
                        offsetInMonths = rollNumber * rollsEveryNMonths;
                        var rollDateUnadjusted = endDate.AddMonths(-offsetInMonths + offsetFromPrevScheduleItem);
                        var rollDateRollAdjusted = RollConventionEnumHelper.AdjustDate(metaScheduleItem.RollConvention, rollDateUnadjusted);
                        results.Insert(0, rollDateRollAdjusted);
                        ++rollNumber;
                    }
                    offsetFromPrevScheduleItem -= offsetInMonths;
                }

                results.Insert(0, startDate);
                results.Add(endDate);
            }
            else//from start date to end date.
            {
                //DateTime rollDate = startDate;
                results.Add(startDate);
                var offsetFromPrevScheduleItem = 0;
                foreach (var metaScheduleItem in rollsMetaSchedule)
                {
                    var numberOfRolls = (int)IntervalHelper.Div(metaScheduleItem.Period, metaScheduleItem.RollFrequency);
                    if (metaScheduleItem.RollFrequency.period != PeriodEnum.M)
                    {
                        throw new System.Exception("Month periods are the only ones supported.");
                    }
                    var rollsEveryNMonths = int.Parse(metaScheduleItem.RollFrequency.periodMultiplier);
                    var rollNumber = 1;
                    var offsetInMonths = 0;
                    while (numberOfRolls-- > 0)
                    {
                        offsetInMonths = rollNumber * rollsEveryNMonths;
                        var rollDateUnadjusted = startDate.AddMonths(offsetInMonths + +offsetFromPrevScheduleItem);
                        var rollDateRollAdjusted = RollConventionEnumHelper.AdjustDate(metaScheduleItem.RollConvention, rollDateUnadjusted);
                        results.Add(rollDateRollAdjusted);
                        ++rollNumber;
                    }
                    offsetFromPrevScheduleItem += offsetInMonths;
                }
                results.Add(endDate);
            }
            return RemoveDuplicates(results);
        }

        /// <summary>
        /// Removes duplicate dates.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<DateTime> RemoveDuplicates(List<DateTime> input)
        {
            var result = new List<DateTime>();
            foreach (var time in input)
            {
                if (!result.Contains(time))
                {
                    result.Add(time);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the unadjusted dates from a meta date scheduler.
        /// </summary>
        /// <param name="metaScheduleDefinition"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static List<DateTime> GetUnadjustedDates(List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition,
                                                        DateTime startDate)
        {
            var offsetsFromStartDateForRollPeriods = new List<Pair<int, RollConventionEnum>>();
            foreach (var metaScheduleEntry in metaScheduleDefinition)
            {
                var numberOfRolls = (int)IntervalHelper.Div(metaScheduleEntry.Second, metaScheduleEntry.First);
                if (metaScheduleEntry.First.period != PeriodEnum.M)
                {
                    throw new System.Exception("Only month-expressed periods are supported.");
                }
                var rollsEveryNMonths = int.Parse(metaScheduleEntry.First.periodMultiplier);
                while (numberOfRolls-- > 0)
                {
                    var lastOffset = 0;
                    if (offsetsFromStartDateForRollPeriods.Count > 0)
                    {
                        lastOffset = offsetsFromStartDateForRollPeriods[offsetsFromStartDateForRollPeriods.Count - 1].First;
                    }
                    lastOffset += rollsEveryNMonths;
                    offsetsFromStartDateForRollPeriods.Add(new Pair<int, RollConventionEnum>(lastOffset, metaScheduleEntry.Third));
                }
            }
            //  Generates dates from a list of month-expressed offsets.
            //
            var result = (from offset in offsetsFromStartDateForRollPeriods
                          let rollDateUnadjusted = startDate.AddMonths(offset.First)
                          select RollConventionEnumHelper.AdjustDate(offset.Second, rollDateUnadjusted)).ToList();
            return RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the unadjusted dates.
        /// </summary>
        /// <param name="metaScheduleDefinition">The meta schedule definition.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="defaultInterval">The default interval.</param>
        /// <param name="defaultRollConvention">The default roll convention.</param>
        /// <param name="fromStartDate">if set to <c>true</c> [from start date].</param>
        /// <returns></returns>
        public static DateTime[] GetUnadjustedDates
            (List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition
                  , DateTime startDate
                  , DateTime endDate
                  , Period defaultInterval
                  , RollConventionEnum defaultRollConvention
                  , Boolean fromStartDate
            )
        {
            DateTime firstReg;
            DateTime lastReg;
            var result = new List<DateTime>();
            List<DateTime> additionalDates;
            DateTime fromDate = startDate;
            DateTime toDate = endDate;
            int intervalMulitplier = 1;
            if (!fromStartDate)
            {
                fromDate = endDate;
                toDate = startDate;
                intervalMulitplier = intervalMulitplier * -1;
            }
            var offsetsFromStartDateForRollPeriods = new List<Pair<Period, RollConventionEnum>>();
            foreach (var metaScheduleEntry in metaScheduleDefinition)
            {
                var numberOfRolls = (int)IntervalHelper.Div(metaScheduleEntry.Second, metaScheduleEntry.First);
                if (numberOfRolls == 0)
                {
                    throw new System.Exception("Invalid period interval specified. The period interval is greater than the duration interval");
                }
                if (!fromStartDate)
                {
                    metaScheduleEntry.First.periodMultiplier = (Convert.ToInt32(metaScheduleEntry.First.periodMultiplier) * intervalMulitplier).ToString(CultureInfo.InvariantCulture);
                }
                while (numberOfRolls-- > 0)
                {
                    offsetsFromStartDateForRollPeriods.Add(new Pair<Period, RollConventionEnum>(metaScheduleEntry.First, metaScheduleEntry.Third));
                }
            }
            if (offsetsFromStartDateForRollPeriods.Count > 0)
            {
                //  Generates dates from a list of intervals expressed as offsets.
                DateTime referenceDate = fromDate;
                foreach (var offset in offsetsFromStartDateForRollPeriods)
                {
                    if (referenceDate == fromDate) result.Add(fromDate);
                    DateTime rollDateUnadjusted = offset.First.Add(referenceDate);
                    DateTime rollConventionDate = DateScheduler.ApplyRollConventionToDate(offset.Second, rollDateUnadjusted);
                    if ((fromStartDate && rollConventionDate > toDate) || (!fromStartDate && rollConventionDate < toDate))
                    {
                        result.Add(toDate);
                        break;
                    }
                    result.Add(rollConventionDate);
                    referenceDate = rollDateUnadjusted;
                }
                // if end date falls after the meta data schedule provided use the defaults
                if (result.Count > 0)
                {
                    if ((fromStartDate && result[result.Count - 1] < toDate) || (!fromStartDate && result[result.Count - 1] > toDate))
                        if (result[result.Count - 1] < endDate)
                        {
                            additionalDates = fromStartDate ? new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(result[result.Count - 1], toDate, defaultInterval, defaultRollConvention, out firstReg, out lastReg)) : new List<DateTime>(DateScheduler.GetUnadjustedDatesFromTerminationDate(toDate, result[result.Count - 1], defaultInterval, defaultRollConvention, out firstReg, out lastReg));
                            result.AddRange(additionalDates);
                            result.Sort();
                        }
                }
            }
            else
            {
                additionalDates = fromStartDate ? new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(startDate, toDate, defaultInterval, defaultRollConvention, out firstReg, out lastReg)) : new List<DateTime>(DateScheduler.GetUnadjustedDatesFromTerminationDate(startDate, toDate, defaultInterval, defaultRollConvention, out firstReg, out lastReg));
                result.AddRange(additionalDates);
            }
            result = RemoveDuplicates(result);
            result.Sort();
            return result.ToArray();
        }
    }

    /// <summary>
    /// The meta date schedule item.
    /// </summary>
    public class MetaScheduleItem
    {
        /// <summary>
        /// the roll frequency.
        /// </summary>
        public Period RollFrequency;

        /// <summary>
        /// The period.
        /// </summary>
        public Period Period;

        /// <summary>
        /// The roll convention.
        /// </summary>
        public RollConventionEnum RollConvention;//roll day convention
    }
}