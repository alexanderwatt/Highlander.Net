using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using National.QRSC.Credit.Analytics.Credit;
using National.QRSC.Credit.Analytics.CreditMetrics;
using National.QRSC.Engine.Factory;
using National.QRSC.Engine.PricingStructures;
using National.QRSC.ExcelWrapper.Helpers;
using National.QRSC.ModelFramework;
using National.QRSC.ObjectCache;
using National.QRSC.QRLib.Rates;
using National.QRSC.FpML.V47;
using National.QRSC.Products.Helpers;
using National.QRSC.Products.InterestRates;
using National.QRSC.Products.InterestRates.Adapters;
using National.QRSC.QRLib.Tests.PricingStructures;
using National.QRSC.Utility.NamedValues;
using National.QRSC.Engine.Helpers;
using InterestRateSwap=National.QRSC.Products.InterestRates.InterestRateSwap;
using RangeExtension = National.QRSC.QRLib.Helpers.RangeExtension;

namespace National.QRSC.QRLib.Tests.Rates
{
    [TestClass]
    public class InterestRateSwapTest
    {
        [TestMethod]
        public void BasicValuationTest()
        {
            // Reset the market
            QRLib.PricingStructures.ClearCache();

            const string marketName = "BasicValuationTest1";
            const string rateCurveId = "RateCurve.AUD-LIBOR-BBA-6M";
            
            DateTime baseDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddYears(5);
            const string businessCenters = "Sydney";

            object[,] structurePropertiesRange
                = {
                      {"PricingStructureType", "RateCurve"},
                      {"IndexTenor", "6M"},
                      {"Currency", "AUD"},
                      {"Index", "LIBOR-BBA"},
                      {"Algorithm", "FastLinearZero"},
                      {"MarketName", marketName},
                      {"BuildDateTime", baseDate},
                      {"IndexName", "AUD-LIBOR-BBA"},
                      {"CurveName", "AUD-LIBOR-BBA-6M"},
                      {"Identifier", rateCurveId}
                  };
            object[,] publishPropertiesRange
                = {
                      {"ExpiryIntervalInMins", 1},
                      {"MarketName", marketName}
                  };
            object[,] values
                = {
                      {"AUD-Deposit-1D", 0.0485, 0},
                      {"AUD-Deposit-1M", 0.0467, 0},
                      {"AUD-Deposit-2M", 0.0481, 0},
                      {"AUD-Deposit-3M", 0.0455, 0},
                      //{"AUD-IRFuture-IR-0", 0.0475, 0},
                      //{"AUD-IRFuture-IR-U9", 0.0464, 0},
                      //{"AUD-IRFuture-IR-Z9", 0.0476, 0},
                      //{"AUD-IRFuture-IR-H0", 0.0466, 0},
                      //{"AUD-IRFuture-IR-M0", 0.0439, 0},
                      //{"AUD-IRFuture-IR-U0", 0.0461, 0},
                      //{"AUD-IRFuture-IR-Z0", 0.0498, 0},
                      //{"AUD-IRFuture-IR-H1", 0.0442, 0},
                      {"AUD-IRSwap-3Y", 0.0488, 0},
                      {"AUD-IRSwap-4Y", 0.0476, 0},
                      {"AUD-IRSwap-5Y", 0.0496, 0},
                      {"AUD-IRSwap-7Y", 0.0454, 0},
                      {"AUD-IRSwap-10Y", 0.0457, 0},
                      {"AUD-IRSwap-12Y", 0.0499, 0},
                      {"AUD-IRSwap-15Y", 0.0411, 0},
                      {"AUD-IRSwap-20Y", 0.0422, 0},
                      {"AUD-IRSwap-25Y", 0.0433, 0},
                      {"AUD-IRSwap-30Y", 0.0453, 0}
                  };

            // Publish the test curves
            PricingStructuresTest.PublishCurveStructureWithProperties(structurePropertiesRange, publishPropertiesRange, values);

            // Retrieve and store the test curve
            string[] curveIds = QRLib.PricingStructures.LoadPricingStructures(structurePropertiesRange);
            Assert.AreEqual(rateCurveId, curveIds.Single());

            string curveId = QRLib.PricingStructures.ListAllPricingStructuresByMarket(marketName)[0];
            Assert.AreEqual(rateCurveId, curveId);

            object[,] swapTerms
                = new object[,]
                      {
                          {"SwapReferenceIdentifier", "TestSwap"},
                          {"EffectiveDate", baseDate},
                          {"TerminationDate", endDate},
                          {"AdjustCalculationDates", "True"},
                          {"BasePartyPaysRecievesInd", "Pays"},
                          {"CounterpartyName", "TestCounterParty"},
                          {"AdjustCalculationDates", "True"},
                          {"BasePartyPaysRecievesInd", "Pays"},
                          {"CounterpartyName", "TestCounterParty"},
                          {"QuotedCurrencyPair", "AUD-AUD"},
                          {"NotionalAmount", 1000000d},
                          {"NotionalCurrency", "AUD"},
                          {"PayReceiveSpotRate", 1d},
                          {"hasInitialExchange", false},
                          {"hasFinallExchange", false},
                          {"principleExchangeBusinessCenters", businessCenters},
                          {"principleExchangeAdjustementConvention", BusinessDayConventionEnum.FOLLOWING.ToString()},
                          {"SalesMargin", 0d}
                      };

            object[,] payLegFixedTerms = new object[,] 
                                             {   { "StreamType", "Fixed" }, 
                                                 { "NotionalAmount", 1000000d },
                                                 { "Currency", "AUD" },
                                                 { "ScheduleGeneration", "Forward" },
                                                 { "BusinessCenters", "Sydney" },
                                                 { "CouponPeriod", "6M" },
                                                 { "RollDay", "17" },
                                                 { "CouponDateAdjustment", BusinessDayConventionEnum.FOLLOWING.ToString()},
                                                 { "DayCountConvention", "ACT/365.FIXED"},
                                                 { "DiscountingType", "Standard" },
                                                 { "DiscountCurveReference", rateCurveId },
                                                 { "FixedOrObservedRate", 0 },
                                             };

            object[,] recLegFloatingTerms = new object[,] 
                                                {   { "StreamType", "Floating" }, 
                                                    { "NotionalAmount", 1000000.00 },
                                                    { "Currency", "AUD" },
                                                    { "ScheduleGeneration", "Forward" },
                                                    { "BusinessCenters", "Sydney" },
                                                    { "CouponPeriod", "6M" },
                                                    { "RollDay", "17" },
                                                    { "CouponDateAdjustment", BusinessDayConventionEnum.FOLLOWING.ToString()},
                                                    { "DayCountConvention", "ACT/365.FIXED"},
                                                    { "DiscountingType", "Standard" },
                                                    { "DiscountCurveReference",rateCurveId},
                                                    { "FixedOrObservedRate", 0 },
                                                    { "ObservedRateSpecified", false },
                                                    { "FixingDateAdjustmentConvention", BusinessDayConventionEnum.NONE.ToString() },
                                                    { "FixingDateBusinessCenters", "Sydney" },
                                                    { "FixingDateResetInterval", "0D" },
                                                    { "ForwardCurveReference", rateCurveId},
                                                    { "RateIndexName", "AUD-LIBOR-BBA" },
                                                    { "RateIndexTenor", "3M" },
                                                    { "Spread",  0},
                                                };

            string[] requiredMetrics = { "NPV", "BreakEvenRate", "ROE", "PCROE", "PCE.MaxValue" };
            object[,] creditValuationSettings = new object[,] 
                                                    {   { "Region", "Australia" },
                                                        { "EcrsRating", 5 },
                                                        { "CounterPartyType", "LARGE CORPORATE"},
                                                        { "LendingCategory", "J" },
                                                        { "AUDToBaseCurrencySpotRate", 1d }
                                                    };

            object[,] valuationResults = IRSwap.GetValuation
                (
                ATMBasisType.Pay,
                swapTerms,
                payLegFixedTerms,
                recLegFloatingTerms,
                null, 
                null,
                null,
                null, 
                creditValuationSettings,
                marketName,
                requiredMetrics,
                baseDate
                );
            Assert.AreNotEqual("EXCEPTION", valuationResults[0,0]);

            creditValuationSettings = new object[,] 
                                          {   { "Region", "Australia" },
                                              { "EcrsRating", 9 },
                                              { "CounterPartyType", "LARGE CORPORATE"},
                                              { "LendingCategory", "J" },
                                              { "AUDToBaseCurrencySpotRate", 1d }
                                          };

            valuationResults = IRSwap.GetValuation
                (
                ATMBasisType.Pay,
                swapTerms,
                payLegFixedTerms,
                recLegFloatingTerms,
                null,
                null,
                null,
                null,
                creditValuationSettings,
                marketName,
                requiredMetrics,
                baseDate
                );
            Assert.AreNotEqual("EXCEPTION", valuationResults[0, 0]);
        }

