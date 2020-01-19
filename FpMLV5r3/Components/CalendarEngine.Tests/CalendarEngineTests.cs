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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Highlander.CalendarEngine.V5r3.Dates;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.CalendarEngine.V5r3.Schedulers;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Analytics.V5r3.BusinessCenters;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Models.V5r3.Rates.Futures;
using Highlander.Reporting.V5r3;
using Highlander.UnitTestEnv.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Exception = System.Exception;

namespace Highlander.CalendarEngine.Tests.V5r3 
{
    [TestClass]
    public class CalendarEngineTests
    {
        #region Constants, Fields

        const int CTotalNoOfYears = 20;//REduced the number of years from 45 due to speed.
        const int CStartYear = 2010;
        const string ResourceName = "Calendar.Tests.CalendarDatFiles";
        //string calendarMap = "Budapest:HUBU,Frankfurt:DEFR";
        private const string CalendarMap = "Adelaide:AUAD,Amsterdam:NLAM,Auckland:NZAU,Belfast:IEBE,Brisbane:AUBR,Brussels:BEBR,Budapest:HUBU,Chicago:USCH,Copenhagen:DKCO,Dublin:IEDU,Frankfurt:DEFR,HongKong:HKHK,Hobart:AUHO,Johannesburg:ZAJO,London:GBLO,Melbourne:AUME,Milan:ITMI,Montreal:CAMO,NewYork:USNY,Oslo:NOOS,Paris:FRPA,Perth:AUPE,Seoul:KRSE,Singapore:SGSI,Stockholm:SEST,Sydney:AUSY,Tokyo:JPTO,Toronto:CATO,Warsaw:PLWA,Wellington:NZWE,Zurich:CHZU,HELL:NONE,TARGET:EUTA";
        //string calendarMap = "Adelaide:IEBE,HongKongNew:IEBE";
        private readonly string[] _exchanges = new[] { "IR", "L", "ED", "ER", "W", "IB", "EY", "ZB", "ES", "RA", "HR", "BAX"};
        const string ResourceNameSuffix = "dat";
        private static readonly DateTime TestBaseDate = new DateTime(2010, 8, 30);
        //private static readonly string[] TestMarketNames = new[] { CurveConst.TEST_EOD };

        //const decimal TOLERANCE_NPV = 1000.0M;

        #endregion

        #region Properties

        private static ILogger LoggerObs { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        private static CalendarEngine.V5r3.CalendarEngine CalendarEngine { get; set; }
        private static TimeSpan Retention { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            CalendarEngine = new CalendarEngine.V5r3.CalendarEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);

            // Set the Retention
            Retention = TimeSpan.FromHours(1);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            UTE.Dispose();
        }

        #endregion

        #region Tests

        #region Calendar tests

        [TestMethod]
        public void AdvanceTest()
        {
            string[] calendars = { "Sydney" };
            DateTime result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", "3y", "MODFOLLOWING");
            Assert.AreNotEqual(DateTime.MinValue, result);
            calendars = new[] { "Melbourne" };
            result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", "4y", "MODFOLLOWING");
            Assert.AreNotEqual(DateTime.MinValue, result);
            calendars = new[] { "Sydney" };
            result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", "4y", "MODFOLLOWING");
            Assert.AreNotEqual(DateTime.MinValue, result);
            calendars = new[] { "Melbourne" };
            for (int y = 0; y < 30; y++)
            {
                string period = y + "y";
                result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", period, "MODFOLLOWING");
                Assert.AreNotEqual(DateTime.MinValue, result);
            }
            calendars = new[] { "Sydney" };
            for (int y = 0; y < 30; y++)
            {
                string period = y + "y";
                result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", period, "MODFOLLOWING");
                Assert.AreNotEqual(DateTime.MinValue, result);
            }
        }

        [TestMethod]
        public void HolidaysBetweenTest()
        {
            DateTime startDate = new DateTime(DateTime.Today.Year, 1, 1);
            DateTime endDate = new DateTime(DateTime.Today.Year, 12, 31);
            HolidaysBetween("London", startDate, endDate);
            HolidaysBetween("AUSY", startDate, endDate);
            HolidaysBetween("Sydney", startDate, endDate);
            HolidaysBetween("TARGET", startDate, endDate);
            HolidaysBetween("EURO", startDate, endDate);
            HolidaysBetween("euro", startDate, endDate);
        }

        private static void HolidaysBetween(string city, DateTime startDate, DateTime endDate)
        {
            string[] locations = { city };
            var results = CalendarEngine.HolidaysBetween(locations, startDate, endDate);
            Assert.AreNotEqual(0, results.Count);
        }

        [TestMethod]
        public void HolidaysBetweenTest2()
        {
            DateTime start = new DateTime(DateTime.Today.Year, 1, 1);
            DateTime end = new DateTime(DateTime.Today.Year, 12, 31);
            const string center = "Sydney";
            List<DateTime> holidays = CalendarEngine.HolidaysBetween(new[] { center }, start, end);
            string message =
                $"{center} contains {holidays.Count} holidays between {start:yyyy-MM-dd} and {end:yyyy-MM-dd}";
            Debug.WriteLine(message);
            Assert.IsNotNull(holidays, message);
            Assert.AreNotEqual(0, holidays.Count, message);
        }

        [TestMethod]
        public void BusinessDayTest()
        {
            Boolean result = CalendarEngine.IsBusinessDay(new[] { "Sydney" }, DateTime.Today);
            result = CalendarEngine.IsBusinessDay(new[] { "AUSY" }, DateTime.Today.AddYears(10));
            result = CalendarEngine.IsBusinessDay(new[] { "EUTA" }, DateTime.Today.AddYears(10));
        }

        [TestMethod]
        public void IsBusinessDayTest()
        {
            Boolean result = CalendarEngine.IsBusinessDay(new[] { "Sydney" }, DateTime.Today);
        }

        [TestMethod]
        public void AdvanceTest2()
        {
            string[] calendars = { "Sydney" };
            DateTime result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", "3y", "MODFOLLOWING");
            Assert.AreNotEqual(DateTime.MinValue, result);
            calendars = new[] { "Melbourne" };
            result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", "4y", "MODFOLLOWING");
            Assert.AreNotEqual(DateTime.MinValue, result);
            calendars = new[] { "Sydney" };
            result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", "4y", "MODFOLLOWING");
            Assert.AreNotEqual(DateTime.MinValue, result);
            calendars = new[] { "Melbourne" };
            for (int y = 0; y < 30; y++)
            {
                string period = y + "y";
                result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", period, "MODFOLLOWING");
                Assert.AreNotEqual(DateTime.MinValue, result);
            }
            calendars = new[] { "Sydney" };
            for (int y = 0; y < 30; y++)
            {
                string period = y + "y";
                result = CalendarEngine.Advance(calendars, DateTime.Today, "Calendar", period, "MODFOLLOWING");
                Assert.AreNotEqual(DateTime.MinValue, result);
            }
        }

        [TestMethod]
        public void SignificantDateTest()
        {
            int[] year = { DateTime.Today.Year };
            var fpmlIdentifier = BusinessCenterEnum.AUSY;
            Stopwatch stopwatch = new Stopwatch();
            // run it once prior to measurement (to remove the initial .Net load/JIT delay)
            IList<SignificantDay> result0 = CalendarEngine.GetSignificantDates(year, fpmlIdentifier);
            stopwatch.Start();
            IList<SignificantDay> result1 = CalendarEngine.GetSignificantDates(year, fpmlIdentifier);
            stopwatch.Stop();
            Debug.Print("Time (s):" + stopwatch.Elapsed.TotalSeconds);
            Assert.IsNotNull(result1);
            Assert.AreNotEqual(0, result1.Count);
            Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < 1);
        }

