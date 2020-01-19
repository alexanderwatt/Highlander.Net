#region Using Directives

using System;
using System.Diagnostics;
using Orion.Analytics.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Orion.Analytics.Tests.Rates
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
                Debug.WriteLine(String.Format("Delta1 : {0} Time: {1}", delta1, i));
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
                Debug.WriteLine(String.Format("Delta1 : {0} Time: {1}", delta1, i));
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
                Debug.WriteLine(String.Format("Delta1 : {0} Time: {1}", delta0, i));
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
                Debug.WriteLine(String.Format("Delta1 : {0} Time: {1}", delta0, i));
            }

        }

    }
}