        [TestMethod]
        public void CreateStructuredSwapTest()
        {
            #region Initialise

            QRLib.PricingStructures.ClearCache();

            #endregion

            #region Build the Market

            const string marketEnvironmentId = "SwapTest";
            DateTime baseDate = new DateTime(DateTime.Today.Year, 05, 09);
            string rateCurveId = "RateCurve.AUD-LIBOR-BBA-6M";

            object[,] structurePropertiesRange = 
                {
                    {"PricingStructureType","RateCurve"},
                    {"IndexTenor","6M"},
                    {"Currency","AUD"},
                    {"Index","LIBOR-BBA"},
                    {"Algorithm","FastLinearZero"},
                    {"MarketName",marketEnvironmentId},
                    {"BuildDateTime",baseDate},
                    {"IndexName","AUD-LIBOR-BBA"},
                    {"CurveName","AUD-LIBOR-BBA-6M"},
                    {"Identifier",rateCurveId}
                };

            object[,] values = 
                {   {"AUD-Deposit-1D",0.0285,0},
                    {"AUD-Deposit-1M",0.0285,0},
                    {"AUD-Deposit-2M",0.0285,0},
                    {"AUD-Deposit-3M",0.0285,0},
                    {"AUD-IRFuture-IR-M9",0.0200,0},
                    {"AUD-IRFuture-IR-U9",0.0200,0},
                    {"AUD-IRFuture-IR-Z9",0.0200,0},
                    {"AUD-IRFuture-IR-H0",0.0200,0},
                    {"AUD-IRFuture-IR-M0",0.0200,0},
                    {"AUD-IRFuture-IR-U0",0.0200,0},
                    {"AUD-IRFuture-IR-Z0",0.0200,0},
                    {"AUD-IRFuture-IR-H1",0.0200,0},
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

            string rateCurveRef = QRLib.PricingStructures.CreatePricingStructure(structurePropertiesRange, values);
            IMarketEnvironment marketEnvironment = ObjectCacheHelper.GetMarket(marketEnvironmentId);

            #endregion

            #region Define the Swap
            object[,] swapTermsList = new object[,] 
                                          {
                                              {"SwapReferenceIdentifier", "SWAP-1"},
                                              {"PayReceiveSpotRate", 1},
                                          };

            object[,] payLegTermsList = new object[,] 
                                            {
                                                {"StreamType", "Fixed"},
                                                {"Currency", "AUD"},
                                                {"DayCountConvention", "ACT/365.FIXED"},
                                                {"DiscountingType", "Standard"},
                                                {"DiscountCurveReference", rateCurveRef},
                                                {"FixedOrObservedRate", 0.06},
                                            };

            DateTime[] startDates = { };
            DateTime[] endDates = { };
            Decimal[] notionals = { };
            Decimal[] rates = { 0.067m, 0.067m, 0.067m, 0.067m };

            CreateCouponMatrix(4 ,3, baseDate, out startDates, out endDates, out notionals);
            int rows = startDates.Length + 1;
            object[,] payMatrix = new object[rows, 4];
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                if (rowIndex == 0)
                {
                    payMatrix[rowIndex, 0] = "StartDate";
                    payMatrix[rowIndex, 1] = "EndDate";
                    payMatrix[rowIndex, 2] = "NotionalAmount";
                    payMatrix[rowIndex, 3] = "FixedOrObservedRate";
                }
                else
                {
                    payMatrix[rowIndex, 0] = startDates[rowIndex - 1];
                    payMatrix[rowIndex, 1] = endDates[rowIndex - 1];
                    payMatrix[rowIndex, 2] = notionals[rowIndex - 1];
                    payMatrix[rowIndex, 3] = rates[rowIndex - 1];
                }
            }

            object[,] receiveLegTermsList = new object[,] 
                                                {
                                                    {"StreamType", "Floating"},
                                                    {"Currency", "AUD"},
                                                    {"DayCountConvention", "ACT/365.FIXED"},
                                                    {"DiscountingType", "Standard"},
                                                    {"DiscountCurveReference", rateCurveRef},
                                                    {"FixedOrObservedRate", 0},
                                                    {"ForwardCurveReference", rateCurveRef},
                                                    {"RateIndexName", "AUD-LIBOR-BBA"},
                                                    {"RateIndexTenor", "6m"},
                                                    {"Spread", 0.0007}
                                                };

            object[,] recMatrix = new object[rows, 4];
            Decimal[] spreads = { 0.0m, 0.0m, 0.0m, 0.0m };
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                if (rowIndex == 0)
                {
                    recMatrix[rowIndex, 0] = "StartDate";
                    recMatrix[rowIndex, 1] = "EndDate";
                    recMatrix[rowIndex, 2] = "NotionalAmount";
                    recMatrix[rowIndex, 3] = "Spread";
                }
                else
                {
                    recMatrix[rowIndex, 0] = startDates[rowIndex - 1];
                    recMatrix[rowIndex, 1] = endDates[rowIndex - 1];
                    recMatrix[rowIndex, 2] = notionals[rowIndex - 1];
                    recMatrix[rowIndex, 3] = spreads[rowIndex - 1];
                }
            }

