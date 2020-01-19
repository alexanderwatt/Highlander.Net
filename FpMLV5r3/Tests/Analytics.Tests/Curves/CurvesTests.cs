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

#region Using Directives

using System;
using System.Diagnostics;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.ModelFramework.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Curves
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class CurvesTests 
    {
        private static readonly double[] rates = { 0.071d, 0.072d, 0.071d, 0.072d, 0.073d, 0.075d, 0.074d, 0.076d, 0.075d };
        private static readonly double[] times = {0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d};

        private static readonly double[] testpts = {0.5, 0.75, 1.0, 1.5, 2.3, 3.1};
        private const string interp = "LinearInterpolation";

        private readonly decimal[] _compounding = { 0.0m, (decimal)1 / 365, (decimal)1 / 52, (decimal)1 / 12, (decimal)1 / 6, (decimal)1 / 4, (decimal)1 / 2, 1 };

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestGetValue1()
        {
            foreach(var pt in testpts)
            {
                var result = CurveAnalytics.GetValue(pt, interp, true, times, rates);
                Debug.WriteLine($"rate : {pt} Time: {result}");
            }
        }

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestGetValue2()
        {
            var baseDate = new DateTime(2008, 3, 3);
            for (var i = 1; i < 10; i++ )
            {
                var targetDate = baseDate.AddMonths(3*i);
                IPoint point = new DateTimePoint1D(baseDate, targetDate);
                var result = CurveAnalytics.GetDateValue(point, interp, true, times, rates);
                Debug.WriteLine($"rate : {targetDate} Date: {result}");
            }
        }
    }
}