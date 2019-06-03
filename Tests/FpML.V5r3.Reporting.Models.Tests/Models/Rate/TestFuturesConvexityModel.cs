using System.Diagnostics;
using Orion.Models.Rates.Futures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Models.Tests.Models.Rate
{
    [TestClass]
    public class TestFuturesConvexityModel
    {
        private readonly IFuturesAssetParameters analyticModelParameters
            = new FuturesAssetParameters
                  {
                      Rate = .05m,
                      Volatility = .2m,
                      TimeToExpiry = 3.0m,
                      YearFraction = .25m,
                      StartDiscountFactor = 1.0m,
                      EndDiscountFactor = 0.1m,
                  };

        [TestMethod]
        public void TestEuroDollarsConvexityModel()
        {
            var model = new EuroDollarFuturesAssetAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString());
            Debug.Print(convexityAdjustment.ToString());
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            Debug.Print(model.DiscountFactorAtMaturity.ToString());
            model.AnalyticParameters.EndDiscountFactor = model.DiscountFactorAtMaturity;
            var impliedQuote = (double)model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString());
            var rate = result + convexityAdjustment;
            Debug.Print(rate.ToString());
            Assert.AreEqual((double)rate, impliedQuote, 0.0000001);
        }

        [TestMethod]
        public void TestEuroYenConvexityModel()
        {
            var model = new EuroYenFuturesAssetAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString());
            Debug.Print(convexityAdjustment.ToString());
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString());
        }

        [TestMethod]
        public void TestEuroConvexityModel()
        {
            var model = new EuroFuturesAssetAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString());
            Debug.Print(convexityAdjustment.ToString());
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString());
        }

        [TestMethod]
        public void TestEuroSterlingConvexityModel()
        {
            var model = new EuroSterlingFuturesAssetAnalytic
                            {
                                AnalyticParameters = analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString());
            Debug.Print(convexityAdjustment.ToString());
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString());
        }

        //[TestMethod]
        //
        //public void TestBankBillsConvexityModel()
        //{
        //    var model = new BankBillsFuturesAssetAnalytic
        //                    {
        //                        AnalyticParameters = _analyticModelParameters
        //                    };
        //    var result = model.AdjustedRate;
        //    var convexityAdjustment = model.ConvexityAdjustment;
        //    Debug.Print(result.ToString());
        //    Debug.Print(convexityAdjustment.ToString());
        //    Assert.AreEqual(convexityAdjustment, 0.0004013431450656m);
        //    var impliedQuote = model.ImpliedQuote;
        //    Debug.Print(impliedQuote.ToString());
        //}

    }
}