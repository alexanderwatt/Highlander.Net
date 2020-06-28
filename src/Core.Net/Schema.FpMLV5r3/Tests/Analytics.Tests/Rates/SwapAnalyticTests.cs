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
using Highlander.Reporting.Analytics.V5r3.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Rates
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class SwapAnalyticTests 
    {
        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestDelta1Amount()
        {
            for (var i = 1; i < 10; i++)
            {
                var delta1 = SwapAnalytics.Delta1ForAnAmount(10000000.0, Math.Exp(-0.1 * i), 0.25, i);
                Debug.WriteLine($"Delta1 : {delta1} Time: {i}");
            }

        }

        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestDelta1Notional()
        {
            for (var i = 1; i < 10; i++)
            {
                var delta1 = SwapAnalytics.Delta1ForNotional(400000000.0, 0.25, 0.1, Math.Exp(-0.1 * i), 
                    0.25, i);
                Debug.WriteLine($"Delta1 : {delta1} Time: {i}");
            }

        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestDelta0Coupon()
        {
            for (var i = 1; i < 10; i++)
            {
                var delta0 = SwapAnalytics.Delta0Coupon(1000000.0, 0.25, 0.07, Math.Exp(-0.07 * i));
                Debug.WriteLine($"Delta1 : {delta0} Time: {i}");
            }

        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestDelta0discountCoupon()
        {
            for (var i = 1; i < 10; i++)
            {
                var delta0 = SwapAnalytics.Delta0DiscountCoupon(1000000.0, 0.25, 0.07, Math.Exp(-0.07 * i));
                Debug.WriteLine($"Delta1 : {delta0} Time: {i}");
            }
        }
    }
}