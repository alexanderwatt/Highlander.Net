using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Distributions;

namespace Orion.Analytics.Tests.Distributions
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