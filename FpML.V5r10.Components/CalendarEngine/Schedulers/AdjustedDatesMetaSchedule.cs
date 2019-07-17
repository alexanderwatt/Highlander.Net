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
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Analytics.Schedulers;
using Orion.Util.Helpers;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CalendarEngine.Schedulers
{
    ///<summary>
    /// A dates meta scheduler.
    ///</summary>
    public class AdjustedDatesMetaSchedule
    {
        ///<summary>
        /// Gets the adjusted dates from a provided date schedule.
        ///</summary>
        ///<param name="metaScheduleDefinition"></param>
        ///<param name="startDate"></param>
        ///<param name="businessDayAdjustments"></param>
        ///<param name="businessCalendar"></param>
        ///<returns></returns>
        public static List<DateTime> GetAdjustedDates(List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition,
                                                        DateTime startDate, BusinessDayAdjustments businessDayAdjustments, IBusinessCalendar businessCalendar)
        {
            var unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(metaScheduleDefinition, startDate);
            return unadjustedDates.Select(date => AdjustedDateHelper.ToAdjustedDate(businessCalendar, date, businessDayAdjustments)).ToList();
        }

        /// <summary>
        /// A simple date scheduler.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="interval"></param>
        /// <param name="rollConventionEnum"></param>
        /// <param name="backwardGeneration"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <param name="businessCalendar"></param>
        /// <returns></returns>
        public static List<DateTime> GetAdjustedDates2(DateTime startDate, DateTime endDate, Period interval, RollConventionEnum rollConventionEnum,
                                                       bool backwardGeneration, BusinessDayAdjustments businessDayAdjustments, IBusinessCalendar businessCalendar)
        {
            var unadjustedDates = DatesMetaSchedule.GetUnadjustedDates2(startDate, endDate, interval, rollConventionEnum, backwardGeneration);
            return unadjustedDates.Select(date => AdjustedDateHelper.ToAdjustedDate(businessCalendar, date, businessDayAdjustments)).ToList();
        }

        /// <summary>
        /// A slightly  ore complicated date scheduler.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="rollsMetaSchedule"></param>
        /// <param name="backwardGeneration"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <param name="businessCalendar"></param>
        /// <returns></returns>
        public static List<DateTime> GetAdjustedDates3(DateTime startDate, DateTime endDate, List<MetaScheduleItem> rollsMetaSchedule,
                                                       bool backwardGeneration, BusinessDayAdjustments businessDayAdjustments, IBusinessCalendar businessCalendar)
        {
            var unadjustedDates = DatesMetaSchedule.GetUnadjustedDates3(startDate, endDate, rollsMetaSchedule, backwardGeneration);
            return unadjustedDates.Select(date => AdjustedDateHelper.ToAdjustedDate(businessCalendar, date, businessDayAdjustments)).ToList();
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinition">This must have 3 columns: interval, interval, roll convention enum.</param>
        /// <param name="startDate"></param>
        /// <param name="businessCalendar"></param>
        /// <param name="calendarAsString"></param>
        /// <param name="businessDayAdjustment"></param>
        /// <returns></returns>
        public static DateTime[] GetMetaDatesSchedule(List<ThreeStringsRangeItem> metaScheduleDefinition,
                                                       DateTime startDate,
                                                       IBusinessCalendar businessCalendar,
                                                       string calendarAsString,
                                                       string businessDayAdjustment)
        {
            var resultAsListOfDates = GetDatesSchedule(metaScheduleDefinition,
                                                       startDate,
                                                       businessCalendar,
                                                       calendarAsString,
                                                       businessDayAdjustment);
            var result = resultAsListOfDates;
            return result;
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange">This must have 3 columns: interval, interval, roll convention enum.</param>
        /// <param name="startDate"></param>
        /// <param name="businessCalendar"></param>
        /// <param name="calendarAsString"></param>
        /// <param name="businessDayAdjustment"></param>
        /// <returns></returns>
        public static DateTime[] GetMetaDatesSchedule(string[,] metaScheduleDefinitionRange,
                                                       DateTime startDate,
                                                       IBusinessCalendar businessCalendar,
                                                       string calendarAsString,
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
            var result = GetMetaDatesSchedule(metaScheduleDefinition,
                                                       startDate,
                                                       businessCalendar,
                                                       calendarAsString,
                                                       businessDayAdjustment);
            return result;
        }

        /// <summary>
        /// Gets a dates schedule.
        /// </summary>
        /// <param name="metaScheduleDefinitionRange"></param>
        /// <param name="startDate"></param>
        /// <param name="calendar"></param>
        /// <param name="businessCentreAsString">This should not be blank.</param>
        /// <param name="businessDayAdjustment">This should not be blank.</param>
        /// <returns></returns>
        public static DateTime[] GetDatesSchedule(List<ThreeStringsRangeItem> metaScheduleDefinitionRange,
                                                  DateTime startDate,
                                                  IBusinessCalendar calendar,
                                                  string businessCentreAsString,
                                                  string businessDayAdjustment)
        {
            var metaScheduleDefinition = metaScheduleDefinitionRange.Select(item => new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse(item.Value1), PeriodHelper.Parse(item.Value2), RollConventionEnumHelper.Parse(item.Value3))).ToList();
            List<DateTime> resultAsListOfDates;
            if (calendar == null)
            {
                resultAsListOfDates = DatesMetaSchedule.GetUnadjustedDates(metaScheduleDefinition, startDate);
            }
            if (String.IsNullOrEmpty(businessCentreAsString) | String.IsNullOrEmpty(businessDayAdjustment))
            {
                resultAsListOfDates = DatesMetaSchedule.GetUnadjustedDates(metaScheduleDefinition, startDate);
            }
            else
            {
                var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayAdjustment, businessCentreAsString);
                resultAsListOfDates = GetAdjustedDates(metaScheduleDefinition, startDate, businessDayAdjustments, calendar);
            }
            var result = resultAsListOfDates.ToArray();
            return result;
        }
    }
}