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
using Highlander.Reporting.Analytics.V5r3.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Distributions
{
    /// <summary>
    /// Unit Tests for the class GaussianRandomNumbers.
    /// </summary>
    [TestClass]
    public class GaussianRandomNumbersTests
    {
        /// <summary>
        /// Tests the method GetGaussianDeviates.
        /// </summary>
        [TestMethod]
        public void TestGetGaussianDeviates()
        {
            // Test the case of a user defined seed.
            int seed1 = 123;
            GaussianRandomNumbers generatorObj1 =
                new GaussianRandomNumbers(seed1);
            Assert.IsNotNull(generatorObj1);
            long numDeviates1 = 1000;
            double[] storage1 = null;
            generatorObj1.GetGaussianDeviates(numDeviates1, ref storage1);
            Assert.AreEqual(numDeviates1, storage1.Length);
            // Test the case of an internally set seed.
            int seed2 = -1;
            GaussianRandomNumbers generatorObj2 =
                new GaussianRandomNumbers(seed2);
            Assert.IsNotNull(generatorObj2);
            long numDeviates2 = 1000;
            double[] storage2 = null;
            generatorObj2.GetGaussianDeviates(numDeviates2, ref storage2);
            Assert.AreEqual(numDeviates2, storage2.Length);
            // Test that the two sequences of Gaussian deviates are distinct.
            for(long i = 0; i < Math.Min(numDeviates1, numDeviates2); ++i)
            {
                Assert.AreNotEqual(storage1[i], storage2[i]);
            }
        }
    }
}