            #endregion

            #region Define the Valuation Criteria

            string[] requiredMetrics = { "NPV", "BreakEvenRate", "PayBreakEven", "ReceiveBreakEven" };
            DateTime valuationDate = DateTime.Today;
            
            #endregion

            #region Create & Value the Swap

            string expected = "SWAP-1";
            string swapId = QRLib.Rates.IRSwap.CreateStructuredSwap(swapTermsList, payLegTermsList, payMatrix, null, receiveLegTermsList, recMatrix, null);
            Assert.AreEqual(expected, swapId);
            InterestRateSwap swap = ProductHelper.Get<InterestRateSwap>(swapId);
            Assert.IsNotNull(swap);

            CreditSettings creditSettings = new CreditSettings(ServerStore.Client);
            IDictionary<string, object> valuation = swap.BasicValuation(valuationDate, marketEnvironment, requiredMetrics, creditSettings);

            #endregion

            #region Validate the Results

            Assert.IsNotNull(valuation["BreakEvenRate"]);
            Assert.IsNotNull(valuation["NPV"]);

            #endregion 

            #region Reset

            QRLib.PricingStructures.ClearCache();

            #endregion
        }

        [TestMethod]
        public void StructuredSwapRiskValuationTest()
        {
            #region Initialise

            QRLib.PricingStructures.ClearCache();

            #endregion

            #region Build the Market

            const string marketEnvironmentId = "SwapTest";
            DateTime baseDate = new DateTime(DateTime.Today.Year, 05, 09);
            string rateCurveId = "RateCurve.AUD-LIBOR-BBA-6M";

            object[,] structurePropertiesRange = 
                {
                    {"PricingStructureType","RateCurve"},
                    {"IndexTenor","6M"},
                    {"Currency","AUD"},
                    {"Index","LIBOR-BBA"},
                    {"Algorithm","FastLinearZero"},
                    {"MarketName",marketEnvironmentId},
                    {"BuildDateTime",baseDate},
                    {"IndexName","AUD-LIBOR-BBA"},
                    {"CurveName","AUD-LIBOR-BBA-6M"},
                    {"Identifier",rateCurveId}
                };

            object[,] values = 
                {   {"AUD-Deposit-1D",0.0285,0},
                    {"AUD-Deposit-1M",0.0285,0},
                    {"AUD-Deposit-2M",0.0285,0},
                    {"AUD-Deposit-3M",0.0285,0},
                    {"AUD-IRFuture-IR-M9",0.0200,0},
                    {"AUD-IRFuture-IR-U9",0.0200,0},
                    {"AUD-IRFuture-IR-Z9",0.0200,0},
                    {"AUD-IRFuture-IR-H0",0.0200,0},
                    {"AUD-IRFuture-IR-M0",0.0200,0},
                    {"AUD-IRFuture-IR-U0",0.0200,0},
                    {"AUD-IRFuture-IR-Z0",0.0200,0},
                    {"AUD-IRFuture-IR-H1",0.0200,0},
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

            string rateCurveRef = QRLib.PricingStructures.CreatePricingStructure(structurePropertiesRange, values);
            IMarketEnvironment marketEnvironment = ObjectCacheHelper.GetMarket(marketEnvironmentId);

            #endregion

            #region Define the Swap
            object[,] swapTermsList = new object[,] 
                                          {
                                              {"SwapReferenceIdentifier", "SWAP-1"},
                                              {"PayReceiveSpotRate", 1},
                                              {"BasePartyPaysRecievesInd", "Pays"},
                                          };

            object[,] payLegTermsList = new object[,] 
                                            {
                                                {"StreamType", "Fixed"},
                                                {"Currency", "AUD"},
                                                {"DayCountConvention", "ACT/365.FIXED"},
                                                {"DiscountingType", "Standard"},
                                                {"DiscountCurveReference", rateCurveRef},
                                                {"FixedOrObservedRate", 0.06},
                                                {"RateIndexName", "AUD-LIBOR-BBA"},
                                                {"RateIndexTenor", "6m"},
                                            };

            DateTime[] startDates = { };
            DateTime[] endDates = { };
            Decimal[] notionals = { };
            Decimal[] rates = { 0.067m, 0.067m, 0.067m, 0.067m };

            CreateCouponMatrix(4, 3, baseDate, out startDates, out endDates, out notionals);
            int rows = startDates.Length + 1;
            object[,] payMatrix = new object[rows, 4];
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                if (rowIndex == 0)
                {
                    payMatrix[rowIndex, 0] = "StartDate";
                    payMatrix[rowIndex, 1] = "EndDate";
                    payMatrix[rowIndex, 2] = "NotionalAmount";
                    payMatrix[rowIndex, 3] = "FixedOrObservedRate";
                }
                else
                {
                    payMatrix[rowIndex, 0] = startDates[rowIndex - 1];
                    payMatrix[rowIndex, 1] = endDates[rowIndex - 1];
                    payMatrix[rowIndex, 2] = notionals[rowIndex - 1];
                    payMatrix[rowIndex, 3] = rates[rowIndex - 1];
                }
            }

            object[,] receiveLegTermsList = new object[,] 
                                                {
                                                    {"StreamType", "Floating"},
                                                    {"Currency", "AUD"},
                                                    {"DayCountConvention", "ACT/365.FIXED"},
                                                    {"DiscountingType", "Standard"},
                                                    {"DiscountCurveReference", rateCurveRef},
                                                    {"FixedOrObservedRate", 0},
                                                    {"ForwardCurveReference", rateCurveRef},
                                                    {"RateIndexName", "AUD-LIBOR-BBA"},
                                                    {"RateIndexTenor", "6m"},
                                                    {"Spread", 0.0007}
                                                };

            object[,] recMatrix = new object[rows, 4];
            Decimal[] spreads = { 0.0m, 0.0m, 0.0m, 0.0m };
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                if (rowIndex == 0)
                {
                    recMatrix[rowIndex, 0] = "StartDate";
                    recMatrix[rowIndex, 1] = "EndDate";
                    recMatrix[rowIndex, 2] = "NotionalAmount";
                    recMatrix[rowIndex, 3] = "Spread";
                }
                else
                {
                    recMatrix[rowIndex, 0] = startDates[rowIndex - 1];
                    recMatrix[rowIndex, 1] = endDates[rowIndex - 1];
                    recMatrix[rowIndex, 2] = notionals[rowIndex - 1];
                    recMatrix[rowIndex, 3] = spreads[rowIndex - 1];
                }
            }

