using System;
using System.Collections.Generic;
//using Microsoft.VisualStudio.Profiler;
using FpML.V5r3.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Schedulers;

namespace Orion.Analytics.Tests.Dates
{
    [TestClass]
    public class CalculationPeriodScheduleTests
    {

        [TestMethod]
        public void GetUnadjustedDateScheduleTest()
        {
            var startDate = new DateTime(2010, 03, 31);
            var term = new Period { period = PeriodEnum.M, periodMultiplier = "2" };
            var interval = new Period { period = PeriodEnum.M, periodMultiplier = "1" };
            
            List<DateTime> dates = DateScheduler.GetUnadjustedDateSchedule(startDate, term, interval);
            
            Assert.AreEqual(3, dates.Count);
            Assert.AreEqual(startDate, dates[0]);
            Assert.AreEqual(new DateTime(2010, 04, 30), dates[1]);
            Assert.AreEqual(new DateTime(2010, 05, 31), dates[2]);
            //Assert.AreEqual(new DateTime(2010, 05, 31), dates[3]);
        }
    }
}
