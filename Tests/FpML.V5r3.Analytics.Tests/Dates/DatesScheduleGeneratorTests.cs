#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Util.Helpers;
using Orion.Analytics.Schedulers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Orion.Analytics.Tests.Dates
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