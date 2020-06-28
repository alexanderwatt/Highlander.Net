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
using System.Diagnostics;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Helpers
{
    [TestClass]
    public class TestArrayUtilities
    {
        private static decimal[] array1 = {1.0m, 3.0m, 6.0m};
        private static decimal[] array2 = { 0.5m, 0.5m, 0.5m };
        private static decimal[] array3 = { 2.0m, 2.0m, 2.0m };

        /// <summary>
        /// Testing the FuturesMarginConvexityAdjustment.
        /// </summary>
        [TestMethod]
        public void TestSumProduct()
        {
            var result1 = ArrayUtilities.SumProduct(array1, array2);
            var result2 = ArrayUtilities.SumProduct(array1, array2, array3);
            Debug.WriteLine($"result1 : {result1} result2: {result2}");
        }
    }
}