using HLV5r3.Analytics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Rates;

namespace Orion.ExcelAPI.Tests.Analytics
{
    /// <summary>
    ///This is a test class for SwapFunctionsTest and is intended
    ///to contain all SwapFunctionsTest Unit Tests
    ///</summary>
    [TestClass]
    public class SwapFunctionsTest
    {
        /// <summary>
        ///A test for TimeFromInterval
        ///</summary>
        [TestMethod]
        public void TimeFromIntervalTest()
        {
            const string period = "2W";
            const double expected = 2 / 52d;
            double actual = new Swaps().TimeFromInterval(period);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Delta1ForNotional
        ///</summary>
        [TestMethod]
        public void Delta1ForNotionalTest()
        {
            const double notional = 100;
            const double yearfraction = 0.5;
            const double rate = 0.0615;
            const double paymentDiscountFactor = 0.92;
            const double periodAsTimesPerYear = 6;
            const double curveYearFraction = 0.5;
            const double expected = 0.00007070;
            double actual = SwapAnalytics.Delta1ForNotional(notional, yearfraction, rate, paymentDiscountFactor, periodAsTimesPerYear, curveYearFraction);
            Assert.AreEqual(expected, actual, 1e-8);
        }

        /// <summary>
        ///A test for Delta1ForAnAmount
        ///</summary>
        [TestMethod]
        public void Delta1ForAnAmountTest()
        {
            const double amount = 100000;
            const double paymentDiscountFactor = 0.92;
            const double periodAsTimesPerYear = 6; // ie every 2 months
            const double curveYearFraction = 0.5; // ie every 6 months
            const double expected = 2.29933398;
            double actual = SwapAnalytics.Delta1ForAnAmount(amount, paymentDiscountFactor, periodAsTimesPerYear, curveYearFraction);
            Assert.AreEqual(expected, actual, 1e-8);
        }

        /// <summary>
        ///A test for Delta0DiscountCoupon
        ///</summary>
        [TestMethod]
        public void Delta0DiscountCouponTest()
        {
            const double notional = 100;
            const double dayFraction = 0.5; // 6 months
            const double rate = 0.0615; // 6.15%
            const double paymentDiscountFactor = 0.92;
            const double expected = -0.00432963;
            double actual = SwapAnalytics.Delta0DiscountCoupon(notional, dayFraction, rate, paymentDiscountFactor);
            Assert.AreEqual(expected, actual, 1e-8);
        }

        /// <summary>
        ///A test for Delta0Coupon
        ///</summary>
        [TestMethod]
        public void Delta0CouponTest()
        {
            const double notional = 100;
            const double dayFraction = 0.5; // 6 months
            const double rate = 0.0615; // 6.15%
            const double paymentDiscountFactor = 0.92;
            const double expected = -0.0046;
            double actual = SwapAnalytics.Delta0Coupon(notional, dayFraction, rate, paymentDiscountFactor);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ComputeAnnuityFactor
        ///</summary>
        [TestMethod]
        public void ComputeAnnuityFactorTest()
        {
            const double forwardSwapRate = 0.0615; // 6.15%
            const double dfSwapStart = 0.95;
            const double dfSwapEnd = 0.80;
            const double Expected = 2.43902439;
            double actual = SwapAnalytics.ComputeAnnuityFactor(forwardSwapRate, dfSwapStart, dfSwapEnd);
            Assert.AreEqual(Expected, actual, 0.00000001);
        }
    }
}