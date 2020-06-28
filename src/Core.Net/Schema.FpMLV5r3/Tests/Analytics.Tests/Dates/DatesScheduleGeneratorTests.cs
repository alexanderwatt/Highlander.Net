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

#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Dates
{
    [TestClass]
    public class DatesScheduleGeneratorTests
    {
        [TestMethod]
        public void DatesScheduleGeneratorGetDatesSchedule()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);
            //BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition = GetMetaScheduleDefinition();
            var metaScheduleDefinitionRange = new  List<ThreeStringsRangeItem>();
            foreach (Triplet<Period, Period, RollConventionEnum> triplet in metaScheduleDefinition)
            {
                var threeStringsRangeItem = new ThreeStringsRangeItem
                    {
                        Value1 = triplet.First.ToString(),
                        Value2 = triplet.Second.ToString(),
                        Value3 = triplet.Third.ToString()
                    };
                metaScheduleDefinitionRange.Add(threeStringsRangeItem);
            }
            object o = DateScheduler.GetUnadjustedDatesSchedule(metaScheduleDefinitionRange, startDate, "FOLLOWING");
            //PrintListOfDates(unadjustedDates);
            Debug.Print("Dates:");
//            Debug.Print(ParameterFormatter.FormatObject(o));
        }

        [TestMethod]
        public void DatesScheduleGeneratorGetDatesScheduleNoAdjustment()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);
            //BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition = GetMetaScheduleDefinition();
            var metaScheduleDefinitionRange = new  List<ThreeStringsRangeItem>();
            foreach (Triplet<Period, Period, RollConventionEnum> triplet in metaScheduleDefinition)
            {
                var threeStringsRangeItem = new ThreeStringsRangeItem
                    {
                        Value1 = triplet.First.ToString(),
                        Value2 = triplet.Second.ToString(),
                        Value3 = triplet.Third.ToString()
                    };
                metaScheduleDefinitionRange.Add(threeStringsRangeItem);
            }
            object o = DateScheduler.GetUnadjustedDatesSchedule(metaScheduleDefinitionRange, startDate, "");
            //PrintListOfDates(unadjustedDates);
            Debug.Print("Dates:");
            //          Debug.Print(ParameterFormatter.FormatObject(o));
        }




        internal static List<Triplet<Period,Period,RollConventionEnum>>  GetMetaScheduleDefinition()
        {
            var result = new List<Triplet<Period, Period, RollConventionEnum>>
                {
                    new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("1M"), PeriodHelper.Parse("1Y"),
                                                                    RollConventionEnum.Item17),
                    new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("3M"), PeriodHelper.Parse("5Y"),
                                                                    RollConventionEnum.Item17),
                    new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("6M"), PeriodHelper.Parse("4Y"),
                                                                    RollConventionEnum.Item17)
                };
            return result;
        }
    }
}