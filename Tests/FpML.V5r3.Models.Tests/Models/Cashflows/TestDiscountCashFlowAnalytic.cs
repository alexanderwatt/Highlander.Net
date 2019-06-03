using System.Diagnostics;
using Orion.Models.Rates.Coupons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.V5r3.Models.Tests.Models.Cashflows
{
    [TestClass]
    public class TestDiscountCashFlowAnalytic
    {
        [TestMethod]
        public void FixedRateCouponAnalyicsDiscounted()
        {
            IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                {
                                                                    Rate = .05m,
                                                                    DiscountType = DiscountType.AFMA,
                                                                    YearFraction = 0.25m,
                                                                    NotionalAmount = 10000000m,
                                                                    HasReset = false,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = .999m,
                                                                    EndDiscountFactor = 0.99m,
                                                                    CurveYearFraction = 3.0m,
                                                                    PeriodAsTimesPerYear = 0.25m,
                                                                    DiscountRate = .05m 
                                                                };
            var model = new FixedRateCouponAnalytic {AnalyticParameters = analyticModelParameters};
            var value = analyticModelParameters.NotionalAmount*analyticModelParameters.Rate*analyticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, 36.666666666666666666666666630m);
            Assert.AreEqual(delta0, 0.0m);
            Assert.AreEqual(expectedValue, 123456.79012345679012345679m);
        }

        [TestMethod]
        public void FixedRateCouponAnalyicsNonDiscounted()
        {
            IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                {
                                                                    Rate = .05m,
                                                                    DiscountType = DiscountType.None,
                                                                    YearFraction = 0.25m,
                                                                    NotionalAmount = 10000000m,
                                                                    HasReset = false,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = .999m,
                                                                    EndDiscountFactor = 0.99m,
                                                                    CurveYearFraction = 3.0m
                                                                };
            var model = new FixedRateCouponAnalytic();
            model.AnalyticParameters = analyticModelParameters;
            var value = analyticModelParameters.NotionalAmount * analyticModelParameters.Rate * analyticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, 37.125m);
            Assert.AreEqual(delta0, 0.0m);
            Assert.AreEqual(expectedValue, 125000m);
        }

    }
}