            #endregion

            #region Define the Valuation Criteria

            string[] requiredMetrics = { "NPV", "BreakEvenRate", "PayBreakEven", "ReceiveBreakEven", "ROE", "PCROE" };
            ICreditValuationSettings creditValuationSettings =
                new CreditValuationSettings
                    {
                        AUDToBaseCurrencySpotRate = 1.0m,
                        CounterpartyType = "LARGE CORPORATE",
                        EcrsRating = 5,
                        LendingCategory = "J",
                        Region = "Australia",
                    };

            DateTime valuationDate = DateTime.Today;

            #endregion

            #region Create & Value the Swap

            string expected = "SWAP-1";
            string swapId = QRLib.Rates.IRSwap.CreateStructuredSwap(swapTermsList, payLegTermsList, payMatrix, null, receiveLegTermsList, recMatrix, null);

            Assert.AreEqual(expected, swapId);
            InterestRateSwap swap = ProductHelper.Get<InterestRateSwap>(swapId);

            Assert.IsNotNull(swap);
            CreditSettings creditSettings = new CreditSettings(ServerStore.Client);
            IDictionary<string, object> valuation = swap.RiskValuation(valuationDate, marketEnvironment, requiredMetrics, creditValuationSettings, creditSettings);

            #endregion

            #region Validate the Results

            Assert.IsNotNull(valuation["BreakEvenRate"]);
            Assert.IsNotNull(valuation["NPV"]);
            Assert.IsNotNull(valuation["ROE"]);

            #endregion

            #region Reset

            QRLib.PricingStructures.ClearCache();

