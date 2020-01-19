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
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Exception = System.Exception;

namespace Highlander.Analytics.Tests.V5r3.Dates
{
    [TestClass]
    public class FinancialUnitTests
    {
        //private string[] _exchanges = new[] { "IR", "L", "ED", "ER", "W", "IB", "EY", "ZB", "ES", "RA", "HR" };

        [TestMethod]
        public void RegularPeriodIntervalUnadjustedDates()
        {
            bool bFailureSet = false;
            try
            {
                // 5 year 3 month cash flow
                DateTime startDate = new DateTime(2007, 01, 5);
                Period intervalToMaturity = PeriodHelper.Parse("5Y");
                Period  periodInterval = PeriodHelper.Parse("3M");
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

        [TestMethod]
        public void TestGetDayCounterTypes()
        {
            IDayCounter dc =  DayCounterHelper.Parse("Actual365");
            Assert.IsNotNull(dc);
            // No longer supported
            //dc = DayCounterHelper.Parse("ActualMY");
            //Assert.IsNotNull(dc);
            //dc = DayCounterHelper.Parse("ActualQuarters");
            //Assert.IsNotNull(dc);
        }
    }
}