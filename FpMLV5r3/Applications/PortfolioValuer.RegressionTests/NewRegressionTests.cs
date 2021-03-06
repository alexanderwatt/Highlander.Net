﻿/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.PVRegression.TestData.V5r3;
using Highlander.Reporting.Contracts.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.UnitTestEnv.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Serialisation;
using Highlander.Workflow;
using Highlander.Workflow.CurveGeneration.V5r3;
using Highlander.Workflow.PortfolioValuation.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Exception = System.Exception;

namespace Highlander.PVRegression.Tests.V5r3 
{
    [TestClass]
    public class NewRegressionTests
    {
        #region Constants, Fields

        private static readonly DateTime TestBaseDate = new DateTime(2010, 8, 30);
        private static readonly string[] TestMarketNames = { CurveConst.TEST_EOD };

        const decimal ToleranceNPV = 1000.0M;

        #endregion

        #region Properties

        private static TimeSpan Retention { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        private static CurveEngine.V5r3.CurveEngine Engine { get; set; }
        private static IDictionary<string, ExpectedValue> ExpectedValues { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            Engine = new CurveEngine.V5r3.CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            // stress all loaded curves
            StressAllFxCurves();
            StressAllRateCurves();
            //load expected values
            ExpectedValues = LoadExpectedValues();
            // Set the Retention
            Retention = TimeSpan.FromHours(1);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            //Logger.Dispose()
        }

        #endregion

        #region Helpers

        #region Internal Struct

        internal struct ExpectedValue
        {
            /// <summary>
            /// Current value produced by source system (e.g. Calypso or Murex).
            /// </summary>
            public decimal SourceNPV { get; set; }

            /// <summary>
            /// Current value produced by the analytics (allowing for known/acceptable issues).
            /// </summary>
            public decimal OutputNPV { get; set; }

            /// <summary>
            /// Optimal target value (when all issues/inaccuracies are resolved).
            /// </summary>
            public decimal TargetNPV { get; set; }
        }

        #endregion

        #region Class Initialization Helpers


        private static int GetColumn(List<string> headings, string heading)
        {
            int result = headings.IndexOf(heading);
            if (result < 0)
                throw new ArgumentException("Unknown heading", heading);
            return result;
        }

        /// <summary>
        /// Load the expected values for the test trades
        /// </summary>
        /// <returns></returns>
        private static IDictionary<string, ExpectedValue> LoadExpectedValues()
        {
            // load expected values
            IDictionary<string, ExpectedValue> expectedValues = new Dictionary<string, ExpectedValue>();
            using (var csvFile = new StringReader(RegressionTestDataLoader.GetExpectedResults("ExpectedNPVs.csv")))
            {
                // CSV headers
                int colSystem = -1;
                int colTradeId = -1;
                int colMarketName = -1;
                int colMetricSourceNpv = -1;
                int colMetricOutputNpv = -1;
                int colIRScenario = -1;
                int colFXScenario = -1;
                int colStressNPVDelta = -1;
                // Line Id
                int lineNum = 0;
                // parse all lines
                // - 1st non-comment line is headings line
                string rawline;
                while ((rawline = csvFile.ReadLine()) != null)
                {
                    string line = rawline.Trim();
                    if ((line.Length == 0) || (line[0] == '#'))
                    {
                        // skip comment lines
                        continue;
                    }
                    if (lineNum == 0)
                    {
                        // process heading (line 0)
                        List<string> headings = line.Split(',').ToList();
                        colSystem = GetColumn(headings, "System");
                        colTradeId = GetColumn(headings, "TradeId");
                        colMarketName = GetColumn(headings, "MarketName");
                        colMetricSourceNpv = GetColumn(headings, "SourceNPV");
                        colMetricOutputNpv = GetColumn(headings, "OutputNPV");
                        // optional stress scenarios and NPV variation
                        colIRScenario = GetColumn(headings, "IRScenario");
                        colFXScenario = GetColumn(headings, "FXScenario");
                        colStressNPVDelta = GetColumn(headings, "StressNPVDelta");
                    }
                    else
                    {
                        // process data
                        string key = "(undefined)";
                        try
                        {
                            string[] columns = line.Split(',');
                            // Generate the key
                            string tradeSource = columns[colSystem];
                            string tradeId = columns[colTradeId];
                            string marketName = columns[colMarketName];
                            key = $"{tradeSource}_{tradeId}_{marketName}_{InstrumentMetrics.NPV}".ToLower();
                            // Add the expected values
                            decimal sourceValue = Convert.ToDecimal(columns[colMetricSourceNpv]);
                            decimal outputValue = Convert.ToDecimal(columns[colMetricOutputNpv]);
                            expectedValues[key] = new ExpectedValue
                            {
                                SourceNPV = sourceValue,
                                OutputNPV = outputValue
                            };
                        }
                        catch (Exception fe)
                        {
                            // may not be a problem
                            UTE.Logger.LogWarning("Failed to load expected value '{0}' (line[{1} = '{2}']) {3}: {4}",
                                key, lineNum, line, fe.GetType().Name, fe.Message);
                        }
                    }
                    lineNum++;
                } // while
            }
            return expectedValues;
        }

        #endregion

        #region Test Initialization

        /// <summary>
        /// Create a portfolio with the matching trade
        /// </summary>
        /// <param name="tradeItemName"></param>
        private static string InitializePortfolio(string tradeItemName)
        {
            return InitializePortfolio(new[] { tradeItemName });
        }

        /// <summary>
        /// Create a portfolio with the matching trades
        /// </summary>
        /// <param name="tradeItemNames"></param>
        private static string InitializePortfolio(string[] tradeItemNames)
        {
            // publish portfolio specification
            string portfolioId = Guid.NewGuid().ToString();
            var portfolio = new PortfolioSpecification
            {
                PortfolioId = portfolioId,
                OwnerId = new UserIdentity
                {
                    Name = Engine.Cache.ClientInfo.Name,
                    DisplayName = "Unit Test Agent"
                },
                Description = "Unit Test",
                IncludedTradeItemNames = tradeItemNames
            };
            Engine.Cache.SaveObject(portfolio, Retention);
            return portfolioId;
        }

        #endregion

        #region Value and Test Helpers

        private static void AssertNotModified<T>(string itemName)
        {
            // checks named item for modification of the deserialised data
            ICoreItem item = Engine.Cache.LoadItem(typeof(T), itemName);
            string originalText = item.Text;
            string modifiedText = XmlSerializerHelper.SerializeToString(typeof(T), item.Data);
            if (string.CompareOrdinal(originalText, modifiedText) != 0)
            {
                using (var sw = new StreamWriter($"{itemName}.Original.xml")) { sw.Write(originalText); }
                using (var sw = new StreamWriter($"{itemName}.Modified.xml")) { sw.Write(modifiedText); }
                Assert.Fail("Item '{0}' modified!)", itemName);
            }
        }
        private static void AssertNotModified<T>(IEnumerable<string> itemNames)
        {
            foreach (string itemName in itemNames)
                AssertNotModified<T>(itemName);
        }

        /// <summary>
        /// Generate a valuation request for a market and portfolio
        /// Include any scenarios requested
        /// </summary>
        /// <param name="portfolioId"></param>
        /// <param name="marketName"></param>
        /// <param name="baseParty"></param>
        /// <param name="irScenarios">Optional array of IR Stress Scenario names</param>
        /// <param name="fxScenarios">optional array of FX Stress Scenario names</param>
        /// <returns></returns>
        private static PortfolioValuationRequest CreateRequest(string portfolioId, string marketName, string baseParty, string[] irScenarios = null, string[] fxScenarios = null)
        {
            // create portfolio valuation request
            Guid requestId = Guid.NewGuid();
            var request = new PortfolioValuationRequest
            {
                BaseDate = TestBaseDate,
                RequestId = requestId.ToString(),
                Retention = Retention.ToString(),
                SubmitTime = DateTimeOffset.Now.ToString("o"),
                RequestDescription = "PV Regression Test",
                RequesterId = new UserIdentity
                {
                    Name = Engine.Cache.ClientInfo.Name,
                    DisplayName = "Unit Test Agent"
                },
                PortfolioId = portfolioId,
                MarketDate = null,
                MarketName = marketName,
                ReportingCurrency = "AUD",
                BaseParty = baseParty,
                IRScenarioNames = irScenarios,
                FXScenarioNames = fxScenarios
            };
            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        private static HandlerResponse ExecuteTradeValuation(PortfolioValuationRequest request)
        {
            // create and execute workflow
            HandlerResponse result;
            IWorkContext context = new WorkContext(UTE.Logger, Engine.Cache, "UnitTest");
            using (var workflow = new WFCalculatePortfolioStressValuation())
            {
                workflow.Initialise(context);
                WorkflowOutput<HandlerResponse> output = workflow.Execute(request);
                result = output.Result;
                WorkflowHelper.ThrowErrors(output.Errors);
            }
            return result;
        }

        private static IEnumerable<string> GenerateAllFxCurveNames()
        {
            var currencies = new[] { "AUD", "USD", "NZD", "GBP", "HKD", "EUR", "JPY" };
            var fxCurveNames = new List<string>();
            foreach (string currency1 in currencies)
            {
                fxCurveNames.AddRange(from currency2 in currencies where currency1 != currency2 select
                    $"{currency1}-{currency2}");
            }
            return fxCurveNames;
        }

        private static IEnumerable<string> GenerateAllRateCurveNames()
        {
            var currencies = new[] { "AUD", "USD", "NZD", "GBP", "HKD", "EUR", "JPY" };
            var indexName1S = new[] { "LIBOR", "BBR" };
            var indexName2S = new[] { "BBA", "BBSW", "FRA" , "SENIOR"};
            var indexTenors = new[] { null, "1M", "2M", "3M", "6M" };
            return (from currency in currencies
                    from indexName1 in indexName1S
                    from indexName2 in indexName2S
                    from indexTenor in indexTenors
                    select indexTenor != null ? $"{currency}-{indexName1}-{indexName2}-{indexTenor}" : $"{currency}-{indexName1}-{indexName2}"
                ).ToList();
        }

        public static void StressAllFxCurves()
        {
            var marketNames = new[] { CurveConst.TEST_EOD, CurveConst.QR_LIVE };
            var curveTypes = new[] { "FxCurve" };
            ExecuteCurveStressing(marketNames, curveTypes, GenerateAllFxCurveNames());
        }

        public static void StressAllRateCurves()
        {
            var marketNames = new[] { CurveConst.TEST_EOD, CurveConst.QR_LIVE };
            var curveTypes = new[] { "RateCurve" };
            ExecuteCurveStressing(marketNames, curveTypes, GenerateAllRateCurveNames());
        }

        private static HandlerResponse ExecuteCurveStressing(string marketName, string curveType, string curveName)
        {
            return ExecuteCurveStressing(new[] { marketName }, new[] { curveType }, new[] { curveName });
        }

        private static HandlerResponse ExecuteCurveStressing(IEnumerable<string> marketNames, IEnumerable<string> curveTypes, IEnumerable<string> curveNames)
        {
            // create list of base curve ids to stress
            var curveSelectors = new List<CurveSelection>();
            // hack make sure QR_LIVE AUD reference curve is stressed first
            {
                curveSelectors.Add(new CurveSelection
                    {
                    MarketName = CurveConst.QR_LIVE,
                    CurveType = "RateCurve",
                    CurveName = "AUD-BBR-BBSW-3M"
                });
            }
            curveSelectors.AddRange(from marketName in marketNames
                                    from curveType in curveTypes
                                    from curveName in curveNames
                                    let baseCurveUniqueId = string.Format(UTE.NameSpace + ".Market.{0}.{1}.{2}", marketName, curveType, curveName)
                                    let curveItem = Engine.Cache.LoadItem<Market>(baseCurveUniqueId)
                                    where curveItem != null
                                    select new CurveSelection
                                        {
                                            MarketName = marketName, CurveType = curveType, CurveName = curveName
                                        });
            // create and execute workflow
            var request = new StressedCurveGenRequest
                {
                RequestId = Guid.NewGuid().ToString(),
                RequesterId = new UserIdentity
                    {
                    Name = Engine.Cache.ClientInfo.Name,
                    DisplayName = "Unit Test Agent"
                },
                BaseDate = TestBaseDate,
                CurveSelector = curveSelectors.ToArray()
                
            };
            // build list of curve unique ids required
            var baseCurveUniqueIds = curveSelectors.Select(curveSelector => string.Format(UTE.NameSpace + ".Market.{0}.{1}.{2}", curveSelector.MarketName, curveSelector.CurveType, curveSelector.CurveName)).ToList();
            AssertNotModified<Market>(baseCurveUniqueIds);
            HandlerResponse result;
            IWorkContext context = new WorkContext(UTE.Logger, Engine.Cache, "UnitTest");
            using (var workflow = new WFGenerateStressedCurve())
            {
                workflow.Initialise(context);
                WorkflowOutput<HandlerResponse> output = workflow.Execute(request);
                WorkflowHelper.ThrowErrors(output.Errors);
                result = output.Result;
            }
            AssertNotModified<Market>(baseCurveUniqueIds);
            return result;
        }

        private static void AssertValuation(HandlerResponse result)
        {
            // Load valuationItems for the current request Id
            List<ICoreItem> rawValuationItems =
                Engine.Cache.LoadItems<ValuationReport>(Expr.BoolAND(Expr.IsNull(ValueProp.Aggregation),
                                                               Expr.IsEQU(RequestBase.Prop.RequestId, Guid.Parse(result.RequestId))));
            foreach (ICoreItem valuationItem in rawValuationItems)
            {
                // Extract the valuation report
                var valuation = (ValuationReport)valuationItem.Data;
                // Check that the valuation returned successfully
                var excpName = valuationItem.AppProps.GetValue<string>(WFPropName.ExcpName, null);
                if (excpName != null)
                {
                    // valuation failed - log details
                    var excpText = valuationItem.AppProps.GetValue<string>(WFPropName.ExcpText, null);
                    UTE.Logger.LogError(
                        "Valuation failed:" +
                        "{0}    UniqueId : {1}" +
                        "{0}    Exception: {2}: '{3}'",
                        Environment.NewLine,
                        valuationItem.Name, excpName, excpText);
                    Assert.Fail(excpName, $"Workflow error ({excpName})");
                }
                // check for errors
                var tradeSource = valuationItem.AppProps.GetValue<string>(TradeProp.TradeSource);
                var tradeId = valuationItem.AppProps.GetValue<string>(TradeProp.TradeId);
                var marketName = valuationItem.AppProps.GetValue<string>(CurveProp.Market);
                var key = $"{tradeSource}_{tradeId}_{marketName}_{InstrumentMetrics.NPV}".ToLower();
                // Assert expected value has been supplied
                Assert.IsTrue(ExpectedValues.ContainsKey(key), $"ExpectedValue for key:({key}) is missing.");
                ExpectedValue expectedValue = ExpectedValues[key];
                Quotation quote = valuation.GetFirstQuotationForMetricName(InstrumentMetrics.NPV.ToString());
                Assert.IsNotNull(quote, $"CalculatedValue for key:({key}) is missing.");
                Assert.IsTrue(quote.valueSpecified, $"CalculatedValue for key:({key}) is missing.");
                decimal calculatedNPV = quote.value;
                decimal differenceNPV = expectedValue.OutputNPV - calculatedNPV;
                Assert.IsFalse(System.Math.Abs(differenceNPV) > ToleranceNPV,
                    $"ToleranceExceeded(|{differenceNPV:F0}| > {ToleranceNPV:F0})");
            }
        }


        private static void PrintValuation(HandlerResponse result)
        {
            // Load valuationItems for the current request Id
            List<ICoreItem> rawValuationItems =
                Engine.Cache.LoadItems<ValuationReport>(Expr.BoolAND(Expr.IsNull(ValueProp.Aggregation),
                                                               Expr.IsEQU(RequestBase.Prop.RequestId, Guid.Parse(result.RequestId))));
            foreach (ICoreItem valuationItem in rawValuationItems)
            {
                Debug.Print(XmlSerializerHelper.SerializeToString(valuationItem.AppProps.Serialise()));
                // Extract the valuation report
                var valuation = (ValuationReport)valuationItem.Data;
                XmlSerializerHelper.SerializeToFile(valuation, valuationItem.Name);
                //Debug.Print(valuationItem.Text);
            }
        }


        #endregion

        /// <summary>
        /// Value and Test the portfolio
        /// </summary>
        /// <param name="portfolioId"></param>
        /// <param name="irScenarios">Optional IR Scenarios</param>
        /// <param name="fxScenarios">Optional FX Scenarios</param>
        private static void ValueAndTestPortfolio(string portfolioId, string[] irScenarios = null, string[] fxScenarios = null)
        {
            foreach (var testMarketName in TestMarketNames)
            {
                PortfolioValuationRequest request = CreateRequest(portfolioId, testMarketName, null, irScenarios, fxScenarios);
                HandlerResponse result = ExecuteTradeValuation(request);
                AssertValuation(result);
            }
        }

        /// <summary>
        /// Value and Test the portfolio
        /// </summary>
        /// <param name="portfolioId"></param>
        private static void ValueAndTestPortfolioDebug(string portfolioId)
        {
            var irScenarios = new[] {ScenarioConst.GlobalIRDn100bp};
            //var curveTypes = new[] { "RateCurve" };
            //ExecuteCurveStressing(TestMarketNames, curveTypes, Generate_All_RateCurveNames());
            foreach (var testMarketName in TestMarketNames)
            {
                PortfolioValuationRequest request = CreateRequest(portfolioId, testMarketName, null, irScenarios);
                HandlerResponse result = ExecuteTradeValuation(request);
                PrintValuation(result);
            }
        }

        /// <summary>
        /// Value and Test the portfolio
        /// </summary>
        /// <param name="portfolioId"></param>
        private static void ValueTradeDebug(string portfolioId)
        {
            var irScenarios = new[] { ScenarioConst.Unstressed };
            //var curveTypes = new[] { "RateCurve" };
            //ExecuteCurveStressing(TestMarketNames, curveTypes, Generate_All_RateCurveNames());
            foreach (var testMarketName in TestMarketNames)
            {
                PortfolioValuationRequest request = CreateRequest(portfolioId, testMarketName, null, irScenarios);
                HandlerResponse result = ExecuteTradeValuation(request);
                PrintValuation(result);
            }
        }

        #endregion

        #region Tests

        #region Stress curve tests

        [TestMethod]
        public void StressFxCurveAllCombinations()
        {
            var marketNames = new[] { CurveConst.TEST_EOD, CurveConst.QR_LIVE };
            var curveTypes = new[] { "FxCurve" };
            ExecuteCurveStressing(marketNames, curveTypes, GenerateAllFxCurveNames());
        }

        [TestMethod]
        public void StressRateCurveAllCombinations()
        {
            var marketNames = new[] { CurveConst.TEST_EOD, CurveConst.QR_LIVE };
            var curveTypes = new[] { "RateCurve" };
            ExecuteCurveStressing(marketNames, curveTypes, GenerateAllRateCurveNames());
        }

        #region Fx ordinary curves

        [TestMethod]
        public void StressFxCurveTestEODAUDUSD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "AUD-USD"); }

