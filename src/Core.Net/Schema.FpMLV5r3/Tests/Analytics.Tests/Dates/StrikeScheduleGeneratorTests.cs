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
    public class StrikeScheduleGeneratorTests
    {
        [TestMethod]
        public void TestGetStrikeScheduleImpl()
        {

            var inputAndOutput = new Dictionary<double[], List<double>>
            {
                {
                    new[] {0.05, 0.0025, 1.0, 12.0},
                    new List<double>(new[]
                    {
                        0.05, 0.05 + (0.0025) * 1, 0.05 + (0.0025) * 2, 0.05 + (0.0025) * 3, 0.05 + (0.0025) * 4,
                        0.05 + (0.0025) * 5, 0.05 + (0.0025) * 6, 0.05 + (0.0025) * 7, 0.05 + (0.0025) * 8,
                        0.05 + (0.0025) * 9, 0.05 + (0.0025) * 10, 0.05 + (0.0025) * 11
                    })
                },
                {
                    new[] {0.05, +0.0025, 2.0, 8.0},
                    new List<double>(new[]
                    {
                        0.05, 0.05, 0.05 + (0.0025) * 1, 0.05 + (0.0025) * 1, 0.05 + (0.0025) * 2,
                        0.05 + (0.0025) * 2, 0.05 + (0.0025) * 3, 0.05 + (0.0025) * 3
                    })
                },
                {
                    new[] {0.05, +0.0025, 3.0, 8.0},
                    new List<double>(new[]
                    {
                        0.05, 0.05, 0.05, 0.05 + (0.0025) * 1, 0.05 + (0.0025) * 1, 0.05 + (0.0025) * 1,
                        0.05 + (0.0025) * 2, 0.05 + (0.0025) * 2
                    })
                }
            };
            foreach (double[] input in inputAndOutput.Keys)
            {
                double initialValue = input[0];
                double step = input[1];
                double applyStepToEachNthCashflow = input[2];
                double totalNumberOfCashflows = input[3];
                List<double> notionalSchedule = StrikeScheduleGenerator.GetStrikeScheduleImpl(initialValue, step, (int)applyStepToEachNthCashflow, (int)totalNumberOfCashflows);
                Assert.AreEqual(totalNumberOfCashflows, notionalSchedule.Count);
                List<double> expectedOutput = inputAndOutput[input];
                Assert.AreEqual(expectedOutput.Count, notionalSchedule.Count);
                for(int i = 0; i < expectedOutput.Count; ++i)
                {
                    Assert.AreEqual(expectedOutput[i], notionalSchedule[i], 0.0000001);
                }
            }
        }
    }
}