using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using National.QRSC.QRLib.Tests.PricingStructures;
using National.QRSC.FpML.V47;

namespace National.QRSC.QRLib.Tests.Rates
{
    /// <summary>
    /// Summary description for SwaptionTest
    /// </summary>
    [TestClass]
    public class SwaptionTest
    {
        [TestMethod]
        public void ValueSwaptionTradeTest()
        {
            const string marketEnvironment = "ValueSwaptionTradeTest2";
            DateTime baseDate = new DateTime(DateTime.Today.Year, 05, 09);
            string rateCurveId = "RateCurve.AUD-LIBOR-BBA-3M";

            object[,] structurePropertiesRange = 
                {
                    {"PricingStructureType","RateCurve"},
                    {"IndexTenor","3M"},
                    {"Currency","AUD"},
                    {"Index","LIBOR-BBA"},
                    {"Algorithm","FastLinearZero"},
                    {"MarketName",marketEnvironment},
                    {"BuildDateTime",baseDate},
                    {"IndexName","AUD-LIBOR-BBA"},
                    {"CurveName","AUD-LIBOR-BBA-3M"},
                    {"Identifier",rateCurveId}
                };
            object[,] publishPropertiesRange = 
                {   {"ExpiryIntervalInMins", Storage.Common.ExpiryIntervalInMinutes},
                    {"MarketName",marketEnvironment}
                };
            object[,] values = 
                {   {"AUD-Deposit-1D",0.0285,0},
                    {"AUD-Deposit-1M",0.0285,0},
                    {"AUD-Deposit-2M",0.0285,0},
                    {"AUD-Deposit-3M",0.0285,0},
                    {"AUD-IRFuture-IR-Z9",0.0200,0},
                    {"AUD-IRFuture-IR-H0",0.0200,0},
                    {"AUD-IRFuture-IR-M0",0.0200,0},
                    {"AUD-IRFuture-IR-U0",0.0200,0},
                    {"AUD-IRFuture-IR-Z0",0.0200,0},
                    {"AUD-IRFuture-IR-H1",0.0200,0},
                    {"AUD-IRFuture-IR-M1",0.0200,0},
                    {"AUD-IRFuture-IR-U1",0.0200,0},
                    {"AUD-IRSwap-3Y",0.0400,0},
                    {"AUD-IRSwap-4Y",0.0400,0},
                    {"AUD-IRSwap-5Y",0.0400,0},
                    {"AUD-IRSwap-7Y",0.0400,0},
                    {"AUD-IRSwap-10Y",0.0400,0},
                    {"AUD-IRSwap-12Y",0.0400,0},
                    {"AUD-IRSwap-15Y",0.0400,0},
                    {"AUD-IRSwap-20Y",0.0400,0},
                    {"AUD-IRSwap-25Y",0.0400,0},
                    {"AUD-IRSwap-30Y",0.0400,0}
                };


            object[,] fxCurvePropertiesRange = 
            {
                {"PricingStructureType", "FxCurve"},
                {"IndexTenor", "6M"},
                {"Currency", "AUD"},
                {"QuoteCurrency", "USD"},
                {"QuoteBasis", "Currency1PerCurrency2"},
                {"Algorithm", "LinearForward"},
                {"MarketName", marketEnvironment},
                {"BuildDateTime", baseDate},
                {"BaseDate", baseDate},
                //{"CurrencyPair", "AUD-USD"},
                //{"CurveName", "AUD-USD"},
                {"Identifier", "AUD-USD"},
            };

            object[,] fxCurveValues = 
                {   {"AUDUSD-FxForward-1D",0.0285,0},
                    {"AUDUSD-FxSpot-SP",0.0285,0},
                    {"AUDUSD-FxForward-1M",0.0285,0},
                    {"AUDUSD-FxForward-2M",0.0285,0},
                    {"AUDUSD-FxForward-3M",0.0200,0},
                    {"AUDUSD-FxForward-6M",0.0200,0},
                    {"AUDUSD-FxForward-9M",0.0200,0},
                    {"AUDUSD-FxForward-1Y",0.0200,0},
                    {"AUDUSD-FxForward-2Y",0.0200,0},
                    {"AUDUSD-FxForward-3Y",0.0200,0},
                    {"AUDUSD-FxForward-4Y",0.0200,0},
                    {"AUDUSD-FxForward-5Y",0.0200,0},
                    {"AUDUSD-FxForward-6Y",0.0400,0},
                    {"AUDUSD-FxForward-7Y",0.0400,0},
                    {"AUDUSD-FxForward-8Y",0.0400,0},
                    {"AUDUSD-FxForward-9Y",0.0400,0},
                    {"AUDUSD-FxForward-10Y",0.0400,0},
                    {"AUDUSD-FxForward-12Y",0.0400,0},
                    {"AUDUSD-FxForward-15Y",0.0400,0},
                    {"AUDUSD-FxForward-20Y",0.0400,0},
                };

            object[,] rateCurveFilters = new object[,] 
            {   { "PricingStructureType", "RateCurve" }, 
                { "MarketName", marketEnvironment},
                { "Currency", "AUD" },
                { "IndexTenor", "3M" },
                { "Domain", "Highlander.Market" },
                { "Index", "LIBOR-BBA"}
            };

            object[,] fxCurveFilters = new object[,] 
            {   { "PricingStructureType", "FXCurve" }, 
                { "MarketName", marketEnvironment },
                { "Currency", "AUD" },
                { "QuoteCurrency", "USD"},
                { "Domain", "Highlander.Market" } 
            };

            object[,] volSurfaceFilters = new object[,]
                                              {
                                                  {"PricingStructureType", "RateVolatilityCube"},
                                                  {"MarketName", marketEnvironment},
                                                  {"Currency", "AUD"},
                                                  {"Instrument", "AUD-IRSwap"},
                                                  {"Domain", "Highlander.Market"},
                                                  {"Identifier", "UnitTestVolCube"},
                                                  {"CurveName", "AUD-Sydney-03/09/2009"},
                                                  {"BuildDateTime", baseDate},
                                                  {"Source", "SydneySwapDesk"},
                                                  {"Algorithm", "Linear"}
                                              };

            object[,] volData = new object[,]
                                    {
                                        {"3m", "1m", 0.11, 0.21, 0.31},
                                        {"6m", "1m", 0.12, 0.22, 0.32},
                                        {"3m", "3m", 0.115, 0.215, 0.315},
                                        {"6m", "3m", 0.125, 0.225, 0.325}
                                    };

            double[] strikes = new double[] {0.01, 0.02, 0.03};

            QRLib.PricingStructures.ClearCache();
            // Publish the test curves
            PricingStructuresTest.PublishCurveStructureWithProperties(structurePropertiesRange, publishPropertiesRange, values);
            PricingStructuresTest.PublishCurveStructureWithProperties(fxCurvePropertiesRange, publishPropertiesRange, fxCurveValues);
            Thread.Sleep(1000);

            string rateCurveRef = QRLib.PricingStructures.RequestPricingStructure(rateCurveFilters);
            string fxCurveRef = QRLib.PricingStructures.RequestPricingStructure(fxCurveFilters);
            string volsurfaceRef = QRLib.PricingStructures.CreateVolatilityCube(volSurfaceFilters, strikes, volData);

            Assert.AreEqual(string.Format("{0}.{1}-LIBOR-BBA-{2}", rateCurveFilters[0, 1], rateCurveFilters[2, 1], rateCurveFilters[3,1]), rateCurveRef);
            //Assert.AreEqual(string.Format("{0}.{1}-{2}", fxCurveFilters[0, 1], fxCurveFilters[2, 1], fxCurveFilters[3, 1]).ToLowerInvariant(), fxCurveRef.ToLowerInvariant());
            Assert.AreEqual(string.Format("{1}-{2}", fxCurveFilters[0, 1], fxCurveFilters[2, 1], fxCurveFilters[3, 1]).ToLowerInvariant(), fxCurveRef.ToLowerInvariant());

            DateTime cEffectiveDate = baseDate;
            DateTime cExerciseDate = new DateTime(cEffectiveDate.Year + 1, cEffectiveDate.Month, cEffectiveDate.Day);
            Double notional = -100000000.00;

            object[,] swaptionTerms = new object[,] 
            {   { "DealReferenceIdentifier", "TestSwaption" }, 
                { "PaymentDate", cEffectiveDate },
                { "ExerciseDate", cExerciseDate },
                { "BasePartyBuySellsInd", "Sells"},
                { "BasePartyName", "Bob" },
                { "CounterPartyName", "BobsCounterParty" },
                { "AdjustCalculationDates", true },
                { "BusinessCenters", "Sydney"},
                { "DateAdjustmentConvention", BusinessDayConventionEnum.FOLLOWING.ToString() },
                { "PaymentCurrency", "AUD" },
                { "PaymentAmount", 0},
                { "ResetInterval", "2D" },
                { "VolatilitySurfaceReference", volsurfaceRef}
            };

            object[,] swapTerms = new object[,] 
            {   { "SwapReferenceIdentifier", "TestSwap" }, 
                { "BasePartyPaysRecievesInd", "Pays" },
                { "CounterPartyName", "BobsCounterParty" },
                { "NotionalAmount", System.Math.Abs(notional)},
                { "NotionalCurrency", "AUD"},
                { "SwapMaturityTenor", "1Y" },
                { "AdjustCalculationDates", true },
                { "hasInitialExchange", false},
                { "hasFinallExchange", false},
                { "PrincipleExchangeAdjustementConvention", BusinessDayConventionEnum.FOLLOWING.ToString() },
                { "PrincipleExchangeBusinessCenters", "Sydney" },
                { "PayReceiveSpotRate", 1 },
            };

            object[,] payLegFixedTerms = new object[,] 
            {   { "StreamType", "Fixed" }, 
                { "NotionalAmount", notional },
                { "Currency", "AUD" },
                { "ScheduleGeneration", "Forward" },
                { "BusinessCenters", "Sydney" },
                { "CouponPeriod", "3M" },
                { "RollDay", "17" },
                { "CouponDateAdjustment", BusinessDayConventionEnum.FOLLOWING.ToString()},
                { "DayCountConvention", "ACT/365.FIXED"},
                { "DiscountingType", "Standard" },
                { "DiscountCurveReference", rateCurveRef },
                { "FixedOrObservedRate", 0.0 },
            };

            object[,] recLegFloatingTerms = new object[,] 
            {   { "StreamType", "Floating" }, 
                { "NotionalAmount", 100000000.00 },
                { "Currency", "AUD" },
                { "ScheduleGeneration", "Forward" },
                { "BusinessCenters", "Sydney" },
                { "CouponPeriod", "3M" },
                { "RollDay", "17" },
                { "CouponDateAdjustment", BusinessDayConventionEnum.FOLLOWING.ToString()},
                { "DayCountConvention", "ACT/365.FIXED"},
                { "DiscountingType", "Standard" },
                { "DiscountCurveReference", rateCurveRef },
                { "FixedOrObservedRate", 0 },
                { "ObservedRateSpecified", false },
                { "FixingDateAdjustmentConvention", BusinessDayConventionEnum.NONE.ToString() },
                { "FixingDateBusinessCenters", "Sydney" },
                { "FixingDateResetInterval", "0D" },
                { "ForwardCurveReference", rateCurveRef },
                { "RateIndexName", "AUD-LIBOR-BBA" },
                { "RateIndexTenor", "3M" },
                { "Spread", 0 },
            };

            object[,] payLegAmortisationStep = null;
            object[,] receiveLegAmortisationStep = null;
            object[,] payCouponStep = null;
            object[,] receiveCouponStep = null;

            string[] counterPartySettings = { "Large Corporate", "J", "1" };
            string[] regulatoryCapitalSettings = { "TIER1", "IR", "CORPORATE" };

            object[,] creditValuationSettings = new object[,] 
            {   { "Region", "Australia" },
                { "EcrsRating", 9 },
                { "CounterPartyType", "LARGE CORPORATE"},
                { "LendingCategory", "J" }
            };

            string[] requiredMetrics = { "NPV", "BreakEvenStrike"};
            DateTime valuationDate = cEffectiveDate;

            object[,] results = QRLib.Rates.InterestRateSwaption.GetValuation
                (
                    swaptionTerms,
                    swapTerms,
                    payLegFixedTerms,
                    recLegFloatingTerms,
                    payLegAmortisationStep,
                    receiveLegAmortisationStep,
                    payCouponStep,
                    receiveCouponStep,
                    creditValuationSettings,
                    marketEnvironment,
                    requiredMetrics,
                    valuationDate
                );

            Assert.AreEqual("NPV", results[0, 0]);
            Assert.IsNotNull(results[0, 1]);
            Assert.AreEqual("BreakEvenStrike", results[1, 0]);
            Assert.IsNotNull(results[1, 1]);

            swaptionTerms[10, 1] = System.Math.Abs((decimal)results[0, 1]);
            payLegFixedTerms[11, 1] = results[1, 1];
            results = QRLib.Rates.InterestRateSwaption.GetValuation
            (
                swaptionTerms,
                swapTerms,
                payLegFixedTerms,
                recLegFloatingTerms,
                payLegAmortisationStep,
                receiveLegAmortisationStep,
                payCouponStep,
                receiveCouponStep,
                creditValuationSettings,
                marketEnvironment,
                requiredMetrics,
                valuationDate
            );
            Assert.AreEqual(results[0, 0], "NPV");
            Assert.IsNotNull(results[0, 1]);
            Assert.AreEqual(results[1, 0], "BreakEvenStrike");
            Assert.IsNotNull(results[1, 1]);

            QRLib.PricingStructures.ClearCache();

        }
    }
}
