using System.Diagnostics;
using FpML.V5r10.Reporting.Models.Rates.Coupons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.V5r10.Models.Tests.Models.Coupons
{
    [TestClass]
    public class TestRateCouponAnalytics
    {
        [TestMethod]
        public void FixedRateCouponAnalyticsDiscounted()
        {
            IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                {
                                                                    Rate = .05m,
                                                                    DiscountType = DiscountType.AFMA,
                                                                    YearFraction = 0.25m,
                                                                    NotionalAmount = 10000000m,
                                                                    HasReset = false,
                                                                    EndDiscountFactor = 0.9m,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = 1.0m,
                                                                    CurveYearFraction = 3.0m,
                                                                    PeriodAsTimesPerYear = 0.25m,
                                                                    DiscountRate= .05m
                                                                };
            var model = new FixedRateCouponAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
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
                                                                    EndDiscountFactor = 0.9m,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = 1.0m,
                                                                    CurveYearFraction = 3.0m
                                                                };
            var model = new FixedRateCouponAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
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

        [TestMethod]
        public void FloatingRateCouponAnalyicsDiscounted()
        {
            IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                {
                                                                    Rate = .05m,
                                                                    DiscountType = DiscountType.AFMA,
                                                                    YearFraction = 0.25m,
                                                                    NotionalAmount = 10000000m,
                                                                    HasReset = false,
                                                                    EndDiscountFactor = 0.9m,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = 1.0m,
                                                                    CurveYearFraction = 3.0m,
                                                                    DiscountRate = .05m
                                                                };
            var model = new FloatingRateCouponAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var value = analyticModelParameters.NotionalAmount * analyticModelParameters.Rate * analyticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = (double)model.Delta1;
            var delta0 = (double)model.Delta0;
            var expectedValue = (double)model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, 297, 10-6);
            Assert.AreEqual(delta0, -24.6913580246914, 10-6);
            Assert.AreEqual(expectedValue, 1000000, 10-6);
        }

        [TestMethod]
        public void FloatingRateCouponAnalyicsNonDiscounted()
        {
            IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                {
                                                                    //Rate = .1m,
                                                                    DiscountType = DiscountType.None,
                                                                    YearFraction = 2.0m,
                                                                    NotionalAmount = 10000000m,
                                                                    HasReset = false,
                                                                    EndDiscountFactor = 0.9m,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = 1.0m,
                                                                    CurveYearFraction = 0.25m
                                                                };
            var model = new FloatingRateCouponAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var value = analyticModelParameters.NotionalAmount * analyticModelParameters.Rate * analyticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = (double)model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = (double)model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, 27.5, 10-6);
            Assert.AreEqual(delta0, -1980);
            Assert.AreEqual(expectedValue, 1111111.11111, 10 - 6);
        }

        [TestMethod]
        public void FixedRateCouponAnalyicsDiscountedBuckettedDelta1()
        {
            IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                {
                                                                    //Rate = .05m,
                                                                    DiscountType = DiscountType.AFMA,
                                                                    YearFraction = 0.25m,
                                                                    NotionalAmount = 10000000m,
                                                                    HasReset = false,
                                                                    EndDiscountFactor = 0.9m,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    StartDiscountFactor = 1.0m,
                                                                    CurveYearFraction = 3.1m,
                                                                    PeriodAsTimesPerYear = 0.25m
                                                                };
            var model = new FixedRateCouponAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var value = analyticModelParameters.NotionalAmount * analyticModelParameters.Rate * analyticModelParameters.YearFraction;
            var result = model.BucketedDeltaVector2;
            var sum = 0.0m;
            foreach (var element in result)
            {
                Debug.Print(element.ToString());
//                Assert.AreEqual(element, -3.0178326474622770919067215333m);
            }
            foreach (var element in result)
            {
                sum += element;
                //                Assert.AreEqual(element, -3.0178326474622770919067215333m);
            }
            Debug.Print(sum.ToString());

        }
    }
}