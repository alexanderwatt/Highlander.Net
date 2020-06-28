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

using System.Diagnostics;
using System.Globalization;
using Highlander.Reporting.Models.V5r3.Rates.Futures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Models.Tests.V5r3.Rate
{
    [TestClass]
    public class TestFuturesConvexityModel
    {
        private readonly IRateFuturesAssetParameters _analyticModelParameters
            = new RateFuturesAssetParameters
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
                                AnalyticParameters = _analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString(CultureInfo.InvariantCulture));
            Debug.Print(convexityAdjustment.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            Debug.Print(model.DiscountFactorAtMaturity.ToString(CultureInfo.InvariantCulture));
            model.AnalyticParameters.EndDiscountFactor = model.DiscountFactorAtMaturity;
            var impliedQuote = (double)model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString(CultureInfo.InvariantCulture));
            var rate = result + convexityAdjustment;
            Debug.Print(rate.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual((double)rate, impliedQuote, 0.0000001);
        }

        [TestMethod]
        public void TestEuroYenConvexityModel()
        {
            var model = new EuroYenFuturesAssetAnalytic
                            {
                                AnalyticParameters = _analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString(CultureInfo.InvariantCulture));
            Debug.Print(convexityAdjustment.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TestEuroConvexityModel()
        {
            var model = new EuroFuturesAssetAnalytic
                            {
                                AnalyticParameters = _analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString(CultureInfo.InvariantCulture));
            Debug.Print(convexityAdjustment.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TestEuroSterlingConvexityModel()
        {
            var model = new EuroSterlingFuturesAssetAnalytic
                            {
                                AnalyticParameters = _analyticModelParameters
                            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString(CultureInfo.InvariantCulture));
            Debug.Print(convexityAdjustment.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(convexityAdjustment, 0.0004786305830711m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TestBankBillsConvexityModel()
        {
            var model = new BankBillsFuturesAssetAnalytic
            {
                AnalyticParameters = _analyticModelParameters
            };
            var result = model.AdjustedRate;
            var convexityAdjustment = model.ConvexityAdjustment;
            Debug.Print(result.ToString(CultureInfo.InvariantCulture));
            Debug.Print(convexityAdjustment.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(convexityAdjustment, 0.0004013431450656m);
            var impliedQuote = model.ImpliedQuote;
            Debug.Print(impliedQuote.ToString(CultureInfo.InvariantCulture));
        }
    }
}