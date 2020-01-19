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
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.TestHelpers.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Utilities
{
    /// <summary>
    /// Unit Tests for the class DayCount.
    /// </summary>
    [TestClass]
    public class DayCountUnitTests
    {
        private double _actual;
        private string _dayCount;
        private double _expected;
        private DateTime _refDate;
        private double _tolerance;
        

        [TestInitialize]
        public void Initialisation()
        {
            _dayCount = "ACT/365.FIXED";
            _refDate = new DateTime(2005,10,13,0,0,0);
            _tolerance = 1.0E-7;
        }

        /// <summary>
        /// Tests the method YearFraction.
        /// </summary>
        [TestMethod]
        public void TestYearFraction()
        {
            _dayCount = "ACT/ACT.ISDA";

            // Instantiate a class object and check that it is not null.
            IDayCounter obj = DayCounterHelper.Parse(_dayCount);

            Assert.AreNotEqual(obj, null);

            // Test the default day count convention.
            DateTime targetDate1 = new DateTime(2005, 12, 15, 0, 0, 0);
            _expected = 0.1726027397;
            _actual = obj.YearFraction(_refDate, targetDate1);

            AssertExtension.Less(Math.Abs(_actual - _expected),_tolerance);

            // Test ACT/360 day count convention in lower case format.
            _dayCount = "ACT/360";
            DateTime targetDate2 = new DateTime(2006, 1, 17, 0, 0, 0);
            _expected = 0.26301369863013694262537;
            _actual = obj.YearFraction(_refDate, targetDate2);
            double temp = _actual - _expected;
            AssertExtension.Less(Math.Abs(temp), _tolerance);

            // Test ACT/365 day count convention.
            _dayCount = "ACT/365.FIXED";
            DateTime targetDate3 = new DateTime(2005, 12, 25, 0, 0, 0);
            _expected = 0.1999999999;
            _actual = obj.YearFraction(_refDate, targetDate3);
            temp = _actual - _expected;
            AssertExtension.Less(Math.Abs(temp), _tolerance);

            // Test negative year fraction.
            _dayCount = "ACT/360";
            DateTime targetDate4 = new DateTime(2004, 8, 15, 0, 0, 0);
            _expected = -1.160603338;
            _actual = obj.YearFraction(_refDate, targetDate4);
            temp = _actual - _expected;
            AssertExtension.Less(Math.Abs(temp), _tolerance);
        }
    }
}