            #endregion
        }


        [TestMethod]
        public void TargetROEMTMTest()
        {
            Decimal targetRoe = 20.0m;
            DateTime effectiveDate = new DateTime(2010, 2, 4);
            DateTime termDate = new DateTime(2020, 2, 4);

            TradeData tradeData
                = new TradeData
                      {
                          SpecifyRateToAUD = true,
                          RateToAUD = 1.0m,
                          EvalDate = effectiveDate,
                          StartDate = effectiveDate,
                          MaturityDate = termDate,
                          ECRSRating = 1,
                          LendingCategory = "J",
                          Region = "Australia",
                          CounterpartyType = "Large Corporate",
                          FaceValue = 1000000.0m,
                          Product = "Interest Rate Swaps",
                          ProductCategory = "IR",
                          PrimaryCurrency = "AUD",
                          SettleCurrency = "AUD",
                          BusinessUnit = "Markets",
                          MTM = Decimal.Zero
                      };

            MtmTargetRoe targetMtmCalculator = new MtmTargetRoe(tradeData, "SYDWDDQUR02", "CreditConfig", null);

            decimal audResult =
                Solvers.SolveForTargetValueByBrent
                    (Decimal.Zero,      //interval lower bound
                     100000M,    //interval upper bound                                         
                     0.1M,       //numerical tolerance
                     1000,       //max iterations
                     targetRoe,
                     targetMtmCalculator
                    );

            tradeData
                = new TradeData
                      {
                          SpecifyRateToAUD = true,
                          RateToAUD = 1,
                          EvalDate = effectiveDate,
                          StartDate = effectiveDate,
                          MaturityDate = termDate,
                          ECRSRating = 1,
                          LendingCategory = "J",
                          Region = "Australia",
                          CounterpartyType = "Large Corporate",
                          FaceValue = 1000000.0m,
                          MTM = audResult,
                          Product = "Interest Rate Swaps",
                          ProductCategory = "IR",
                          PrimaryCurrency = "AUD",
                          SettleCurrency = "AUD",
                          BusinessUnit = "Markets"
                      };

            Decimal[] regCapVector;
            decimal result = ROEDataManager.NormalROECalc(tradeData, "SYDWDDQUR02", "CreditConfig", null, out regCapVector);

        }

        [TestMethod]
        public void PartialDifferentialHedgeTest()
        {
            DateTime baseDate = new DateTime(2010, 01, 15);

            #region Build the market

            const string MarketEnvironmentId = "SensitivityTest";
            object[,] properties1 =  {  
                                         {"PricingStructureType", "RateCurve"},
                                         {"MarketName", MarketEnvironmentId},
                                         {"IndexTenor", "6M"},
                                         {"Currency", "AUD"},
                                         {"Index", "LIBOR-BBA"},
                                         {"Algorithm", "FastLinearZero"},
                                         {"BaseDate", baseDate},
                                         {"Identifier", "Discounting"}
                                        
                                     };

            object[,] data1 =        { 
                                         {"AUD-Deposit-1D",	0.038950, 0},
                                         {"AUD-Deposit-1M",	0.041050, 0},
                                         {"AUD-Deposit-2M",	0.041050, 0},
                                         {"AUD-Deposit-3M",	0.041350, 0},
                                         {"AUD-IRFuture-IR-H0",	0.044499, 0},
                                         {"AUD-IRFuture-IR-M0",	0.048693, 0},
                                         {"AUD-IRFuture-IR-U0",	0.051581, 0},
                                         {"AUD-IRFuture-IR-Z0",	0.053562, 0},
                                         {"AUD-IRFuture-IR-H1",	0.055535, 0},
                                         {"AUD-IRFuture-IR-M1",	0.057299, 0},
                                         {"AUD-IRFuture-IR-U1",	0.058656, 0},
                                         {"AUD-IRFuture-IR-Z1",	0.059657, 0},
                                         {"AUD-IRSwap-3Y",	0.054166, 0},
                                         {"AUD-IRSwap-4Y",	0.056308, 0},
                                         {"AUD-IRSwap-5Y",	0.057841, 0},
                                         {"AUD-IRSwap-6Y",	0.059071, 0},
                                         {"AUD-IRSwap-7Y",	0.060111, 0},
                                         {"AUD-IRSwap-8Y",	0.060788, 0},
                                         {"AUD-IRSwap-9Y",	0.061209, 0},
                                         {"AUD-IRSwap-10Y",	0.061558, 0},
                                         {"AUD-IRSwap-12Y",	0.062216, 0},
                                         {"AUD-IRSwap-15Y",	0.062342, 0},
                                         {"AUD-IRSwap-20Y",	0.061624, 0},
                                         {"AUD-IRSwap-25Y",	0.059987, 0},
                                         {"AUD-IRSwap-30Y",	0.058313, 0}
                                     };
            NamedValueSet structureProperties1 = RangeExtension.ToNamedValueSet(properties1);
            IPricingStructure pricingStructure1 = PricingStructureFactory.CreatePricingStructure(structureProperties1, data1);
            string rateCurveRef1 = pricingStructure1.GetPricingStructureId().Id;
            ObjectCacheHelper.SetPricingStructureInMarket(MarketEnvironmentId, rateCurveRef1, pricingStructure1);

            #endregion

            #region Define the Swap

            object[,] swapTermsList = new object[,] 
                                          {
                                              {"SwapReferenceIdentifier", "SWAP-1"},
                                              {"PayReceiveSpotRate", 1},
                                              {"BasePartyPaysRecievesInd", "Pays"},
                                          };

            object[,] payLegTermsList = new object[,] 
                                            {
                                                {"StreamType", "Fixed"},
                                                {"Currency", "AUD"},
                                                {"DayCountConvention", "ACT/365.FIXED"},
                                                {"DiscountingType", "Standard"},
                                                {"DiscountCurveReference", rateCurveRef1},
                                            };

            IInterestRateStream stream = new Stream();
            IStructuredCouponSpec[] streamspec = stream.BuildStructuredSchedule
                ( baseDate,
                  new DateTime(baseDate.Year + 10, baseDate.Month, baseDate.Day), 
                  -1000000.0m,
                  0.0615016084809535875878951592M,
                  0.0m, 
                  "Forward", 
                  "Sydney", 
                  BusinessDayConventionEnum.FOLLOWING, 
                  "6M", 
                  RollConventionEnum.Item15, 
                  DiscountingTypeEnum.Standard, null);


            int rowIndex = 0;
            object[,] payMatrix = new object[streamspec.Length+1, 4];
            payMatrix[rowIndex, 0] = "StartDate";
            payMatrix[rowIndex, 1] = "EndDate";
            payMatrix[rowIndex, 2] = "NotionalAmount";
            payMatrix[rowIndex, 3] = "FixedOrObservedRate";

            rowIndex++;
            foreach (IStructuredCouponSpec item in streamspec)
            {
                payMatrix[rowIndex, 0] = item.StartDate;
                payMatrix[rowIndex, 1] = item.EndDate;
                payMatrix[rowIndex, 2] = item.NotionalAmount;
                payMatrix[rowIndex, 3] = item.FixedOrObservedRate;
                rowIndex++;
            }

            object[,] receiveLegTermsList = new object[,] 
                                                {
                                                    {"StreamType", "Floating"},
                                                    {"Currency", "AUD"},
                                                    {"DayCountConvention", "ACT/365.FIXED"},
                                                    {"DiscountingType", "Standard"},
                                                    {"DiscountCurveReference", rateCurveRef1},
                                                    {"ForwardCurveReference", rateCurveRef1},
                                                    {"RateIndexName", "AUD-LIBOR-BBA"},
                                                    {"RateIndexTenor", "6M"},
                                                };

            streamspec = stream.BuildStructuredSchedule
                ( baseDate,
                  new DateTime(baseDate.Year + 10, baseDate.Month, baseDate.Day),
                  1000000.0m,
                  0.0M,
                  0.0m,
                  "Forward",
                  "Sydney",
                  BusinessDayConventionEnum.FOLLOWING,
                  "6M",
                  RollConventionEnum.Item15,
                  DiscountingTypeEnum.Standard, null);

            rowIndex = 0;
            object[,] recMatrix = new object[streamspec.Length+1, 4];
            recMatrix[rowIndex, 0] = "StartDate";
            recMatrix[rowIndex, 1] = "EndDate";
            recMatrix[rowIndex, 2] = "NotionalAmount";
            recMatrix[rowIndex, 3] = "Spread";
            rowIndex++;

            foreach (IStructuredCouponSpec item in streamspec)
            {
                recMatrix[rowIndex, 0] = item.StartDate;
                recMatrix[rowIndex, 1] = item.EndDate;
                recMatrix[rowIndex, 2] = item.NotionalAmount;
                recMatrix[rowIndex, 3] = item.Spread;
                rowIndex++;
            }

            #endregion

            #region run sensitivity test

            const string expected = "SWAP-1";
            string swapId = IRSwap.CreateStructuredSwap(swapTermsList, payLegTermsList, payMatrix, null, receiveLegTermsList, recMatrix, null);
            Assert.AreEqual(expected, swapId);

            object[,] results
                = IRSwap.PartialDifferentialHedge(swapId, baseDate, MarketEnvironmentId,
                                                             new string[] {rateCurveRef1}, 1, false);

            Assert.IsNotNull(results);
            Assert.AreEqual(rateCurveRef1, results[0, 0]);
            Assert.AreEqual(1, results.GetUpperBound(1));  // Check that there is only one curve returned
            Assert.AreEqual(data1.GetUpperBound(0), results.GetUpperBound(0) - 1);
            Assert.AreEqual(0m, results[results.GetUpperBound(0), 1]);

            #endregion
        }

        // this is an integration test as it relies on curves being published.
        //[TestMethod]
        public void PartialDifferentialHedgeEodTest()
        {
            DateTime baseDate = new DateTime(2010, 01, 15);

            #region Load the markets

            const string MarketEnvironmentId = "EOD";
            object[,] properties1 =  {  
                                         {"MarketName", MarketEnvironmentId},
                                         {"PricingStructureType", "RateCurve"},
                                     };

            var ids = QRLib.PricingStructures.LoadPricingStructures(properties1);

            #endregion

            #region Define the Swap

            object[,] swapTermsList = new object[,] 
                                          {
                                              {"SwapReferenceIdentifier", "SWAP-1"},
                                              {"PayReceiveSpotRate", 1},
                                              {"BasePartyPaysRecievesInd", "Pays"},
                                          };

            object[,] payLegTermsList = new object[,]
                                            {
                                                {"StreamType", "Floating"},
                                                {"Currency", "AUD"},
                                                {"DayCountConvention", "ACT/365.FIXED"},
                                                {"DiscountingType", "Standard"},
                                                {"DiscountCurveReference", "RateCurve.USD-ZERO-BANK-3M"},
                                                {"ForwardCurveReference", "RateCurve.USD-ZERO-BANK-3M"},
                                                {"RateIndexName", "USD-LIBOR-BBA"},
                                                {"RateIndexTenor", "3M"},
                                            };

            IInterestRateStream stream = new Stream();
            IStructuredCouponSpec[] streamspec = stream.BuildStructuredSchedule
                (baseDate,
                  new DateTime(baseDate.Year + 10, baseDate.Month, baseDate.Day),
                  -1000000.0m,
                  0.0615016084809535875878951592M,
                  0.0m,
                  "Forward",
                  "Sydney",
                  BusinessDayConventionEnum.FOLLOWING,
                  "6M",
                  RollConventionEnum.Item15,
                  DiscountingTypeEnum.Standard, null);


            int rowIndex = 0;
            object[,] payMatrix = new object[streamspec.Length + 1, 4];
            payMatrix[rowIndex, 0] = "StartDate";
            payMatrix[rowIndex, 1] = "EndDate";
            payMatrix[rowIndex, 2] = "NotionalAmount";
            payMatrix[rowIndex, 3] = "FixedOrObservedRate";

            rowIndex++;
            foreach (IStructuredCouponSpec item in streamspec)
            {
                payMatrix[rowIndex, 0] = item.StartDate;
                payMatrix[rowIndex, 1] = item.EndDate;
                payMatrix[rowIndex, 2] = item.NotionalAmount;
                payMatrix[rowIndex, 3] = item.FixedOrObservedRate;
                rowIndex++;
            }

            object[,] receiveLegTermsList = new object[,] 
                                                {
                                                    {"StreamType", "Floating"},
                                                    {"Currency", "AUD"},
                                                    {"DayCountConvention", "ACT/365.FIXED"},
                                                    {"DiscountingType", "Standard"},
                                                    {"DiscountCurveReference", "RateCurve.AUD-XCCY-BASIS-3M"},
                                                    {"ForwardCurveReference", "RateCurve.AUD-ZERO-BANK-3M"},
                                                    {"RateIndexName", "AUD-LIBOR-BBA"},
                                                    {"RateIndexTenor", "6M"},
                                                };

            streamspec = stream.BuildStructuredSchedule
                (baseDate,
                  new DateTime(baseDate.Year + 10, baseDate.Month, baseDate.Day),
                  1000000.0m,
                  0.0M,
                  0.0m,
                  "Forward",
                  "Sydney",
                  BusinessDayConventionEnum.FOLLOWING,
                  "6M",
                  RollConventionEnum.Item15,
                  DiscountingTypeEnum.Standard, null);

            rowIndex = 0;
            object[,] recMatrix = new object[streamspec.Length + 1, 4];
            recMatrix[rowIndex, 0] = "StartDate";
            recMatrix[rowIndex, 1] = "EndDate";
            recMatrix[rowIndex, 2] = "NotionalAmount";
            recMatrix[rowIndex, 3] = "Spread";
            rowIndex++;

            foreach (IStructuredCouponSpec item in streamspec)
            {
                recMatrix[rowIndex, 0] = item.StartDate;
                recMatrix[rowIndex, 1] = item.EndDate;
                recMatrix[rowIndex, 2] = item.NotionalAmount;
                recMatrix[rowIndex, 3] = item.Spread;
                rowIndex++;
            }

            #endregion

            #region run sensitivity test

            const string expected = "SWAP-1";
            string swapId = IRSwap.CreateStructuredSwap(swapTermsList, payLegTermsList, payMatrix, null, receiveLegTermsList, recMatrix, null);
            Assert.AreEqual(expected, swapId);

            object[,] results
                = IRSwap.PartialDifferentialHedge(swapId, baseDate, MarketEnvironmentId,
                                                             new string[] { "RateCurve.AUD-XCCY-BASIS-3M" }, 1, false);

            Assert.IsNotNull(results);

            #endregion
        }

        [TestMethod]
        public void CreateSummedCurveTest()
        {
            Dictionary<string, decimal> curve1
                = new Dictionary<string, decimal>
                      {
                          {"Item1", 1},
                          {"Item3", 2},
                          {"Item4", 4.1m}
                      };

            Dictionary<string, decimal> curve2
                = new Dictionary<string, decimal>
                      {
                          {"Item2", 2},
                          {"Item4", 4},
                          {"Item5", 5}
                      };

            Dictionary<string, Dictionary<string, decimal>> curves
                = new Dictionary<string, Dictionary<string, decimal>> 
                {
                    {"Curve1" ,curve1},
                    {"Curve2" ,curve2}
                };
            Dictionary<string, decimal> newCurve = CurveHelper.CreateSummedCurve(curves);

            Assert.AreEqual(5, newCurve.Count);
            decimal item4 = newCurve["Item4"];
            Assert.AreEqual(8.1m, item4);
        }


        [TestMethod]
        public void GetVanillaSwapCouponSchedule()
        {
            const string marketName = "VanillaSwapTest";
            const string curveId = "TestCurve";

            DateTime baseDate = new DateTime(2009, 05, 09);
            DateTime endDate = new DateTime(2014, 05, 09);
            const string businessCenters = "Sydney";

            object[,] structurePropertiesRange
                = {
                      {"BaseDate", new DateTime(2009, 03, 01)},
                      {"PricingStructureType", "RateCurve"},
                      {"IndexTenor", "6M"},
                      {"Currency", "AUD"},
                      {"Index", "LIBOR-BBA"},
                      {"Algorithm", "FastLinearZero"},
                      {"MarketName", marketName},
                      {"IndexName", "AUD-LIBOR-BBA"},
                      {"CurveName", "AUD-LIBOR-BBA-6M"},
                      {"Identifier", curveId}
                  };
            object[,] values
                    = {
                      {"AUD-Deposit-1D", 0.0485, 0},
                      {"AUD-Deposit-1M", 0.0467, 0},
                      {"AUD-Deposit-2M", 0.0481, 0},
                      {"AUD-Deposit-3M", 0.0455, 0},
                      {"AUD-IRFuture-IR-M9", 0.0475, 0},
                      {"AUD-IRFuture-IR-U9", 0.0464, 0},
                      {"AUD-IRFuture-IR-Z9", 0.0476, 0},
                      {"AUD-IRFuture-IR-H0", 0.0466, 0},
                      {"AUD-IRFuture-IR-M0", 0.0439, 0},
                      {"AUD-IRFuture-IR-U0", 0.0461, 0},
                      {"AUD-IRFuture-IR-Z0", 0.0498, 0},
                      {"AUD-IRFuture-IR-H1", 0.0442, 0},
                      {"AUD-IRSwap-3Y", 0.0488, 0},
                      {"AUD-IRSwap-4Y", 0.0476, 0},
                      {"AUD-IRSwap-5Y", 0.0496, 0},
                      {"AUD-IRSwap-7Y", 0.0454, 0},
                      {"AUD-IRSwap-10Y", 0.0457, 0},
                      {"AUD-IRSwap-12Y", 0.0499, 0},
                      {"AUD-IRSwap-15Y", 0.0411, 0},
                      {"AUD-IRSwap-20Y", 0.0422, 0},
                      {"AUD-IRSwap-25Y", 0.0433, 0},
                      {"AUD-IRSwap-30Y", 0.0453, 0}
                  };


            object[,] swapTerms
     = new object[,]
                      {
                          {"SwapReferenceIdentifier", "TestSwap"},
                          {"EffectiveDate", baseDate},
                          {"TerminationDate", endDate},
                          {"AdjustCalculationDates", "True"},
                          {"BasePartyPaysRecievesInd", "Pays"},
                          {"CounterpartyName", "TestCounterParty"},
                          {"AdjustCalculationDates", "True"},
                          {"BasePartyPaysRecievesInd", "Pays"},
                          {"CounterpartyName", "TestCounterParty"},
                          {"QuotedCurrencyPair", "AUD-AUD"},
                          {"NotionalAmount", Convert.ToDouble(1000000)},
                          {"NotionalCurrency", "AUD"},
                          {"PayReceiveSpotRate", Convert.ToDouble(1)},
                          {"hasInitialExchange", true},
                          {"hasFinallExchange", true},
                          {"principleExchangeBusinessCenters", businessCenters},
                          {"principleExchangeAdjustementConvention", BusinessDayConventionEnum.FOLLOWING.ToString()},
                          {"SalesMargin", 0.0d}
                      };

            object[,] payLegFixedTerms = new object[,] 
                                             {   { "StreamType", "Fixed" }, 
                                                 { "NotionalAmount", 1000000.00 },
                                                 { "Currency", "AUD" },
                                                 { "ScheduleGeneration", "Forward" },
                                                 { "BusinessCenters", "Sydney" },
                                                 { "CouponPeriod", "6M" },
                                                 { "RollDay", "17" },
                                                 { "CouponDateAdjustment", BusinessDayConventionEnum.FOLLOWING.ToString()},
                                                 { "DayCountConvention", "ACT/365.FIXED"},
                                                 { "DiscountingType", "Standard" },
                                                 { "DiscountCurveReference", curveId },
                                                 { "FixedOrObservedRate", 0 },
                                             };

            object[,] recLegFloatingTerms = new object[,] 
                                                {   { "StreamType", "Floating" }, 
                                                    { "NotionalAmount", 1000000.00 },
                                                    { "Currency", "AUD" },
                                                    { "ScheduleGeneration", "Forward" },
                                                    { "BusinessCenters", "Sydney" },
                                                    { "CouponPeriod", "6M" },
                                                    { "RollDay", "17" },
                                                    { "CouponDateAdjustment", BusinessDayConventionEnum.FOLLOWING.ToString()},
                                                    { "DayCountConvention", "ACT/365.FIXED"},
                                                    { "DiscountingType", "Standard" },
                                                    { "DiscountCurveReference",curveId},
                                                    { "FixedOrObservedRate", 0 },
                                                    { "ObservedRateSpecified", false },
                                                    { "FixingDateAdjustmentConvention", BusinessDayConventionEnum.NONE.ToString() },
                                                    { "FixingDateBusinessCenters", "Sydney" },
                                                    { "FixingDateResetInterval", "0D" },
                                                    { "ForwardCurveReference", curveId},
                                                    { "RateIndexName", "AUD-LIBOR-BBA" },
                                                    { "RateIndexTenor", "3M" },
                                                    { "Spread",  0},
                                                };

            // Reset the market
            QRLib.PricingStructures.ClearCache();
            string result = QRLib.PricingStructures.ListAllPricingStructures()[0];
            Assert.IsTrue(result.StartsWith("Warning: no pricing structures were found"));
            QRLib.PricingStructures.CreatePricingStructure(structurePropertiesRange, values);

            string[] scheduleFields = {"AdjustedStartDate", "AdjustedEndDate", "Notional", "Rate", "Margin"};
            Double[] indexes = {1,2};
            object schedule = IRSwap.CreateVanillaSwapSchedule(swapTerms, payLegFixedTerms, recLegFloatingTerms, true, marketName, baseDate, scheduleFields, indexes, false); 
        }

        private static void CreateCouponMatrix(int cycles, int months, DateTime refDate, out DateTime[] startDates, out DateTime[] endDates, out Decimal[] notionals)
        {
            DateTime baseDate = refDate.AddMonths(-1 * months);
            List<DateTime> startDatesList = new List<DateTime>();
            List<DateTime> endDatesList = new List<DateTime>();

            for (int index = 1; index <= cycles; index++)
                startDatesList.Add(baseDate.AddMonths(months * index));

            for (int index = 1; index <= cycles; index++)
                endDatesList.Add(startDatesList[0].AddMonths(months * index));

            startDates = startDatesList.ToArray();
            endDates = endDatesList.ToArray();

            List<Decimal> amounts = new List<decimal>();
            for (int index = 0; index < 20; index++)
                amounts.Add(1000000.00m);

            notionals = amounts.ToArray();
        }
    }
}