using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using National.QRSC.QRLib.Rates;
using National.QRSC.Engine.Tests.Helpers;
using National.QRSC.QRLib.Tests.PricingStructures;

namespace National.QRSC.QRLib
{
    /// <summary>
    ///This is a test class for CrossCurrencySwapTest and is intended
    ///to contain all CrossCurrencySwapTest Unit Tests
    ///</summary>
    [TestClass]
    public class CrossCurrencySwapTest
    {
        /// <summary>
        ///A test for EvaluateMetrics
        ///</summary>
        [TestMethod]
        public void EvaluateMetricsTest()
        {
            PricingStructuresTest pricingStructuresTest = new PricingStructuresTest();
            pricingStructuresTest.CreatePricingStructureTest();

            CrossCurrencySwapTest crossCurrencySwapTest = new CrossCurrencySwapTest();
            crossCurrencySwapTest.CreateAmortisedSwapTest();

            string[] swapProductReferenceKeys = { "SWAP-1" };

            string[] requestedMetrics = { "BreakEvenRate" };

            string marketEnvironmentId = PricingStructuresTest.MarketEnvironmentId;
            DateTime valuationDate = Convert.ToDateTime("15/05/2009");
            bool withHeaders = false;
            object[,] actual = (object[,])IRSwap.EvaluateMetrics(swapProductReferenceKeys, requestedMetrics, marketEnvironmentId, valuationDate, withHeaders);
            Assert.AreEqual(0.0407, (double)(decimal)actual[0, 0], 0.0001);
        }

        /// <summary>
        ///A test for CreateAmortisedSwap
        ///</summary>
        [TestMethod()]
        public void CreateAmortisedSwapTest()
        {
            object[,] swapTermsList
                = new object[,]
                      {
                          {"SwapReferenceIdentifier", "SWAP-1"},
                          {"EffectiveDate", Convert.ToDateTime("15/05/2009")},
                          {"TerminationDate", Convert.ToDateTime("15/05/2014")},
                          {"AdjustCalculationDates", true},
                          {"BasePartyPaysRecievesInd", "Pays"},
                          {"CounterpartyName", "Telstra"},
                          {"QuotedCurrencyPair", "AUD-AUD"},
                          {"NotionalAmount", 1000000},
                          {"NotionalCurrency", "AUD"},
                          {"PayReceiveSpotRate", 1},
                          {"hasInitialExchange", false},
                          {"hasFinallExchange", false},
                          {"principleExchangeBusinessCenters", "Sydney"},
                          {"principleExchangeAdjustementConvention", "MODFOLLOWING"}
                      };

            object[,] payLegTermsList
                = new object[,]
                      {
                          {"StreamType", "Fixed"},
                          {"NotionalAmount", 1000000.00},
                          {"Currency", "AUD"},
                          {"ScheduleGeneration", "Forward"},
                          {"BusinessCenters", "Sydney"},
                          {"CouponPeriod", "6m"},
                          {"RollDay", "1"},
                          {"CouponDateAdjustment", "FOLLOWING"},
                          {"DayCountConvention", "ACT/365.FIXED"},
                          {"DiscountingType", "Standard"},
                          {"DiscountCurveReference", "RateCurve.AUD-LIBOR-BBA-3M"},
                          {"FixedOrObservedRate", 0.06},
                          {"ObservedRateSpecified", false},
                          {"ForwardCurveReference", "RateCurve.AUD-LIBOR-BBA-3M"},
                          {"FixingDateBusinessCenters", "Sydney"},
                          {"FixingDateResetInterval", "0D"},
                          {"FixingDateAdjustmentConvention", "NONE"},
                          {"RateIndexName", "AUD-LIBOR-BBA"},
                          {"RateIndexTenor", "6m"},
                          {"Spread", 0.0022}
                      };

            object[,] receiveLegTermsList = new object[,] 
            {
                {"StreamType", "Floating"},
                {"NotionalAmount", 1000000},
                {"Currency", "AUD"},
                {"ScheduleGeneration", "Forward"},
                {"BusinessCenters", "Sydney"},
                {"CouponPeriod", "6m"},
                {"RollDay", "1"},
                {"CouponDateAdjustment", "FOLLOWING"},
                {"DayCountConvention", "ACT/365.FIXED"},
                {"DiscountingType", "Standard"},
                {"DiscountCurveReference", "RateCurve.AUD-LIBOR-BBA-3M"},
                {"FixedOrObservedRate", 0},
                {"ObservedRateSpecified", false},
                {"ForwardCurveReference", "RateCurve.AUD-LIBOR-BBA-3M"},
                {"FixingDateBusinessCenters", "Sydney"},
                {"FixingDateResetInterval", "0D"},
                {"FixingDateAdjustmentConvention", "NONE"},
                {"RateIndexName", "AUD-LIBOR-BBA"},
                {"RateIndexTenor", "6m"},
                {"Spread", 0.0007}
            };

            object[,] payLegAmortisationStepSchedule = null; // TODO: Initialize to an appropriate value
            object[,] receiveLegAmortisationStepSchedule = null; // TODO: Initialize to an appropriate value
            object[,] payCouponStepSchedule = null; // TODO: Initialize to an appropriate value
            object[,] receiveCouponStepSchedule = null; // TODO: Initialize to an appropriate value
            string expected = "SWAP-1";
            string actual;
            actual = IRSwap.CreateAmortisedSwap(swapTermsList, payLegTermsList, receiveLegTermsList, payLegAmortisationStepSchedule, receiveLegAmortisationStepSchedule, payCouponStepSchedule, receiveCouponStepSchedule);
            Assert.AreEqual(expected, actual);
        }
    }
}
