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

using System.Collections.Generic;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Dates
{
    [TestClass]
    public class NotionalScheduleGeneratorTests
    {
        [TestMethod]
        public void TestGetNotionalScheduleImpl()
        {

            var inputAndOutput = new Dictionary<double[], List<double>>
            {
                {
                    new[] {50000000.0, -5000000.0, 1.0, 12.0},
                    new List<double>(new[]
                    {
                        50000000.0, 50000000.0 + (-5000000.0) * 1, 50000000.0 + (-5000000.0) * 2,
                        50000000.0 + (-5000000.0) * 3, 50000000.0 + (-5000000.0) * 4, 50000000.0 + (-5000000.0) * 5,
                        50000000.0 + (-5000000.0) * 6, 50000000.0 + (-5000000.0) * 7, 50000000.0 + (-5000000.0) * 8,
                        50000000.0 + (-5000000.0) * 9, 50000000.0 + (-5000000.0) * 10,
                        50000000.0 + (-5000000.0) * 11
                    })
                },
                {
                    new[] {50000000.0, +10000000.0, 2.0, 8.0},
                    new List<double>(new[]
                    {
                        50000000.0, 50000000.0, 50000000.0 + (10000000.0) * 1, 50000000.0 + (10000000.0) * 1,
                        50000000.0 + (10000000.0) * 2, 50000000.0 + (10000000.0) * 2, 50000000.0 + (10000000.0) * 3,
                        50000000.0 + (10000000.0) * 3
                    })
                },
                {
                    new[] {50000000.0, +10000000.0, 3.0, 8.0},
                    new List<double>(new[]
                    {
                        50000000.0, 50000000.0, 50000000.0, 50000000.0 + (10000000.0) * 1,
                        50000000.0 + (10000000.0) * 1, 50000000.0 + (10000000.0) * 1, 50000000.0 + (10000000.0) * 2,
                        50000000.0 + (10000000.0) * 2
                    })
                }
            };





            foreach (double[] input in inputAndOutput.Keys)
            {
                double initialValue = input[0];
                double step = input[1];
                double applyStepToEachNthCashflow = input[2];
                double totalNumberOfCashflows = input[3];
                List<double> notionalSchedule = NotionalScheduleGenerator.GetNotionalScheduleImpl(initialValue, step, (int)applyStepToEachNthCashflow, (int)totalNumberOfCashflows);
                Assert.AreEqual(totalNumberOfCashflows, notionalSchedule.Count);
                List<double> expectedOutput = inputAndOutput[input];
                Assert.AreEqual(expectedOutput.Count, notionalSchedule.Count);
                for(int i = 0; i < expectedOutput.Count; ++i)
                {
                    Assert.AreEqual(expectedOutput[i], notionalSchedule[i]);
                }
            }
        }
    }
}