#region Using directives

using System.Collections.Generic;
using Orion.Analytics.Schedulers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Orion.Analytics.Tests.Dates
{
    [TestClass]
    public class SpreadScheduleGeneratorTests
    {
        [TestMethod]
        public void TestGetSpreadScheduleImpl()
        {

            var inputAndOutput = new Dictionary<double[], List<double>>();

            inputAndOutput.Add(
                new[] { 0.05, 0.0025, 1.0, 12.0 },//initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows
                new List<double>(new[] 
                                     { 0.05, 
                                       0.05 + (0.0025) * 1,
                                       0.05 + (0.0025) * 2,
                                       0.05 + (0.0025) * 3,
                                       0.05 + (0.0025) * 4,
                                       0.05 + (0.0025) * 5,
                                       0.05 + (0.0025) * 6,
                                       0.05 + (0.0025) * 7,
                                       0.05 + (0.0025) * 8,
                                       0.05 + (0.0025) * 9,
                                       0.05 + (0.0025) * 10,
                                       0.05 + (0.0025) * 11
                                     }));

            inputAndOutput.Add(
                new[] { 0.05, +0.0025, 2.0, 8.0 },//initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows
                new List<double>(new[] 
                                     { 0.05, 
                                       0.05,
                                       0.05 + (0.0025) * 1,
                                       0.05 + (0.0025) * 1,
                                       0.05 + (0.0025) * 2,
                                       0.05 + (0.0025) * 2,
                                       0.05 + (0.0025) * 3,
                                       0.05 + (0.0025) * 3
                                     }));

            inputAndOutput.Add(
                new[] { 0.05, +0.0025, 3.0, 8.0 },//initialValue, step, applyStepToEachNthCashflow, totalNumberOfCashflows
                new List<double>(new[] 
                                     { 0.05, 
                                       0.05,
                                       0.05,
                                       0.05 + (0.0025) * 1,
                                       0.05 + (0.0025) * 1,
                                       0.05 + (0.0025) * 1,
                                       0.05 + (0.0025) * 2,
                                       0.05 + (0.0025) * 2
                                     }));


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