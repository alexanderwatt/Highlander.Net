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
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Dates
{
    [TestClass]
    public class DateSchedulerTest
    {
        [TestMethod]
        public void UnadjustedDatesFromTermination()
        {
            DateTime effectiveDate = new DateTime(2009, 05, 01);
            DateTime terminationDate = new DateTime(2010, 11, 27);
            Period periodInterval = PeriodHelper.Parse("3M");
            RollConventionEnum rollDayConvention = RollConventionEnum.Item19;
            DateTime[] dates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention,  out var firstRegularPeriodStartDate, out var lastRegularPeriodEndDate);
            Assert.AreEqual(dates.Length, 7);
            Assert.AreEqual(firstRegularPeriodStartDate, new DateTime(2009, 08, 19));
            Assert.AreEqual(lastRegularPeriodEndDate, new DateTime(2010, 08, 19));
            dates = DateScheduler.GetUnadjustedDatesFromTerminationDate(effectiveDate, terminationDate, periodInterval, rollDayConvention,  out firstRegularPeriodStartDate, out lastRegularPeriodEndDate);
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
            //bdates = AdjustDates(bdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDates, bdates);
            DateTime[] fdates = DateScheduler.GetUnadjustedDatesFromEffectiveDate(effectiveDate, terminationDate, periodInterval, rollDayConvention, out _, out _);
            //fdates = AdjustDates(fdates, BusinessDayConventionEnum.MODFOLLOWING, "AUSY");
            Validate(expectedDates, fdates);
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
    }
}