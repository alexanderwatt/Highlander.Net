using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Schedulers;
using Orion.Util.Helpers;

namespace Orion.Analytics.Tests.Dates
{
    [TestClass]
    public class DatesMetaSchedulerTests
    {

        [TestMethod]
        public void GetUnadjustedDates()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);

            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(DatesScheduleGeneratorTests.GetMetaScheduleDefinition(), startDate);

            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetUnadjustedDatesfromStartToEndDate()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = new DateTime(startDate.Year + 15, startDate.Month, startDate.Day);

            Debug.Print("Start date: {0}", startDate);
            Debug.Print("End date: {0}", endDate);

            DateTime[] unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(GetMetaScheduleDefinition2(), startDate, endDate, PeriodHelper.Parse("1Y"), RollConventionEnumHelper.Parse("17"), true);

            PrintListOfDates(new List<DateTime>(unadjustedDates));
        }

        [TestMethod]
        public void GetUnadjustedDatesfromEndToStartDate()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = new DateTime(startDate.Year + 15, startDate.Month, startDate.Day);

            Debug.Print("Start date: {0}", startDate.ToShortDateString());
            Debug.Print("End date: {0}", endDate.ToShortDateString());

            DateTime[] unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(GetMetaScheduleDefinition2(), startDate, endDate, PeriodHelper.Parse("3M"), RollConventionEnumHelper.Parse("17"), false);

            PrintListOfDates(new List<DateTime>(unadjustedDates));
        }

        [TestMethod]
        public void GetUnadjustedDates2Forward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);

            Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(startDate.Day.ToString());

            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates2(startDate, endDate, interval, rollConventionEnum, false);

            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetUnadjustedDates3Forward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);

            //Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(startDate.Day.ToString());

            List<MetaScheduleItem> rollsMetaSchedule = new List<MetaScheduleItem>();

            MetaScheduleItem item1 = new MetaScheduleItem();
            item1.Period = IntervalHelper.FromMonths(6);
            item1.RollFrequency = IntervalHelper.FromMonths(1);
            item1.RollConvention = rollConventionEnum;
            rollsMetaSchedule.Add(item1);

            MetaScheduleItem item2 = new MetaScheduleItem();
            item2.Period = IntervalHelper.FromMonths(6);
            item2.RollFrequency = IntervalHelper.FromMonths(3);
            item2.RollConvention = rollConventionEnum;
            rollsMetaSchedule.Add(item2);

            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates3(startDate, endDate, rollsMetaSchedule, false);

            Debug.Print("Meta schedule:");
            //         Debug.Print(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(rollsMetaSchedule)));

            Debug.Print("Start date: {0}", startDate);
            Debug.Print("End date: {0}", endDate);

            Debug.Print("Schedule:");
            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetUnadjustedDates2Backward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);

            Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(endDate.Day.ToString());

            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates2(startDate, endDate, interval, rollConventionEnum, true);

            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetUnadjustedDates3Backward()
        {
            var startDate = new DateTime(2008, 04, 10);
            var endDate = new DateTime(2009, 04, 24);

            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(endDate.Day.ToString());

            var rollsMetaSchedule = new List<MetaScheduleItem>();

            var item1 = new MetaScheduleItem();
            item1.Period = IntervalHelper.FromMonths(6);
            item1.RollFrequency = IntervalHelper.FromMonths(1);
            item1.RollConvention = rollConventionEnum;
            rollsMetaSchedule.Add(item1);

            var item2 = new MetaScheduleItem();
            item2.Period = IntervalHelper.FromMonths(6);
            item2.RollFrequency = IntervalHelper.FromMonths(3);
            item2.RollConvention = rollConventionEnum;
            rollsMetaSchedule.Add(item2);

            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates3(startDate, endDate, rollsMetaSchedule, true);

            Debug.Print("Meta schedule:");
            //        Debug.Print(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(rollsMetaSchedule)));

            Debug.Print("Start date: {0}", startDate);
            Debug.Print("End date: {0}", endDate);

            Debug.Print("Schedule:");
            PrintListOfDates(unadjustedDates);
        }


        private static void PrintListOfDates(List<DateTime> list)
        {
            int number = 1;

            foreach (DateTime time in list)
            {
                Debug.Print("{0}:{1}", number++, time.ToString(CultureInfo.GetCultureInfo("en-AU")));
            }
        }

        private static List<Triplet<Period, Period, RollConventionEnum>> GetMetaScheduleDefinition2()
        {
            var result = new List<Triplet<Period, Period, RollConventionEnum>>
                {
                    new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("1M"), PeriodHelper.Parse("1Y"),
                                                                    RollConventionEnum.Item17),
                    new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("2Y"), PeriodHelper.Parse("8Y"),
                                                                    RollConventionEnum.Item17),
                    new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("6M"), PeriodHelper.Parse("1Y"),
                                                                    RollConventionEnum.Item17)
                };

            return result;
        }
    }
}
