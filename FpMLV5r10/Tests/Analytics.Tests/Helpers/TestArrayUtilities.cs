using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Utilities;

namespace Orion.Analytics.Tests.Helpers
{
    [TestClass]
    public class TestArrayUtilities
    {
        private static decimal[] array1 = new[] {1.0m, 3.0m, 6.0m};
        private static decimal[] array2 = new[] { 0.5m, 0.5m, 0.5m };
        private static decimal[] array3 = new[] { 2.0m, 2.0m, 2.0m };

        /// <summary>
        /// Testing the FuturesMarginConvexityAdjustment.
        /// </summary>
        [TestMethod]
        public void TestSumProduct()
        {
            var result1 = ArrayUtilities.SumProduct(array1, array2);
            var result2 = ArrayUtilities.SumProduct(array1, array2, array3);
            Debug.WriteLine(String.Format("result1 : {0} result2: {1}", result1, result2));
        }
    }
}