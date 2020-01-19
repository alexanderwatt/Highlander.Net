using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.ExcelAPI.Tests.Products
{
    [TestClass]
    public class PriceableAssetsTest
    {
        [TestMethod]
        public void GetStructuredSwapMetricTest()
        {
            const string marketName = "UnitTest";
            const string currency = "AUD";
            DateTime baseDate = new DateTime(2009, 1, 6);
            string curveId = CreateCurve(marketName, currency, baseDate);
            const Decimal notional = 1000000;
            DateTime[] dates = {
                                        new DateTime(2009, 03, 05),
                                        new DateTime(2009, 06, 05),
                                        new DateTime(2009, 09, 05),
                                        new DateTime(2009, 12, 05),
                                    };
            Decimal[] notionalWeights = { 0.8m, 0.7m, 0.6m };
            const string fixedLegDayCount = "ACT/365.FIXED";
            const Decimal fixedRate = 0.06m;
            const string businessDayConvention = "FOLLOWING";
            const string businessCentersAsString = "GBLO";
            const string metric = "DeltaR";
            Decimal actual = PriceableAssets.GetStructuredSwapMetric(marketName, curveId, currency, baseDate, notional, dates, notionalWeights, fixedLegDayCount, fixedRate, businessDayConvention, businessCentersAsString, metric);
            const double expected = 51.737;
            Assert.AreEqual(expected, (double)actual, 0.001d);
        }

        /// <summary>
        ///A test for GetStructuredSwapMetric
        ///</summary>
        [TestMethod]
        public void GetSwapImpliedQuoteTest()
        {
            const string marketName = "UnitTest";
            const string currency = "AUD";
            DateTime baseDate = new DateTime(2009, 1, 6);
            DateTime spotDate = new DateTime(2009, 1, 8);
            string curveId = CreateCurve(marketName, currency, baseDate);
            const string fixedLegDayCount = "ACT/365.FIXED";
            const string term = "3y";
            const string paymentFrequency = "1M";
            const string businessDayConvention = "NONE";
            const string businessCentersAsString = "GBLO";
            Decimal actual = PriceableAssets.GetSwapImpliedQuote(marketName, curveId, currency, baseDate, spotDate,
                fixedLegDayCount, term, paymentFrequency, businessDayConvention, businessCentersAsString);
            const double expected = 0.0454;
            Assert.AreEqual(expected, (double)actual, 0.0001);
        }

        private static string CreateCurve(string marketEnvironmentId, string currency, DateTime baseDate)
        {
            const string index = "LIBOR-BBA";
            const string indexTenor = "6M";
            object[,] pricingStructurePropertiesRange =
                {
                    {"PricingStructureType", "RateCurve"},
                    {"MarketName", marketEnvironmentId},
                    {"IndexTenor", indexTenor},
                    {"Currency", currency},
                    {"Index", index},
                    {"Algorithm", "Default"},
                    {"BaseDate", baseDate},
                };

            object[,] values = 
                { 
                    {"AUD-Deposit-1D",0.03,	0},
                    {"AUD-Deposit-1M", 0.0311,	0},
                    {"AUD-Deposit-2M", 0.0311,	0},
                    {"AUD-Deposit-88D", 0.0311,	0},
                    {"AUD-IRFuture-IR-U9", (100.0 - 96.895) / 100.0,	0}, // IR01
                    {"AUD-IRFuture-IR-Z9", (100.0 - 96.825)/100.0,	0}, // IR02
                    {"AUD-IRFuture-IR-H0", (100.0 - 96.545)/100.0,	0}, // IR03
                    {"AUD-IRFuture-IR-M0", (100.0 - 96.115)/100.0,	0}, //IR04
                    {"AUD-IRFuture-IR-U0", (100.0 - 95.665)/100.0,	0}, // IR05
                    {"AUD-IRFuture-IR-Z0", (100.0 - 95.285)/100.0,	0}, // IR06
                    {"AUD-IRFuture-IR-H1", (100.0 - 94.955)/100.0,	0}, // IR07
                    {"AUD-IRFuture-IR-M1", (100.0 - 94.645)/100.0,	0}, //IR08
                    {"AUD-IRSwap-3Y", 0.0455749999999999,	0},
                    {"AUD-IRSwap-4Y", 0.0500999999999999,	0},
                    {"AUD-IRSwap-5Y", 0.0526999999999999,	0},
                    {"AUD-IRSwap-7Y", 0.0558,	0},
                    {"AUD-IRSwap-10Y", 0.057975,	0},
                    {"AUD-IRSwap-15Y", 0.059725,	0},
                    {"AUD-IRSwap-20Y", 0.058825,	0},
                    {"AUD-IRSwap-25Y", 0.057675,	0},
                    {"AUD-IRSwap-26Y", 0.05741,	0},
                    {"AUD-IRSwap-27Y", 0.057145,	0},
                    {"AUD-IRSwap-28Y", 0.05688,	0},
                    {"AUD-IRSwap-29Y", 0.056615,	0},
                    {"AUD-IRSwap-30Y", 0.05635,	0}
                };
            string id = QRLib.PricingStructures.CreatePricingStructure(pricingStructurePropertiesRange, values);
            return id;
        }
    }
}
