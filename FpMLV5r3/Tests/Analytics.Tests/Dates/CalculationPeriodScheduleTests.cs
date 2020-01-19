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
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.VisualStudio.Profiler;

namespace Highlander.Analytics.Tests.V5r3.Dates
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