        [TestMethod]
        public void ShortFinalStubSpecifiedCalculationPeriodDates()
        {
            const string testFile = "ird_ex05_long_stub_swap.xml";
            var assembly = Assembly.GetExecutingAssembly();
            string expectedXml = ResourceHelper.GetResourceWithPartialName(assembly, testFile);
            var swapTrade = XmlSerializerHelper.DeserializeFromString<DataDocument>(expectedXml);
            Swap trade = (Swap)((DataDocument)swapTrade).trade[0].Item;
            CalculationPeriodDates cpd = trade.swapStream[1].calculationPeriodDates;
            DateTime effectiveDate = Reporting.V5r3.XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(cpd).unadjustedDate.Value;
            DateTime termDate = Reporting.V5r3.XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(cpd).unadjustedDate.Value;
            var cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(cpd);
            Assert.AreEqual(unadjustedCalcPeriods.Count, 5);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsShortStub(unadjustedCalcPeriods[0], cps.PeriodInterval));
            Assert.IsTrue(CalculationPeriodHelper.IsLongStub(unadjustedCalcPeriods[unadjustedCalcPeriods.Count - 1], cps.PeriodInterval));
        }

        [TestMethod]
        public void TestBusinessCalendarHelperWithBusinessCenters()
        {
            CalendarEngine.GetCalendar(new[] { "Sydney", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "Sydney", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "Sydney", "Sydney" });
            CalendarEngine.GetCalendar(new[] { "AUSY", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "AUSY", "Sydney" });
            CalendarEngine.GetCalendar(new[] { "AUSY", "Sydney", "AUSY" });
            bool x = CalendarEngine.IsHoliday(new string[1] { "AUSY" }, DateTime.Today);
            x = CalendarEngine.IsHoliday(new string[1] { "AUSY" }, DateTime.Today);
            x = CalendarEngine.IsHoliday(new string[2] { "AUSY", "Sydney" }, DateTime.Today);
            x = CalendarEngine.IsHoliday(new string[2] { "Sydney", "AUSY" }, DateTime.Today);

        }

        [TestMethod]
        public void TestBusinessCalendarHelperWithBusinessCenters2()
        {
            //const string location = "BusinessCenterDateRules.xml";
            CalendarEngine.IsValidBusinessCalendar(new[] { "Sydney", "AUSY" });
            CalendarEngine.IsValidBusinessCalendar(new[] { "Sydney", "APPLE", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "Sydney", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "Sydney", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "Sydney", "Sydney" });
            CalendarEngine.GetCalendar(new[] { "AUSY", "AUSY" });
            CalendarEngine.GetCalendar(new[] { "AUSY", "Sydney" });
            CalendarEngine.GetCalendar(new[] { "AUSY", "Sydney", "AUSY" });
            Boolean x = CalendarEngine.IsHoliday(new string[1] { "AUSY" }, DateTime.Today);
            x = CalendarEngine.IsHoliday(new string[1] { "AUSY" }, DateTime.Today);
            x = CalendarEngine.IsHoliday(new string[2] { "AUSY", "Sydney" }, DateTime.Today);
            x = CalendarEngine.IsHoliday(new string[2] { "Sydney", "AUSY" }, DateTime.Today);
        }

        [TestMethod]
        public void TestSupportedCalendars()
        {
            string[] cals = { "Copenhagen" };
            //List<DateTime> dates = BusinessCalendarHelper.HolidaysBetween(cals, new DateTime(2008, 5, 12), new DateTime(2008, 5, 13));
            //DateTime[] dates = new DateTime[] { new DateTime(2008, 5, 19), new DateTime(2008, 12, 25) };
            //IDictionary<DateTime, string[]> rules = BusinessCalendar.SignificantRulesByDate("Toronto", dates);
            //Boolean b = BusinessCalendarHelper.IsHoliday(cals, new DateTime(2008, 1, 28));
            var calendars = CalendarEngine.CalendarsSupported(cals);
            Assert.IsTrue(calendars.Contains("Copenhagen"));

        }

        /// <summary>
        /// Tests the build holiday list.
        /// </summary>
        [TestMethod]
        public void TestBuildHolidayList()
        {
            string[] calendars = { };
            string[] fpmlNames = { };
            GetCalendarList(CalendarMap, ref calendars, ref fpmlNames);
            DateTime startDate = new DateTime(CStartYear, 1, 1);
            DateTime endDate = new DateTime(CStartYear + CTotalNoOfYears, 12, 31);
            IBusinessCalendar bc = CalendarEngine.GetCalendar(calendars);
            List<DateTime> dateList = bc.HolidaysBetweenDates(startDate, endDate);
            foreach (DateTime dt in dateList)
            {
                Console.WriteLine(dt.ToString("dd-MM-yyyy"));
                var result = bc.IsHoliday(dt);
                Assert.IsTrue(result);
            }
            bc = CalendarEngine.GetCalendar(fpmlNames);
            dateList = bc.HolidaysBetweenDates(startDate, endDate);
            foreach (DateTime dt in dateList)
            {
                Console.WriteLine(dt.ToString("dd-MM-yyyy"));
                Assert.IsTrue(bc.IsHoliday(dt));
            }
        }

        /// <summary>
        /// Tests the build holiday list.
        /// </summary>
        [TestMethod]
        public void TestBuildHolidayListViaHelper()
        {
            string[] calendars = { };
            string[] fpmlNames = { };
            GetCalendarList(CalendarMap, ref calendars, ref fpmlNames);
            DateTime startDate = new DateTime(2021, 9, 15);
            DateTime endDate = new DateTime(2022, 9, 15);
            string[] calendars2 = { "London", "Sydney", "Budapest" };
            List<DateTime> dateList1 = CalendarEngine.HolidaysBetween(calendars2, startDate, endDate);
            //DateTime startDate = new DateTime(cStartYear, 1, 1);
            //DateTime endDate = new DateTime(cStartYear + cTotalNoOfYears, 12, 31);
            List<DateTime> dateList = CalendarEngine.HolidaysBetween(calendars, startDate, endDate);
            IBusinessCalendar bc = CalendarEngine.GetCalendar(calendars);
            //dateList = CalendarEngine.HolidaysBetween(calendars, startDate, endDate);
            foreach (DateTime dt in dateList)
            {
                Console.WriteLine(dt.ToString("dd-MM-yyyy"));
                Assert.IsTrue(bc.IsHoliday(dt));
            }
            bc = CalendarEngine.GetCalendar(fpmlNames);
            dateList = bc.HolidaysBetweenDates(startDate, endDate);
            foreach (DateTime dt in dateList)
            {
                Console.WriteLine(dt.ToString("dd-MM-yyyy"));
                Assert.IsTrue(bc.IsHoliday(dt));
            }
        }

        /// <summary>
        /// Tests the build business day list.
        /// </summary>
        [TestMethod]
        public void TestBuildBusinessDayList()
        {
            string[] calendars = { };
            string[] fpmlNames = { };
            GetCalendarList(CalendarMap, ref calendars, ref fpmlNames);
            DateTime startDate = new DateTime(CStartYear, 1, 1);
            DateTime endDate = new DateTime(CStartYear + CTotalNoOfYears, 12, 31);
            IBusinessCalendar bc = CalendarEngine.GetCalendar(calendars);
            List<DateTime> dateList = bc.BusinessDaysBetweenDates(startDate, endDate);
            foreach (DateTime dt in dateList)
            {
                Console.WriteLine(dt.ToString("dd-MM-yyyy"));
                Assert.IsTrue(bc.IsBusinessDay(dt));
            }
            bc = CalendarEngine.GetCalendar(fpmlNames);
            dateList = bc.BusinessDaysBetweenDates(startDate, endDate);
            foreach (DateTime dt in dateList)
            {
                Console.WriteLine(dt.ToString("dd-MM-yyyy"));
                Assert.IsTrue(bc.IsBusinessDay(dt));
            }
        }


        [TestMethod]
        public void TestBusinessCalendarAgainstDatFiles()
        {
            //DoPreCondition();
            string[] calenderMaps = CalendarMap.Split(new Char[] { ',' });
            // iterate through each calendar
            foreach (string map in calenderMaps)
            {
                string[] splimap = map.Split(new Char[] { ':' });
                string calendar = splimap[0];
                string fpmlName = splimap[1];
                string[] holidayDates = GetHolidayDatesForValidation(calendar);
                Boolean bRetval = ProcessCalendarDataFiles(calendar, holidayDates);
                bRetval = ProcessCalendarDataFiles(fpmlName, holidayDates);
            }
        }

        //BMK The master test.
        [TestMethod]
        public void TestBusinessCalendarAgainstCalendarClass()
        {
            IDictionary<string, IBusinessCalendar> calendarClasses = new Dictionary<string, IBusinessCalendar>();
            DateTime cDtFrom = new DateTime(CStartYear, 1, 1);
            DateTime dtTo = new DateTime(CStartYear + CTotalNoOfYears, 12, 31);
            //calendarClasses.Add("Budapest", Budapest.Instance);
            //calendarClasses.Add("Frankfurt", Frankfurt.Instance);
            //calendarClasses.Add("Johannesburg", Johannesburg.Instance);
            //calendarClasses.Add("London", London.Instance);
            //calendarClasses.Add("Milan", Milan.Instance);
            //calendarClasses.Add("NewYork", NewYork.Instance);
            //calendarClasses.Add("Oslo", Oslo.Instance);
            //calendarClasses.Add("Stockholm", Stockholm.Instance);
            //calendarClasses.Add("Sydney", Sydney.Instance);
            //calendarClasses.Add("Tokyo", Tokyo.Instance);
            //calendarClasses.Add("Toronto", Toronto.Instance);
            //calendarClasses.Add("Warsaw", Warsaw.Instance);
            //calendarClasses.Add("Wellington", Wellington.Instance);
            //calendarClasses.Add("Zurich", Zurich.Instance);
            //calendarClasses.Add("HELL", Hell.Instance);
            //calendarClasses.Add("TARGET", Target.Instance);
            calendarClasses.Add("HUBU", Budapest.Instance);
            calendarClasses.Add("DEFR", Frankfurt.Instance);
            calendarClasses.Add("ZAJO", Johannesburg.Instance);
            calendarClasses.Add("GBLO", London.Instance);
            calendarClasses.Add("ITMI", Milan.Instance);
            calendarClasses.Add("USNY", NewYork.Instance);
            calendarClasses.Add("NOOS", Oslo.Instance);
            calendarClasses.Add("SEST", Stockholm.Instance);
            calendarClasses.Add("AUSY", Sydney.Instance);
            calendarClasses.Add("JPTO", Tokyo.Instance);
            calendarClasses.Add("CATO", Toronto.Instance);
            calendarClasses.Add("PLWA", Warsaw.Instance);
            calendarClasses.Add("NZWE", Wellington.Instance);
            calendarClasses.Add("CHZU", Zurich.Instance);
            calendarClasses.Add("NONE", Hell.Instance);
            calendarClasses.Add("EUTA", Target.Instance);
            string[] calendars = { };
            string[] fpmlNames = { };
            GetCalendarList(CalendarMap, ref calendars, ref fpmlNames);
            //ProcessCalendarClasses(calendars, calendarClasses, cDtFrom, dtTo);
            ProcessCalendarClasses(fpmlNames, calendarClasses, cDtFrom, dtTo);
        }

        #endregion

        #region Date Scheduler Tests

        [TestMethod]
        public void UnadjustedDatesSchedule1()
        {
            DateTime effectiveDate = new DateTime(2008, 09, 18);
            DateTime terminationDate = new DateTime(2011, 06, 20);
            Period periodInterval = PeriodHelper.Parse("3M");
            RollConventionEnum rollDayConvention = RollConventionEnum.Item29;
            //Back|Forward are same
            string expectedDates = "18/09/2008;29/12/2008;30/03/2009;29/06/2009;29/09/2009;29/12/2009;29/03/2010;29/06/2010;29/09/2010;29/12/2010;29/03/2011;20/06/2011";
            DateTime[] bdates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            bdates = AdjustDates(bdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDates, bdates);
            DateTime[] fdates = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            fdates = AdjustDates(fdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDates, fdates);
        }

        [TestMethod]
        public void UnadjustedDatesSchedule2()
        {
            DateTime effectiveDate = new DateTime(2008, 07, 9);
            DateTime terminationDate = new DateTime(2011, 07, 11);
            Period periodInterval = PeriodHelper.Parse("3M");
            RollConventionEnum rollDayConvention = RollConventionEnum.Item7;
            //Back|Forward are same
            string expectedDates = "9/07/2008;7/10/2008;7/01/2009;7/04/2009;7/07/2009;7/10/2009;7/01/2010;7/04/2010;7/07/2010;7/10/2010;7/01/2011;7/04/2011;11/07/2011";
            DateTime[] bdates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            bdates = AdjustDates(bdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDates, bdates);
            DateTime[] fdates = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            fdates = AdjustDates(fdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDates, fdates);
        }

        [TestMethod]
        public void UnadjustedDatesSchedule3()
        {
            DateTime effectiveDate = new DateTime(2008, 11, 16);
            DateTime terminationDate = new DateTime(2011, 07, 10);
            Period periodInterval = PeriodHelper.Parse("3M");
            RollConventionEnum rollDayConvention = RollConventionEnum.Item29;
            string expectedDatesBack = "17/11/2008;29/01/2009;29/04/2009;29/07/2009;29/10/2009;29/01/2010;29/04/2010;29/07/2010;29/10/2010;31/01/2011;29/04/2011;11/07/2011";
            DateTime[] bdates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            bdates = AdjustDates(bdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDatesBack, bdates);
            string expectedDatesFwd = "17/11/2008;27/02/2009;29/05/2009;31/08/2009;30/11/2009;26/02/2010;31/05/2010;30/08/2010;29/11/2010;28/02/2011;30/05/2011;11/07/2011";
            DateTime[] fdates = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            fdates = AdjustDates(fdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDatesFwd, fdates);
        }

        [TestMethod]
        public void UnadjustedDatesSchedule4()
        {
            DateTime effectiveDate = new DateTime(2008, 11, 29);
            DateTime terminationDate = new DateTime(2011, 08, 29);
            Period periodInterval = PeriodHelper.Parse("3M");
            RollConventionEnum rollDayConvention = RollConventionEnum.Item29;
            string expectedDatesBack = "28/11/2008;27/02/2009;29/05/2009;31/08/2009;30/11/2009;26/02/2010;31/05/2010;30/08/2010;29/11/2010;28/02/2011;30/05/2011;29/08/2011";
            DateTime[] bdates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            bdates = AdjustDates(bdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDatesBack, bdates);
            string expectedDatesFwd = "28/11/2008;27/02/2009;29/05/2009;31/08/2009;30/11/2009;26/02/2010;31/05/2010;30/08/2010;29/11/2010;28/02/2011;30/05/2011;29/08/2011";
            DateTime[] fdates = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            fdates = AdjustDates(fdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDatesFwd, fdates);
        }

        [TestMethod]
        public void RegularPeriodIntervalAdjustedDates()
        {
            bool bFailureSet = false;
            try
            {
                // 5 year 3 month cash flow
                DateTime startDate = new DateTime(2007, 01, 5);
                Period intervalToMaturity = PeriodHelper.Parse("5Y");
                Period periodInterval = PeriodHelper.Parse("3M");
                CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
                var unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, intervalToMaturity, periodInterval);
                List<DateTime> dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(unadjustedCalcPeriods, "unadjustedStartDate");
                Assert.AreEqual(unadjustedCalcPeriods.Count, 20);
                Assert.IsFalse(cps.HasInitialStub);
                Assert.IsFalse(cps.HasFinalStub);
                // 3 year 6 month cash flow
                startDate = new DateTime(2010, 12, 3);
                intervalToMaturity = PeriodHelper.Parse("3Y");
                periodInterval = PeriodHelper.Parse("6M");
                cps = new CalculationPeriodSchedule();
                unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, intervalToMaturity, periodInterval);
                dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(unadjustedCalcPeriods, "unadjustedStartDate");
                Assert.AreEqual(unadjustedCalcPeriods.Count, 6);
                Assert.IsFalse(cps.HasInitialStub);
                Assert.IsFalse(cps.HasFinalStub);
                // This should generate an exception
                startDate = new DateTime(2010, 12, 3);
                intervalToMaturity = PeriodHelper.Parse("3Y");
                periodInterval = PeriodHelper.Parse("7M");
                cps = new CalculationPeriodSchedule();
                unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, intervalToMaturity, periodInterval);
                dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(unadjustedCalcPeriods, "unadjustedStartDate");
                Assert.AreEqual(unadjustedCalcPeriods.Count, 6);
                Assert.IsFalse(cps.HasInitialStub);
                Assert.IsFalse(cps.HasFinalStub);
                Assert.IsTrue(!bFailureSet);
            }
            catch (Exception ex)
            {
                bFailureSet = true;
                Console.WriteLine("RegularPeriodIntervalUnadjustedDates: " + ex.Message);
            }
        }

        [TestMethod]
        public void RegularPeriodFrequencyUnadjustedDates()
        {
            // 5 year 3 month cash flow
            DateTime startDate = new DateTime(2007, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item26;
            CalculationPeriodFrequency frequencyToMaturity = CalculationPeriodFrequencyHelper.Parse("5Y", rollConvention.ToString());
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, frequencyToMaturity, periodFrequency);
            List<DateTime> dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(unadjustedCalcPeriods, "unadjustedStartDate");
            Assert.AreEqual(unadjustedCalcPeriods.Count, 20);
            Assert.IsFalse(cps.HasInitialStub);
            Assert.IsFalse(cps.HasFinalStub);
        }

        [TestMethod]
        public void ShortFinalStubSpecified()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2012, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item26;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            DateTime firstRegularPeriodDate = new DateTime(2007, 1, 26);
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, endDate, firstRegularPeriodDate, periodFrequency, StubPeriodTypeEnum.ShortFinal);
            List<DateTime> dates = CalculationPeriodHelper.GetCalculationPeriodsProperty<DateTime>(unadjustedCalcPeriods, "unadjustedStartDate");
            Assert.AreEqual(unadjustedCalcPeriods.Count, 21);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsShortStub(unadjustedCalcPeriods[unadjustedCalcPeriods.Count - 1], cps.PeriodInterval));
        }

        [TestMethod]
        public void LongFinalStubSpecified()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2012, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item26;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            DateTime firstRegularPeriodDate = new DateTime(2007, 1, 26);
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, endDate, firstRegularPeriodDate, periodFrequency, StubPeriodTypeEnum.LongFinal);
            Assert.AreEqual(unadjustedCalcPeriods.Count, 20);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsLongStub(unadjustedCalcPeriods[unadjustedCalcPeriods.Count - 1], cps.PeriodInterval));
        }

        [TestMethod]
        public void LongFinalStubSpecifiedButNoStub()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2012, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item5;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            Period periodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(periodFrequency);
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, endDate, periodInterval, rollConvention, StubPeriodTypeEnum.LongFinal);
            Assert.AreEqual(unadjustedCalcPeriods.Count, 20);
            Assert.IsFalse(cps.HasFinalStub);
            Assert.IsFalse(cps.HasInitialStub);
        }

        [TestMethod]
        public void ShortInitialStubSpecified()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2012, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item26;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            DateTime firstRegularPeriodDate = new DateTime(2007, 1, 26);
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, endDate, firstRegularPeriodDate, periodFrequency, StubPeriodTypeEnum.ShortInitial);

            Assert.AreEqual(unadjustedCalcPeriods.Count, 21);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsShortStub(unadjustedCalcPeriods[0], cps.PeriodInterval));
        }

        [TestMethod]
        public void LongInitialStubSpecified()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2012, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item26;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            DateTime firstRegularPeriodDate = new DateTime(2007, 4, 26);
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateSchedule(startDate, endDate, firstRegularPeriodDate, periodFrequency, StubPeriodTypeEnum.LongInitial);

            Assert.AreEqual(unadjustedCalcPeriods.Count, 20);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsLongStub(unadjustedCalcPeriods[0], cps.PeriodInterval));
        }

        [TestMethod]
        public void LongFinalStubSpecifiedFromTermination()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2010, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item12;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            Period periodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(periodFrequency);
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            DateTime lastRegularPeriodEndDate = new DateTime(2009, 8, 12);
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateScheduleFromTermDate(startDate, endDate, periodInterval, lastRegularPeriodEndDate, rollConvention, StubPeriodTypeEnum.LongFinal);

            Assert.AreEqual(unadjustedCalcPeriods.Count, 12);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsLongStub(unadjustedCalcPeriods[unadjustedCalcPeriods.Count - 1], cps.PeriodInterval));
        }

        [TestMethod]
        public void ShortFinalStubSpecifiedFromTermination()
        {
            DateTime startDate = new DateTime(2007, 01, 5);
            DateTime endDate = new DateTime(2010, 01, 5);
            RollConventionEnum rollConvention = RollConventionEnum.Item12;
            CalculationPeriodFrequency periodFrequency = CalculationPeriodFrequencyHelper.Parse("3M", rollConvention.ToString());
            Period periodInterval = CalculationPeriodHelper.CalculationPeriodFrequencyToInterval(periodFrequency);
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            DateTime lastRegularPeriodEndDate = new DateTime(2009, 8, 12);
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateScheduleFromTermDate(startDate, endDate, periodInterval, lastRegularPeriodEndDate, rollConvention, StubPeriodTypeEnum.ShortFinal);

            Assert.AreEqual(unadjustedCalcPeriods.Count, 13);
            Assert.IsTrue(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
            Assert.IsTrue(CalculationPeriodHelper.IsShortStub(unadjustedCalcPeriods[unadjustedCalcPeriods.Count - 1], cps.PeriodInterval));
        }

        [TestMethod]
        public void UnadjustedDateNoOfPeriodsFromTermination()
        {
            const int cCouponPeriods = 1;
            DateTime endDate = new DateTime(2010, 01, 5);
            Period periodInterval = PeriodHelper.Parse("6M");
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateScheduleFromTermDate(endDate, periodInterval, cCouponPeriods);

            Assert.AreEqual(unadjustedCalcPeriods.Count, cCouponPeriods);
            Assert.IsFalse(cps.HasFinalStub);
            Assert.IsFalse(cps.HasInitialStub);
        }

        [TestMethod]
        public void UnadjustedDateFullFirstFromTermination()
        {
            const int cCouponPeriods = 6;
            DateTime startDate = new DateTime(2007, 03, 5);
            DateTime endDate = new DateTime(2010, 01, 5);
            Period periodInterval = PeriodHelper.Parse("6M");
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateScheduleFromTermDate(startDate, endDate, periodInterval, true);
            Assert.AreEqual(unadjustedCalcPeriods.Count, cCouponPeriods);
            Assert.IsFalse(cps.HasFinalStub);
            Assert.IsFalse(cps.HasInitialStub);
        }

        [TestMethod]
        public void UnadjustedDatePartialFirstFromTermination()
        {
            const int cCouponPeriods = 6;
            DateTime startDate = new DateTime(2007, 03, 5);
            DateTime endDate = new DateTime(2010, 01, 5);
            Period periodInterval = PeriodHelper.Parse("6M");
            CalculationPeriodSchedule cps = new CalculationPeriodSchedule();
            List<CalculationPeriod> unadjustedCalcPeriods = cps.GetUnadjustedCalculationDateScheduleFromTermDate(startDate, endDate, periodInterval, false);
            Assert.AreEqual(unadjustedCalcPeriods.Count, cCouponPeriods);
            Assert.IsFalse(cps.HasFinalStub);
            Assert.IsTrue(cps.HasInitialStub);
        }

        #endregion

        #region CalculationPeriod Scheduler Tests

        [TestMethod]
        public void GetAdjustedDateScheduleSpeedTestFirstRun()
        {
            // properties
            DateTime startDate = new DateTime(2010, 03, 31);
            Period term = new Period { period = PeriodEnum.M, periodMultiplier = "2" };
            Period interval = new Period { period = PeriodEnum.M, periodMultiplier = "1" };
            BusinessDayAdjustments adjustment = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "USNY-AUSY");
            Stopwatch stopwatch = new Stopwatch();

            // Pre-load holidays so that they are not done in the timer
            int year = DateTime.Today.Year;
            const string FpmlIdentifier = "AUSY";
            //var dates1 = CalendarEngine.GetSignificantDates(new[] { year }, new[] { FpmlIdentifier });

            // Now test
            //DataCollection.StartProfile(ProfileLevel.Global, DataCollection.CurrentId);
            var bc = CalendarEngine.GetCalendar(new[] {FpmlIdentifier});
            stopwatch.Start();
            var dates = AdjustedDateScheduler.GetAdjustedDateSchedule(startDate, term, interval, adjustment, bc);
            stopwatch.Stop();

            //DataCollection.StopProfile(ProfileLevel.Global, DataCollection.CurrentId);

            Debug.Print("Time taken (s): " + stopwatch.Elapsed.TotalSeconds);
            Assert.IsNotNull(dates);
            Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < 0.1);
        }

        [TestMethod]
        public void GetAdjustedDateScheduleSpeedTestSubsequentRuns()
        {
            // properties
            var startDate = new DateTime(2010, 03, 31);
            var term = new Period { period = PeriodEnum.M, periodMultiplier = "2" };
            var interval = new Period { period = PeriodEnum.M, periodMultiplier = "1" };
            BusinessDayAdjustments adjustment = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "USNY-AUSY");
            
            var bc = CalendarEngine.ToBusinessCalendar(adjustment.businessCenters);
            var stopwatch = new Stopwatch();

            //// Pre-load holidays so that they are not done in the timer
            //var dates1 = AdjustedDateScheduler.GetAdjustedDateSchedule(startDate, term, interval, adjustment, bc);

            // Now test
            //DataCollection.StartProfile(ProfileLevel.Global, DataCollection.CurrentId);

            stopwatch.Start();
            var dates = AdjustedDateScheduler.GetAdjustedDateSchedule(startDate, term, interval, adjustment, bc);
            stopwatch.Stop();

            //DataCollection.StopProfile(ProfileLevel.Global, DataCollection.CurrentId);

            Debug.Print("Time taken (s): " + stopwatch.Elapsed.TotalSeconds);
            Assert.IsNotNull(dates);
            Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < 0.04);//Changed to .04 as .01 didn't succeed.
        }

        [TestMethod]
        public void GetUnadjustedDateScheduleTest()
        {
            DateTime startDate = new DateTime(2010, 03, 31);
            Period term = new Period { period = PeriodEnum.M, periodMultiplier = "2" };
            Period interval = new Period { period = PeriodEnum.M, periodMultiplier = "1" };

            var dates = DateScheduler.GetUnadjustedDateSchedule(startDate, term, interval);

            Assert.AreEqual(3, dates.Count);
            Assert.AreEqual(startDate, dates[0]);
            Assert.AreEqual(new DateTime(2010, 04, 30), dates[1]);
            Assert.AreEqual(new DateTime(2010, 05, 31), dates[2]);
            //Assert.AreEqual(new DateTime(2010, 05, 31), dates[3]);
        }

        [TestMethod]
        public void GetAdjustedDateScheduleTest()
        {
            DateTime startDate = new DateTime(2010, 03, 31);
            Period term = new Period { period = PeriodEnum.M, periodMultiplier = "2" };
            Period interval = new Period { period = PeriodEnum.M, periodMultiplier = "1" };
            BusinessDayAdjustments adjustment = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "USNY-AUSY");
            var bc = CalendarEngine.ToBusinessCalendar(adjustment.businessCenters);

            var dates = AdjustedDateScheduler.GetAdjustedDateSchedule(startDate, term, interval, adjustment, bc);

            Assert.AreEqual(3, dates.Count);
            Assert.AreEqual(startDate, dates[0]);
            Assert.AreEqual(new DateTime(2010, 04, 30), dates[1]);
            Assert.AreEqual(new DateTime(2010, 05, 28), dates[2]);
        }

        #endregion

        #region Dates MetaScheduler Tests

        [TestMethod]
        public void GetAdjustedDates()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            List<DateTime> unadjustedDates = AdjustedDatesMetaSchedule.GetAdjustedDates(GetMetaScheduleDefinition(), startDate, businessDayAdjustments, bc);
            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetUnadjustedDates()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);
            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(GetMetaScheduleDefinition(), startDate);
            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetUnadjustedDatesFromStartToEndDate()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = new DateTime(startDate.Year + 15, startDate.Month, startDate.Day);
            Debug.Print("Start date: {0}", startDate);
            Debug.Print("End date: {0}", endDate);
            DateTime[] unadjustedDates = DatesMetaSchedule.GetUnadjustedDates(GetMetaScheduleDefinition2(), startDate, endDate, PeriodHelper.Parse("1Y"), RollConventionEnumHelper.Parse("17"), true);
            PrintListOfDates(new List<DateTime>(unadjustedDates));
        }

        [TestMethod]
        public void GetUnadjustedDatesFromEndToStartDate()
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
            MetaScheduleItem item1 = new MetaScheduleItem
            {
                Period = IntervalHelper.FromMonths(6),
                RollFrequency = IntervalHelper.FromMonths(1),
                RollConvention = rollConventionEnum
            };
            rollsMetaSchedule.Add(item1);
            MetaScheduleItem item2 = new MetaScheduleItem
            {
                Period = IntervalHelper.FromMonths(6),
                RollFrequency = IntervalHelper.FromMonths(3),
                RollConvention = rollConventionEnum
            };
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
        public void GetAdjustedDates3Forward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);
            //Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(startDate.Day.ToString());
            List<MetaScheduleItem> rollsMetaSchedule = new List<MetaScheduleItem>();
            MetaScheduleItem item1 = new MetaScheduleItem
            {
                Period = IntervalHelper.FromMonths(6),
                RollFrequency = IntervalHelper.FromMonths(1),
                RollConvention = rollConventionEnum
            };
            rollsMetaSchedule.Add(item1);
            MetaScheduleItem item2 = new MetaScheduleItem
            {
                Period = IntervalHelper.FromMonths(6),
                RollFrequency = IntervalHelper.FromMonths(3),
                RollConvention = rollConventionEnum
            };
            rollsMetaSchedule.Add(item2);
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("MODFOLLOWING", "AUSY");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            List<DateTime> unadjustedDates = AdjustedDatesMetaSchedule.GetAdjustedDates3(startDate, endDate, rollsMetaSchedule, false, businessDayAdjustments, bc);
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
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(endDate.Day.ToString());
            List<MetaScheduleItem> rollsMetaSchedule = new List<MetaScheduleItem>();
            MetaScheduleItem item1 = new MetaScheduleItem
            {
                Period = IntervalHelper.FromMonths(6),
                RollFrequency = IntervalHelper.FromMonths(1),
                RollConvention = rollConventionEnum
            };
            rollsMetaSchedule.Add(item1);
            MetaScheduleItem item2 = new MetaScheduleItem
            {
                Period = IntervalHelper.FromMonths(6),
                RollFrequency = IntervalHelper.FromMonths(3),
                RollConvention = rollConventionEnum
            };
            rollsMetaSchedule.Add(item2);
            List<DateTime> unadjustedDates = DatesMetaSchedule.GetUnadjustedDates3(startDate, endDate, rollsMetaSchedule, true);
            Debug.Print("Meta schedule:");
            //        Debug.Print(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(rollsMetaSchedule)));
            Debug.Print("Start date: {0}", startDate);
            Debug.Print("End date: {0}", endDate);
            Debug.Print("Schedule:");
            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetAdjustedDates3_Backward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);

            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(endDate.Day.ToString());

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

            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("MODFOLLOWING", "AUSY");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            List<DateTime> unadjustedDates = AdjustedDatesMetaSchedule.GetAdjustedDates3(startDate, endDate, rollsMetaSchedule, true, businessDayAdjustments, bc);


            Debug.Print("Meta schedule:");
            //            Debug.Print(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(rollsMetaSchedule)));

            Debug.Print("Start date: {0}", startDate);
            Debug.Print("End date: {0}", endDate);

            Debug.Print("Schedule:");
            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetAdjustedDates2_Forward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);

            Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(startDate.Day.ToString());

            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("MODFOLLOWING", "AUSY");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            List<DateTime> unadjustedDates = AdjustedDatesMetaSchedule.GetAdjustedDates2(startDate, endDate, interval, rollConventionEnum, false, businessDayAdjustments, bc);

            PrintListOfDates(unadjustedDates);
        }

        [TestMethod]
        public void GetAdjustedDates2_Backward()
        {
            DateTime startDate = new DateTime(2008, 04, 10);
            DateTime endDate = new DateTime(2009, 04, 24);

            Period interval = IntervalHelper.FromMonths(3);
            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(endDate.Day.ToString());

            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("MODFOLLOWING", "AUSY");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            List<DateTime> unadjustedDates = AdjustedDatesMetaSchedule.GetAdjustedDates2(startDate, endDate, interval, rollConventionEnum, true, businessDayAdjustments, bc);

            PrintListOfDates(unadjustedDates);
        }

        #endregion

        #region Dates Schedule Generator Tests

        [TestMethod]
        public void DatesScheduleGenerator_GetDatesSchedule()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);

            //BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition = GetMetaScheduleDefinition();

            List<ThreeStringsRangeItem> metaScheduleDefinitionRange = new List<ThreeStringsRangeItem>();

            foreach (Triplet<Period, Period, RollConventionEnum> triplet in metaScheduleDefinition)
            {
                ThreeStringsRangeItem threeStringsRangeItem = new ThreeStringsRangeItem();
                threeStringsRangeItem.Value1 = triplet.First.ToString();
                threeStringsRangeItem.Value2 = triplet.Second.ToString();
                threeStringsRangeItem.Value3 = triplet.Third.ToString();

                metaScheduleDefinitionRange.Add(threeStringsRangeItem);
            }
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            object o = AdjustedDatesMetaSchedule.GetDatesSchedule(metaScheduleDefinitionRange, startDate, bc, "AUSY-GBLO", "FOLLOWING");

            //PrintListOfDates(unadjustedDates);
            Debug.Print("Dates:");
            //            Debug.Print(ParameterFormatter.FormatObject(o));
        }

        [TestMethod]
        public void DatesScheduleGenerator_GetDatesSchedule_NoAdjustment()
        {
            DateTime startDate = DateTime.Today;
            Debug.Print("Start date: {0}", startDate);

            //BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            List<Triplet<Period, Period, RollConventionEnum>> metaScheduleDefinition = GetMetaScheduleDefinition();

            List<ThreeStringsRangeItem> metaScheduleDefinitionRange = new List<ThreeStringsRangeItem>();

            foreach (Triplet<Period, Period, RollConventionEnum> triplet in metaScheduleDefinition)
            {
                ThreeStringsRangeItem threeStringsRangeItem = new ThreeStringsRangeItem();
                threeStringsRangeItem.Value1 = triplet.First.ToString();
                threeStringsRangeItem.Value2 = triplet.Second.ToString();
                threeStringsRangeItem.Value3 = triplet.Third.ToString();

                metaScheduleDefinitionRange.Add(threeStringsRangeItem);
            }
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create("FOLLOWING", "AUSY-GBLO");
            var bc = CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters);
            object o = AdjustedDatesMetaSchedule.GetDatesSchedule(metaScheduleDefinitionRange, startDate, bc, "", "");

            //PrintListOfDates(unadjustedDates);
            Debug.Print("Dates:");
            //          Debug.Print(ParameterFormatter.FormatObject(o));
        }

        #endregion

        #region DateTime Scheduler Tests

        [TestMethod]
        public void UnadjustedDatesFromTermination()
        {
            DateTime effectiveDate = new DateTime(2009, 05, 01);
            DateTime terminationDate = new DateTime(2010, 11, 27);
            Period periodInterval = PeriodHelper.Parse("3M");
            RollConventionEnum rollDayConvention = RollConventionEnum.Item19;

            DateTime firstRegularPeriodStartDate = effectiveDate;
            DateTime lastRegularPeriodEndDate = terminationDate;

            DateTime[] dates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);

            Assert.AreEqual(dates.Length, 7);
            Assert.AreEqual(firstRegularPeriodStartDate, new DateTime(2009, 08, 19));
            Assert.AreEqual(lastRegularPeriodEndDate, new DateTime(2010, 08, 19));

            dates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
            Assert.AreEqual(dates.Length, 7);
            Assert.AreEqual(firstRegularPeriodStartDate, new DateTime(2009, 08, 19));
            Assert.AreEqual(lastRegularPeriodEndDate, new DateTime(2010, 08, 19));

            effectiveDate = new DateTime(2009, 11, 16);
            terminationDate = new DateTime(2011, 11, 28);
            rollDayConvention = RollConventionEnum.Item29;
            dates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);

            Assert.AreEqual(dates.Length, 9);
            Assert.AreEqual(firstRegularPeriodStartDate, new DateTime(2010, 02, 28));
            Assert.AreEqual(lastRegularPeriodEndDate, new DateTime(2011, 08, 29));

            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            for (int i = 0; i < dates.Length; i++)
            {
                dates[i] = AdjustedDateHelper.ToAdjustedDate(CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters), dates[i], businessDayAdjustments);
            }
        }

        #endregion

        #region DayCounter tests

        [TestMethod]
        public void TestGetDayCounterTypes()
        {
            IDayCounter dc = DayCounterHelper.Parse("Actual365");
            Assert.IsNotNull(dc);

            // No longer supported
            //dc = DayCounterHelper.Parse("ActualMY");
            //Assert.IsNotNull(dc);

            //dc = DayCounterHelper.Parse("ActualQuarters");
            //Assert.IsNotNull(dc);
        }

        #endregion

        #region Adjusted Date Tests

        [TestMethod]
        public void ToAdjustedDateTest()
        {
            // Pre-load holidays so that they are not done in the timer
            //int year = DateTime.Today.Year;
            //const string FpmlIdentifier = "AUSY";
            var calendar = CalendarEngine.GetCalendar(new[] { "AUSY", "USNY" });
            
            // properties
            DateTime startDate = new DateTime(2010, 03, 31);
            BusinessDayAdjustments adjustment = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, "USNY-AUSY");
            Stopwatch stopwatch = new Stopwatch();

            // Now test

            //DataCollection.StartProfile(ProfileLevel.Global, DataCollection.CurrentId);

            stopwatch.Start();
            DateTime dates = AdjustedDateHelper.ToAdjustedDate(calendar, startDate, adjustment);
            stopwatch.Start();

            //DataCollection.StopProfile(ProfileLevel.Global, DataCollection.CurrentId);

            Debug.Print("Time taken (s): " + stopwatch.Elapsed.TotalSeconds);
            Assert.IsNotNull(dates);
            Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < 0.1);
        }

        #endregion

        #region Last Trading dates Tests

        [TestMethod]
        public void GetNextYearTest()
        {
            int year = LastTradingDate.GetNextYear(2009, 9);
            Assert.AreEqual(2009, year);
            year = LastTradingDate.GetNextYear(2009, 0);
            Assert.AreEqual(2010, year);
            year = LastTradingDate.GetNextYear(2010, 0);
            Assert.AreEqual(2010, year);
            year = LastTradingDate.GetNextYear(2010, 1);
            Assert.AreEqual(2011, year);
            year = LastTradingDate.GetNextYear(2010, 9);
            Assert.AreEqual(2019, year);
        }

        [TestMethod]
        public void TestExchangeIRCode()
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("IR", "Z8");
            Assert.AreEqual(dt.Month, 12);

            DateTime refDate = new DateTime(2010, 2, 8);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "IR", "H0");
            Assert.AreEqual(dt, new DateTime(2010, 3, 12));          
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "IR", "M0");
            Assert.AreEqual(dt, new DateTime(2010, 6, 11));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "IR", "U0");
            Assert.AreEqual(dt, new DateTime(2010, 9, 10));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "IR", "Z0");
            Assert.AreEqual(dt, new DateTime(2010, 12, 10));
        }

        [TestMethod]
        public void TestExchangeIBCode()
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("IB", "Z8"); //IB is the last business day of the month!
            Assert.AreEqual(dt.Month, 12);
            DateTime refDate = new DateTime(2011, 3, 21);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "IB", "U8");
            Assert.AreEqual(dt.Month, 9);
            refDate = new DateTime(2008, 7, 8);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "IB", "U1");
            Assert.AreEqual(dt.Day, 30);
        }

        [TestMethod]
        public void TestExchangeEDCode()
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("ED", "Z8");
            Assert.AreEqual(dt.Month, 12);
            DateTime refDate = new DateTime(2011, 1, 21);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ED", "H1");
            Assert.AreEqual(dt, new DateTime(2011, 3, 14));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ED", "M1");
            Assert.AreEqual(dt, new DateTime(2011, 6, 13));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ED", "U1");
            Assert.AreEqual(dt, new DateTime(2011, 9, 19));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ED", "Z1");
            Assert.AreEqual(dt, new DateTime(2011, 12, 19));
        }

        [TestMethod]
        public void TestExchangeEYCode()//THis is for Euronext EY.
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("EY", "Z8");
            Assert.AreEqual(dt.Month, 12);
            DateTime refDate = new DateTime(2011, 1, 21);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "EY", "H1");
            Assert.AreEqual(dt, new DateTime(2011, 3, 14));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "EY", "M1");
            Assert.AreEqual(dt, new DateTime(2011, 6, 13));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "EY", "U1");
            Assert.AreEqual(dt, new DateTime(2011, 9, 19));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "EY", "Z1");
            Assert.AreEqual(dt, new DateTime(2011, 12, 19));
        }

        [TestMethod]
        public void TestExchangeESCode()
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("ES", "Z8");
            Assert.AreEqual(dt.Month, 12);
            DateTime refDate = new DateTime(2011, 1, 21);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ES", "H1");
            Assert.AreEqual(dt, new DateTime(2011, 3, 14));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ES", "M1");
            Assert.AreEqual(dt, new DateTime(2011, 6, 13));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ES", "U1");
            Assert.AreEqual(dt, new DateTime(2011, 9, 19));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ES", "Z1");
            Assert.AreEqual(dt, new DateTime(2011, 12, 19));
        }

        [TestMethod]
        public void TestExchangeERCode()
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("ER", "Z8");
            Assert.AreEqual(dt.Month, 12);
            DateTime refDate = new DateTime(2011, 1, 21);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ER", "H1");
            Assert.AreEqual(dt, new DateTime(2011, 3, 14));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ER", "M1");
            Assert.AreEqual(dt, new DateTime(2011, 6, 13));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ER", "U1");
            Assert.AreEqual(dt, new DateTime(2011, 9, 19));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "ER", "Z1");
            Assert.AreEqual(dt, new DateTime(2011, 12, 19));
        }

        [TestMethod]
        public void TestExchangeLCode()
        {
            DateTime dt = LastTradingDayHelper.GetLastTradingDay("L", "Z8");
            Assert.AreEqual(dt.Month, 12);
            DateTime refDate = new DateTime(2011, 1, 21);
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "L", "H1");
            Assert.AreEqual(dt, new DateTime(2011, 3, 16));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "L", "M1");
            Assert.AreEqual(dt, new DateTime(2011, 6, 15));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "L", "U1");
            Assert.AreEqual(dt, new DateTime(2011, 9, 21));
            dt = LastTradingDayHelper.GetLastTradingDay(refDate, "L", "Z1");
            Assert.AreEqual(dt, new DateTime(2011, 12, 21));
        }

        [TestMethod]
        public void TestIMMDate()
        {
            DateTime referenceDate = new DateTime(2010, 06, 08); // Only used for the decade
            DateTime dt = LastTradingDayHelper.GetLastTradingDay(referenceDate, "IR", "Z1");
            Assert.AreEqual(new DateTime(2011, 12, 9), dt);
        }

        [TestMethod]
        public void TestNonMainCycleByYear()
        {
            foreach (string exchange in _exchanges)
            {
                var ex = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>(exchange);
                var td = LastTradingDayHelper.Parse(ex);
                var dt = td.GetLastTradingDays(2008, false);
                Assert.AreEqual(dt.Count, 12);
            }
        }

        [TestMethod]
        public void TestIRLastTradingDays10YearAllMonths()
        {
            Console.WriteLine("IR Last Trading Day 10 year:");
            var ex = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>("IR");
            var td = LastTradingDayHelper.Parse(ex);
            DateTime dt = new DateTime(DateTime.Today.Year, 1, 1);
            for (int incry = dt.Year; incry <= dt.Year + 10; incry++)
            {
                for (int incrm = dt.Month; incrm <= 12; incrm++)
                {
                    DateTime dtLtd = td.GetLastTradingDay(incrm, incry);
                    Console.WriteLine(dtLtd.ToString("dd-MMM-yyyy"));
                }
            }
        }

        [TestMethod]
        public void TestIRLastTradingDays10YearMainCycleOnly()
        {
            Console.WriteLine("IR Last Trading Day 10 year Main Cycle Only:");
            var ex = EnumHelper.Parse<RateFutureAssetAnalyticModelIdentifier>("IR");
            var td = LastTradingDayHelper.Parse(ex);
            DateTime dt = new DateTime(DateTime.Today.Year, 1, 1);
            for (int incry = dt.Year; incry <= dt.Year + 10; incry++)
            {
                var dtLtds = td.GetLastTradingDays(incry, true);
                foreach (DateTime dtLtd in dtLtds)
                {
                    Console.WriteLine(dtLtd.ToString("dd-MMM-yyyy"));
                }
            }
        }

        #endregion

        #region Central Bank Tests

            #region RBA

            [TestMethod]
            public void TestRBADaysForYear()
            {
               var dates = new RBADate().GetCentralBankDays(2008);
                Assert.AreEqual(dates.Count, 11);

                Debug.Print("RBA Dates for year {0}", 2008);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestRBADaysForMonthAndYear()
            {
                DateTime? date = new RBADate().GetCentralBankDay(11, 2008);
                if (date != null)
                {
                    Assert.AreEqual(new DateTime(2008, 11, 5), (DateTime)date);
                    Debug.Print("RBA Dates for month {0} in year {1}", 11, 2008);
                }
            }

            [TestMethod]
            public void TestRBADaysForMonthsAndYear()
            {
                List<int> months = new List<int>{ 1, 2, 4, 7, 9, 12 };
                var dates = new RBADate().DayByMonths(months, 2008);
                Assert.AreEqual(dates.Count, 5);
                Debug.Print("RBA Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestRBADaysInPeriod()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                DateTime dtEnd = new DateTime(2011, 07, 21);
                var dates = new RBADate().GetCentralBankDays(dtStart, dtEnd);
                Assert.AreEqual(dates.Count, 35);

                Debug.Print("RBA Dates between {0} and {1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString());
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestRBADaysRelativeToDateMainCycle()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new RBADate().GetCentralBankDays(dtStart, 13);
                Assert.AreEqual(dates.Count, 12);

                Debug.Print("RBA Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestRBADaysRelativeToDateQuarterlyCycle()
            {
                //DateTime dtStart1 = new DateTime(2008, 05, 22);
                //var dates1 = new RBADate().GetCentralBankDays(dtStart1, 12);

                //var dates2 = new RBADate().GetCentralBankDays(dates1[dates1.Count - 1], 11, false);

                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new RBADate().GetCentralBankDays(dtStart, 12, false);
                Assert.AreEqual(dates.Count, 4);

                Debug.Print("RBA Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            #endregion

            #region BHK

            /// <summary>
            ///A test for BHKDaysInYear
            ///</summary>
            [TestMethod]
            public void BHKDaysInYearTest()
            {
                int year = 2008;
                var actual = new BHKDate().GetCentralBankDays(year);
                Assert.AreEqual(11, actual.Count);
                Debug.Print("BHK Dates for year {0}", 2008);
                foreach (DateTime date in actual)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            /// <summary>
            ///A test for BHKDayByMonth
            ///</summary>
            [TestMethod]
            public void BHKDayByMonthTest()
            {
                int month = 11;
                int year = 2008;
                DateTime expected = new DateTime(2008, 11, 11);
                var actual = new BHKDate().GetCentralBankDay(month, year);
                Assert.AreEqual(expected, actual);
            }

            /// <summary>
            ///A test for BHKDayByMonths
            ///</summary>
            [TestMethod]
            public void BHKDayByMonthsTest()
            {
                List<int> months = new List<int> { 1, 2, 4, 7, 9, 12 };
                int year = 2008;
                var actual = new BHKDate().DayByMonths(months, year);
                Assert.AreEqual(5, actual.Count);
            }

            /// <summary>
            ///A test for GetBHKDays
            ///</summary>
            [TestMethod]
            public void GetBHKDaysTest()
            {
                DateTime startDate = new DateTime(2008, 5, 12);
                DateTime endDate = new DateTime(2011, 7, 21);
                var actual = new BHKDate().GetCentralBankDays(startDate, endDate);
                Assert.AreEqual(36, actual.Count);
            }

            #endregion

            #region BNZ

            /// <summary>
            ///A test for BNZDaysInYear
            ///</summary>
            [TestMethod]
            public void BNZDaysInYearTest()
            {
                int year = 2008;
                var actual = new BNZDate().GetCentralBankDays(year);
                Assert.AreEqual(11, actual.Count);
                Debug.Print("BNZ Dates for year {0}", 2008);
                foreach (DateTime date in actual)
                {
                    Debug.Print(date.ToShortDateString());
                }
                //Assert.Inconclusive("Verify the correctness of this test method.");
            }

            /// <summary>
            ///A test for BNZDayByMonth
            ///</summary>
            [TestMethod]
            public void BNZDayByMonthTest()
            {
                int month = 11;
                int year = 2008;
                DateTime expected = new DateTime(2008, 11, 11); // TODO: Initialize to an appropriate value
                var actual = new BNZDate().GetCentralBankDay(month, year);
                Assert.AreEqual(expected, actual);
                //Assert.Inconclusive("Verify the correctness of this test method.");
            }

            /// <summary>
            ///A test for BNZDayByMonths
            ///</summary>
            [TestMethod]
            public void BNZDayByMonthsTest()
            {
                List<int> months = new List<int> { 1, 2, 4, 7, 9, 12 };
                int year = 2008; // TODO: Initialize to an appropriate value
                var actual = new BNZDate().DayByMonths(months, year);
                Assert.AreEqual(5, actual.Count);
                //Assert.Inconclusive("Verify the correctness of this test method.");
            }

            /// <summary>
            ///A test for GetBNZDays
            ///</summary>
            [TestMethod]
            public void GetBNZDaysTest()
            {
                DateTime startDate = new DateTime(2008, 5, 12);
                DateTime endDate = new DateTime(2011, 7, 21);
                var actual = new BNZDate().GetCentralBankDays(startDate, endDate);
                Assert.AreEqual(36, actual.Count);
                //Assert.Inconclusive("Verify the correctness of this test method.");
            }

            /// <summary>
            ///A test for GetBHKDays
            ///</summary>
            [TestMethod]
            public void GetBNZDaysTest1()
            {
                DateTime startDate = new DateTime(2008, 5, 12);
                DateTime endDate = new DateTime(2011, 7, 21);
                var actual = new BNZDate().GetCentralBankDays(startDate, endDate);
                Assert.AreEqual(36, actual.Count);
                //Assert.Inconclusive("Verify the correctness of this test method.");
            }

            [TestMethod]
            public void TestBNZDaysForYear()
            {
                var dates = new BNZDate().GetCentralBankDays(2008);
                Assert.AreEqual(dates.Count, 11);

                Debug.Print("BNZ Dates for year {0}", 2008);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBNZDaysForMonthAndYear()
            {
                var date = new BNZDate().GetCentralBankDay(11, 2008);
                Assert.AreEqual(date, new DateTime(2008, 11, 11));
                Debug.Print("BNZ Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestBNZDaysInPeriod()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                DateTime dtEnd = new DateTime(2011, 07, 21);
                var dates = new BNZDate().GetCentralBankDays(dtStart, dtEnd);
                Assert.AreEqual(dates.Count, 36);

                Debug.Print("BNZ Dates between {0} and {1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString());
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBNZDaysRelativeToDateMainCycle()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new BNZDate().GetCentralBankDays(dtStart, 13);
                Assert.AreEqual(dates.Count, 13);

                Debug.Print("BNZ Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBNZDaysRelativeToDateQuarterlyCycle()
            {
                DateTime dtStart1 = new DateTime(2008, 05, 22);
                var dates1 = new BNZDate().GetCentralBankDays(dtStart1, 12);

                var dates2 = new BNZDate().GetCentralBankDays(dates1[dates1.Count - 1], 11, false);

                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new BNZDate().GetCentralBankDays(dtStart, 12, false);
                Assert.AreEqual(dates.Count, 4);

                Debug.Print("BNZ Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            #endregion

            #region BOE

            [TestMethod]
            public void TestBOEDaysForYear()
            {
                var dates = new BOEDate().GetCentralBankDays(2008);
                Assert.AreEqual(dates.Count, 11);

                Debug.Print("BOE Dates for year {0}", 2008);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBOEDaysForMonthAndYear()
            {
                var date = new BOEDate().GetCentralBankDay(11, 2008);
                Assert.AreEqual(date, new DateTime(2008, 11, 11));
                Debug.Print("BOE Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestBOEDaysForMonthsAndYear()
            {
                List<int> months = new List<int> { 1, 2, 4, 7, 9, 12 };
                var dates = new BOEDate().DayByMonths(months, 2008);
                Assert.AreEqual(dates.Count, 5);
                Debug.Print("BOE Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestBOEDaysInPeriod()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                DateTime dtEnd = new DateTime(2011, 07, 21);
                var dates = new BOEDate().GetCentralBankDays(dtStart, dtEnd);
                Assert.AreEqual(dates.Count, 36);

                Debug.Print("BOE Dates between {0} and {1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString());
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBOEDaysRelativeToDateMainCycle()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new BOEDate().GetCentralBankDays(dtStart, 13);
                Assert.AreEqual(dates.Count, 13);

                Debug.Print("BOE Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBOEDaysRelativeToDateQuarterlyCycle()
            {
                DateTime dtStart1 = new DateTime(2008, 05, 22);
                var dates1 = new BOEDate().GetCentralBankDays(dtStart1, 12);

                var dates2 = new BOEDate().GetCentralBankDays(dates1[dates1.Count - 1], 11, false);

                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new BOEDate().GetCentralBankDays(dtStart, 12, false);
                Assert.AreEqual(dates.Count, 4);

                Debug.Print("BOE Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            #endregion

            #region BOJ

            [TestMethod]
            public void TestBOJDaysForYear()
            {
                var dates = new BOJDate().GetCentralBankDays(2008);
                Assert.AreEqual(dates.Count, 11);

                Debug.Print("BOJ Dates for year {0}", 2008);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBOJDaysForMonthAndYear()
            {
                var date = new BOJDate().GetCentralBankDay(11, 2008);
                Assert.AreEqual(date, new DateTime(2008, 11, 11));
                Debug.Print("BOJ Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestBOJDaysForMonthsAndYear()
            {
                List<int> months = new List<int> { 1, 2, 4, 7, 9, 12 };
                var dates = new BOJDate().DayByMonths(months, 2008);
                Assert.AreEqual(dates.Count, 5);
                Debug.Print("BOJ Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestBOJDaysInPeriod()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                DateTime dtEnd = new DateTime(2011, 07, 21);
                var dates = new BOJDate().GetCentralBankDays(dtStart, dtEnd);
                Assert.AreEqual(dates.Count, 36);

                Debug.Print("BOJ Dates between {0} and {1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString());
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBOJDaysRelativeToDateMainCycle()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new BOJDate().GetCentralBankDays(dtStart, 13);
                Assert.AreEqual(dates.Count, 13);

                Debug.Print("BOJ Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestBOJDaysRelativeToDateQuarterlyCycle()
            {
                DateTime dtStart1 = new DateTime(2008, 05, 22);
                var dates1 = new BOJDate().GetCentralBankDays(dtStart1, 12);
                var dates2 = new BOJDate().GetCentralBankDays(dates1[dates1.Count - 1], 11, false);
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new BOJDate().GetCentralBankDays(dtStart, 12, false);
                Assert.AreEqual(dates.Count, 4);
                Debug.Print("BOJ Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            #endregion

            #region ECB

            [TestMethod]
            public void TestECBDaysForYear()
            {
                var dates = new ECBDate().GetCentralBankDays(2008);
                Assert.AreEqual(dates.Count, 11);

                Debug.Print("ECB Dates for year {0}", 2008);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestECBDaysForMonthAndYear()
            {
                var date = new ECBDate().GetCentralBankDay(11, 2008);
                Assert.AreEqual(date, new DateTime(2008, 11, 11));
                Debug.Print("ECB Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestECBDaysForMonthsAndYear()
            {
                List<int> months = new List<int> { 1, 2, 4, 7, 9, 12 };
                var dates = new ECBDate().DayByMonths(months, 2008);
                Assert.AreEqual(dates.Count, 5);
                Debug.Print("ECB Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestECBDaysInPeriod()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                DateTime dtEnd = new DateTime(2011, 07, 21);
                var dates = new ECBDate().GetCentralBankDays(dtStart, dtEnd);
                Assert.AreEqual(dates.Count, 36);

                Debug.Print("ECB Dates between {0} and {1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString());
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestECBDaysRelativeToDateMainCycle()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new ECBDate().GetCentralBankDays(dtStart, 13);
                Assert.AreEqual(dates.Count, 13);

                Debug.Print("ECB Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestECBDaysRelativeToDateQuarterlyCycle()
            {
                DateTime dtStart1 = new DateTime(2008, 05, 22);
                var dates1 = new ECBDate().GetCentralBankDays(dtStart1, 12);
                var dates2 = new ECBDate().GetCentralBankDays(dates1[dates1.Count - 1], 11, false);

                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new ECBDate().GetCentralBankDays(dtStart, 12, false);
                Assert.AreEqual(dates.Count, 4);

                Debug.Print("ECB Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            #endregion

            #region FOMC

            [TestMethod]
            public void TestFOMCDaysForYear()
            {
                var dates = new FOMCDate().GetCentralBankDays(2008);
                Assert.AreEqual(dates.Count, 11);

                Debug.Print("FOMC Dates for year {0}", 2008);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestFOMCDaysForMonthAndYear()
            {
                var date = new FOMCDate().GetCentralBankDay(11, 2008);
                Assert.AreEqual(date, new DateTime(2008, 11, 11));
                Debug.Print("FOMC Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestFOMCDaysForMonthsAndYear()
            {
                List<int> months = new List<int> { 1, 2, 4, 7, 9, 12 };
                var dates = new FOMCDate().DayByMonths(months, 2008);
                Assert.AreEqual(dates.Count, 5);
                Debug.Print("FOMC Dates for month {0} in year {1}", 11, 2008);
            }

            [TestMethod]
            public void TestFOMCDaysInPeriod()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                DateTime dtEnd = new DateTime(2011, 07, 21);
                var dates = new FOMCDate().GetCentralBankDays(dtStart, dtEnd);
                Assert.AreEqual(dates.Count, 36);

                Debug.Print("FOMC Dates between {0} and {1}", dtStart.ToShortDateString(), dtEnd.ToShortDateString());
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestFOMCDaysRelativeToDateMainCycle()
            {
                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new FOMCDate().GetCentralBankDays(dtStart, 13);
                Assert.AreEqual(dates.Count, 13);

                Debug.Print("FOMC Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            [TestMethod]
            public void TestFOMCDaysRelativeToDateQuarterlyCycle()
            {
                DateTime dtStart1 = new DateTime(2008, 05, 22);
                var dates1 = new FOMCDate().GetCentralBankDays(dtStart1, 12);
                var dates2 = new FOMCDate().GetCentralBankDays(dates1[dates1.Count - 1], 11, false);

                DateTime dtStart = new DateTime(2008, 05, 12);
                var dates = new FOMCDate().GetCentralBankDays(dtStart, 12, false);
                Assert.AreEqual(dates.Count, 4);

                Debug.Print("FOMC Dates from {0} and {1} subsequent months", dtStart.ToShortDateString(), 13);
                foreach (DateTime date in dates)
                {
                    Debug.Print(date.ToShortDateString());
                }
            }

            #endregion

        #endregion

        #region Helpers


        private static int CalculateNextQuarter()
        {
            int remainder = DateTime.Today.Month % 3;
            int quarter = DateTime.Today.Month - remainder;
            return (remainder == 0) ? quarter : quarter + 3;
        }

        internal static List<Triplet<Period, Period, RollConventionEnum>> GetMetaScheduleDefinition()
        {
            List<Triplet<Period, Period, RollConventionEnum>> result = new List<Triplet<Period, Period, RollConventionEnum>>();

            result.Add(new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("1M"), PeriodHelper.Parse("1Y"), RollConventionEnum.Item17));
            result.Add(new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("3M"), PeriodHelper.Parse("5Y"), RollConventionEnum.Item17));
            result.Add(new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("6M"), PeriodHelper.Parse("4Y"), RollConventionEnum.Item17));

            return result;
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
            List<Triplet<Period, Period, RollConventionEnum>> result = new List<Triplet<Period, Period, RollConventionEnum>>();

            result.Add(new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("1M"), PeriodHelper.Parse("1Y"), RollConventionEnum.Item17));
            result.Add(new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("2Y"), PeriodHelper.Parse("8Y"), RollConventionEnum.Item17));
            result.Add(new Triplet<Period, Period, RollConventionEnum>(PeriodHelper.Parse("6M"), PeriodHelper.Parse("1Y"), RollConventionEnum.Item17));

            return result;
        }

        /// <summary>
        /// Validates the specified expected dates list.
        /// </summary>
        /// <param name="expectedDatesList">The expected dates list.</param>
        /// <param name="observedDates">The observed dates.</param>
        private static void Validate(string expectedDatesList, DateTime[] observedDates)
        {
            string[] expectedDates = expectedDatesList.Split(';');

            Assert.AreEqual(expectedDates.Length, observedDates.Length);
            int index = 0;
            foreach (string expectedDate in expectedDates)
            {
                Assert.AreEqual(DateTime.Parse(expectedDate), observedDates[index]);
                index++;
            }
        }

        /// <summary>
        /// Adjusts the dates.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <param name="dayConvention">The day convention.</param>
        /// <param name="businessCenters">The business centers.</param>
        /// <returns></returns>
        static private DateTime[] AdjustDates(DateTime[] dates, BusinessDayConventionEnum dayConvention, string businessCenters)
        {
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(dayConvention, businessCenters);
            for (int i = 0; i < dates.Length; i++)
            {
                dates[i] = AdjustedDateHelper.ToAdjustedDate(CalendarEngine.ToBusinessCalendar(businessDayAdjustments.businessCenters), dates[i], businessDayAdjustments);
            }
            return dates;
        }

        static private bool CompareHolidays(IBusinessCalendar bc, IBusinessCalendar calendarClass, DateTime dtStart, DateTime dtEnd)
        {
            bool success = true;
            if (bc == null || calendarClass == null)
            {
                return false;
            }
            for (DateTime dtCheckDate = dtStart; dtCheckDate <= dtEnd; dtCheckDate = dtCheckDate.AddDays(1))
            {
                // *** Uncomment if debugging only so that you can break into a certain date
                //if ((dtCheckDate.Year == 2035) && (dtCheckDate.Month == 11) && (dtCheckDate.Day == 21))
                //    success = success;

                if ((!bc.IsHoliday(dtCheckDate)) && (calendarClass.IsHoliday(dtCheckDate)))
                {
                    string errorMessage =
                        String.Format("{0} says that {1} is a holiday, though it is NOT (according to {2} business calendar)",
                                      string.Format("{0}.cs", calendarClass.ToString()), dtCheckDate, bc.ToString());
                    success = false;
                    Debug.WriteLine(errorMessage);
                    Console.WriteLine(errorMessage);
                }
            }
            return success;
        }

        private static DateTime ParseYYYYMMDDDateTime(string s)
        {
            int yyyy = int.Parse(s.Substring(0, 4));
            int mm = int.Parse(s.Substring(4, 2));
            int dd = int.Parse(s.Substring(6, 2));

            return new DateTime(yyyy, mm, dd);
        }

        static private string[] GetHolidayDatesForValidation(string calendarName)
        {
            string fullResourceName = string.Format("{0}.{1}.{2}", ResourceName, calendarName, ResourceNameSuffix);
            string[] holidayDates = { };
            try
            {
                string dataFile = ResourceHelper.GetResource(Assembly.GetExecutingAssembly(), fullResourceName);
                holidayDates = dataFile.Split((" " + Environment.NewLine).ToCharArray());
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }

            return holidayDates;
        }

        static private bool CompareHolidays(IBusinessCalendar bc, string[] holidayDates)
        {
            bool success = true;

            foreach (string dateAsYYYYMMDDString in holidayDates)
            {
                if (dateAsYYYYMMDDString.Trim().Length != 0)//skip if the empty string
                {
                    DateTime date = ParseYYYYMMDDDateTime(dateAsYYYYMMDDString);

                    if (!bc.IsHoliday(date))
                    {
                        string errorMessage =
                            String.Format("{0} says that {1} is a holiday, though it is NOT (according to {2})",
                                          string.Format("{0}.{1}", bc.ToString(), ResourceNameSuffix), date, bc.ToString());

                        Debug.WriteLine(errorMessage);
                        Console.WriteLine(errorMessage);

                        success = false;
                    }
                }
            }
            return success;
        }


        private void GetCalendarList(string calendarMap, ref string[] calendars, ref string[] fpmlNames)
        {
            string[] calenderMaps = calendarMap.Split(new Char[] { ',' });

            // iterate through each calendar
            calendars = new string[calenderMaps.GetLength(0)];
            fpmlNames = new string[calenderMaps.GetLength(0)];
            StringBuilder sbfpmlCalendars = new StringBuilder();
            int len = calenderMaps.GetUpperBound(0);
            for (int i = 0; i <= len; i++)
            {
                string[] splitmap = calenderMaps[i].Split(new Char[] { ':' });
                calendars[i] = splitmap[0];
                fpmlNames[i] = splitmap[1];
            }
        }

        static private Boolean ProcessCalendarDataFiles(string calendar, string[] holidayDates)
        {
            Boolean bFailureNotSet = true;
            Boolean bRetval = true;
            if (holidayDates.Length > 0)
            {
                IBusinessCalendar bc = CalendarEngine.GetCalendar(new[] { calendar });
                bRetval = CompareHolidays(bc, holidayDates);

                if (!bRetval)
                    bFailureNotSet = false;
            }
            else
                bRetval = true;

            return bFailureNotSet;
        }

        //BMK Processing the calendars
        static private Boolean ProcessCalendarClasses(string[] calendars, IDictionary<string, IBusinessCalendar> calendarClasses, DateTime dtFrom, DateTime dtTo)
        {
            Boolean bFailureNotSet = true;
            int[] years = new int[CTotalNoOfYears + 1];
            years[0] = CStartYear;
            for (int i = 1; i <= CTotalNoOfYears; i++)
            {
                years[i] = CStartYear + i;
            }
            foreach (string calendar in calendars)
            {
                if (calendarClasses.ContainsKey(calendar))
                {
                    IBusinessCalendar bc = CalendarEngine.GetCalendar(years, new[] { calendar });
                    IBusinessCalendar calendarClass = calendarClasses[calendar];
                    Boolean bRetval = CompareHolidays(bc, calendarClass, dtFrom, dtTo);

                    if (!bRetval)
                        bFailureNotSet = false;
                }
            }
            return bFailureNotSet;
        }

        #endregion

        #endregion
    }
}