        [TestMethod]
        public void StressFxCurveTestEODEurUSD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "EUR-USD"); }

        [TestMethod]
        public void StressFxCurveTestEODGBPUSD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "GBP-USD"); }

        [TestMethod]
        public void StressFxCurveTestEODHkdUSD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "HKD-USD"); }

        [TestMethod]
        public void StressFxCurveTestEODJPYUSD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "JPY-USD"); }

        [TestMethod]
        public void StressFxCurveTestEODNZDUSD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "NZD-USD"); }

        #endregion

        #region Fx inverse curves

        [TestMethod]
        public void StressFxCurveTestEODUSDAUD() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "USD-AUD"); }

        [TestMethod]
        public void StressFxCurveTestEODUSDHkd() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "USD-HKD"); }

        [TestMethod]
        public void StressFxCurveTestEODUSDJPY() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "USD-JPY"); }

        #endregion

        #region Fx triangulated curves

        [TestMethod]
        public void StressFxCurveTestEODAUDHkd() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "AUD-HKD"); }

        [TestMethod]
        public void StressFxCurveTestEODAUDGBP() { ExecuteCurveStressing(CurveConst.TEST_EOD, "FxCurve", "AUD-GBP"); }

        #endregion

        [TestMethod]
        public void StressRateCurveTestEODAUDBbrBbsw1M()
        {
            ExecuteCurveStressing(CurveConst.TEST_EOD, "RateCurve", "AUD-BBR-BBSW-1M");
        }

        [TestMethod]
        public void StressRateCurveTestEODAUDBbrBbsw6M()
        {
            ExecuteCurveStressing(CurveConst.TEST_EOD, "RateCurve", "AUD-BBR-BBSW-6M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw1M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-1M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw2M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-2M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw3M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-3M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw6M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-6M");
        }

        #endregion

        #region FpML Example Tests

        #region XCCY Tests

        [TestMethod]
        public void ValueFpMLXccySwapTw9235Debug()
        {
            const string tradeName = "Highlander.V5r3.Trade.Reporting.Murex.swap.TW9235";
            string portfolioId = InitializePortfolio(tradeName);
            //string[] curveTypes = new string[] { "RateCurve" };
            //ExecuteCurveStressing(TestMarketNames, curveTypes, Generate_All_RateCurveNames());
            ValueTradeDebug(portfolioId);
            AssertNotModified<Trade>(tradeName);
        }

        #endregion

        #endregion

        #endregion
    }
}
