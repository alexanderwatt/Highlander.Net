
using System.Diagnostics;

using NUnit.Framework;

using nabCap.QR.AnalyticModels.Rates;


namespace nabCap.QR.AnalyticModels.Tests.Models.Coupons
{
    [TestFixture]
    public class TestCouponAnalyics
    {
        private const string cDayCountFraction = "ACT/365.FIXED";

        //static IUnityContainer container = null;

        [NUnit.Framework.SetUp]
        public void LoadTestData()
        {
            //container = TestHelper.SetUpContainer();
        }

        [NUnit.Framework.Test]
        [Category("Coupons")]
        public void FixedRateCouponAnalyicsDiscounted()
        {
            IRateCouponParameters anlayticModelParameters = new RateCouponParameters();
            anlayticModelParameters.Rate = .05m;
            anlayticModelParameters.IsDiscounted = true;
            anlayticModelParameters.YearFraction = 0.25m;
            anlayticModelParameters.NotionalAmount = 10000000m;
            anlayticModelParameters.HasReset = false;
            anlayticModelParameters.EndDiscountFactor = 0.9m;
            anlayticModelParameters.PaymentDiscountFactor = 0.99m;
            anlayticModelParameters.StartDiscountFactor = 1.0m;
            anlayticModelParameters.CurveYearFraction = 3.0m;
            anlayticModelParameters.PeriodAsTimesPerYear = 0.25m;
            var model = new FixedRateCouponAnalytic
                            {
                                AnalyticParameters = anlayticModelParameters
                            };
            var value = anlayticModelParameters.NotionalAmount*anlayticModelParameters.Rate*anlayticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, -36.2139917695473251028806584m);
            Assert.AreEqual(delta0, 0.0m);
            Assert.AreEqual(expectedValue, 123456.79012345679012345679m);
        }

        [NUnit.Framework.Test]
        [Category("Coupons")]
        public void FixedRateCouponAnalyicsNonDiscounted()
        {
            IRateCouponParameters anlayticModelParameters = new RateCouponParameters();
            anlayticModelParameters.Rate = .05m;
            anlayticModelParameters.IsDiscounted = false;
            anlayticModelParameters.YearFraction = 0.25m;
            anlayticModelParameters.NotionalAmount = 10000000m;
            anlayticModelParameters.HasReset = false;
            anlayticModelParameters.EndDiscountFactor = 0.9m;
            anlayticModelParameters.PaymentDiscountFactor = 0.99m;
            anlayticModelParameters.StartDiscountFactor = 1.0m;
            anlayticModelParameters.CurveYearFraction = 3.0m;
            var model = new FixedRateCouponAnalytic
                            {
                                AnalyticParameters = anlayticModelParameters
                            };
            var value = anlayticModelParameters.NotionalAmount * anlayticModelParameters.Rate * anlayticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, -37.125m);
            Assert.AreEqual(delta0, 0.0m);
            Assert.AreEqual(expectedValue, 125000m);
        }

        [NUnit.Framework.Test]
        [Category("Coupons")]
        public void FloatingRateCouponAnalyicsDiscounted()
        {
            IRateCouponParameters anlayticModelParameters = new RateCouponParameters();
            anlayticModelParameters.Rate = .05m;
            anlayticModelParameters.IsDiscounted = true;
            anlayticModelParameters.YearFraction = 0.25m;
            anlayticModelParameters.NotionalAmount = 10000000m;
            anlayticModelParameters.HasReset = false;
            anlayticModelParameters.EndDiscountFactor = 0.9m;
            anlayticModelParameters.PaymentDiscountFactor = 0.99m;
            anlayticModelParameters.StartDiscountFactor = 1.0m;
            anlayticModelParameters.CurveYearFraction = 3.0m;
            var model = new FloatingRateCouponAnalytic
                            {
                                AnalyticParameters = anlayticModelParameters
                            };
            var value = anlayticModelParameters.NotionalAmount * anlayticModelParameters.Rate * anlayticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, -36.66666666666666666666666663m);
            Assert.AreEqual(delta0, 2444444.444444444444444444442m);
            Assert.AreEqual(expectedValue, 123456.79012345679012345679m);
        }

        [NUnit.Framework.Test]
        [Category("Coupons")]
        public void FloatingRateCouponAnalyicsNonDiscounted()
        {
            IRateCouponParameters anlayticModelParameters = new RateCouponParameters();
            anlayticModelParameters.Rate = .05m;
            anlayticModelParameters.IsDiscounted = false;
            anlayticModelParameters.YearFraction = 0.25m;
            anlayticModelParameters.NotionalAmount = 10000000m;
            anlayticModelParameters.HasReset = false;
            anlayticModelParameters.EndDiscountFactor = 0.9m;
            anlayticModelParameters.PaymentDiscountFactor = 0.99m;
            anlayticModelParameters.StartDiscountFactor = 1.0m;
            anlayticModelParameters.CurveYearFraction = 3.0m;
            var model = new FloatingRateCouponAnalytic
                            {
                                AnalyticParameters = anlayticModelParameters
                            };
            var value = anlayticModelParameters.NotionalAmount * anlayticModelParameters.Rate * anlayticModelParameters.YearFraction;
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, -37.125m);
            Assert.AreEqual(delta0, 2475000m);
            Assert.AreEqual(expectedValue, 125000m);
        }

        [NUnit.Framework.Test]
        [Category("Coupons")]
        public void FixedRateCouponAnalyicsDiscountedBuckettedDelta1()
        {
            IRateCouponParameters anlayticModelParameters = new RateCouponParameters();
            anlayticModelParameters.Rate = .05m;
            anlayticModelParameters.IsDiscounted = true;
            anlayticModelParameters.YearFraction = 0.25m;
            anlayticModelParameters.NotionalAmount = 10000000m;
            anlayticModelParameters.HasReset = false;
            anlayticModelParameters.EndDiscountFactor = 0.9m;
            anlayticModelParameters.PaymentDiscountFactor = 0.99m;
            anlayticModelParameters.StartDiscountFactor = 1.0m;
            anlayticModelParameters.CurveYearFraction = 3.0m;
            anlayticModelParameters.PeriodAsTimesPerYear = 0.25m;
            var model = new FixedRateCouponAnalytic
            {
                AnalyticParameters = anlayticModelParameters
            };
            var value = anlayticModelParameters.NotionalAmount * anlayticModelParameters.Rate * anlayticModelParameters.YearFraction;
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