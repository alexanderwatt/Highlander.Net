using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Common;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.Contracts;
using Orion.Util.Expressions;
using Orion.Util.Serialisation;
using Orion.V5r3.Configuration;
using PortfolioValuer.Regression.Data;
using Orion.Workflow;
using Orion.Workflow.CurveGeneration;
using Orion.PortfolioValuation;
using Orion.UnitTestEnv;
using Exception = System.Exception;

namespace PVRegression.Tests 
{
    [TestClass]
    public class NewRegressionTests
    {
        #region Constants, Fields

        private static readonly DateTime TestBaseDate = new DateTime(2010, 8, 30);
        private static readonly string[] TestMarketNames = new[] { CurveConst.TEST_EOD };

        const decimal ToleranceNPV = 1000.0M;

        #endregion

        #region Properties

        private static TimeSpan Retention { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        private static Orion.CurveEngine.CurveEngine Engine { get; set; }
        private static IDictionary<string, ExpectedValue> ExpectedValues { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            Engine = new Orion.CurveEngine.CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            // load regresssion test data and expected values
            LoadConfigDataHelper.LoadConfigurationData(UTE.Logger, UTE.Cache, UTE.NameSpace);
            //RegressionTestDataLoader.Load(UTE.Logger, UTE.Cache);
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
            /// Current value produced by the QRSC anlytics (allowing for known/acceptable issues).
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
                            key = String.Format("{0}_{1}_{2}_{3}", tradeSource, tradeId, marketName, InstrumentMetrics.NPV).ToLower();

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
            if (String.CompareOrdinal(originalText, modifiedText) != 0)
            {
                using (var sw = new StreamWriter(String.Format("{0}.Original.xml", itemName))) { sw.Write(originalText); }
                using (var sw = new StreamWriter(String.Format("{0}.Modified.xml", itemName))) { sw.Write(modifiedText); }
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
                fxCurveNames.AddRange(from currency2 in currencies where currency1 != currency2 select String.Format("{0}-{1}", currency1, currency2));
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
                    select indexTenor != null ? String.Format("{0}-{1}-{2}-{3}", currency, indexName1, indexName2, indexTenor) : String.Format("{0}-{1}-{2}", currency, indexName1, indexName2)).ToList();
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
                                    let baseCurveUniqueId = String.Format(UTE.NameSpace + ".Market.{0}.{1}.{2}", marketName, curveType, curveName)
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
            var baseCurveUniqueIds = curveSelectors.Select(curveSelector => String.Format(UTE.NameSpace + ".Market.{0}.{1}.{2}", curveSelector.MarketName, curveSelector.CurveType, curveSelector.CurveName)).ToList();
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
                    Assert.Fail(excpName, string.Format("Workflow error ({0})", excpName));
                }

                // check for errors
                var tradeSource = valuationItem.AppProps.GetValue<string>(TradeProp.TradeSource);
                var tradeId = valuationItem.AppProps.GetValue<string>(TradeProp.TradeId);
                var marketName = valuationItem.AppProps.GetValue<string>(CurveProp.Market);
                var key = String.Format("{0}_{1}_{2}_{3}", tradeSource, tradeId, marketName, InstrumentMetrics.NPV).ToLower();

                // Assert expected value has been supplied
                Assert.IsTrue(ExpectedValues.ContainsKey(key), string.Format("ExpectedValue for key:({0}) is missing.", key));

                ExpectedValue expectedValue = ExpectedValues[key];

                Quotation quote = valuation.GetFirstQuotationForMetricName(InstrumentMetrics.NPV.ToString());
                Assert.IsNotNull(quote, string.Format("CalculatedValue for key:({0}) is missing.", key));
                Assert.IsTrue(quote.valueSpecified, string.Format("CalculatedValue for key:({0}) is missing.", key));

                decimal calculatedNPV = quote.value;
                decimal differenceNPV = expectedValue.OutputNPV - calculatedNPV;

                Assert.IsFalse(System.Math.Abs(differenceNPV) > ToleranceNPV,
                               String.Format("ToleranceExceeded(|{0}| > {1})", differenceNPV.ToString("F0"), ToleranceNPV.ToString("F0")));
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
        public void StressRateCurveTestEODAUDBbrBbsw_1M()
        {
            ExecuteCurveStressing(CurveConst.TEST_EOD, "RateCurve", "AUD-BBR-BBSW-1M");
        }

        [TestMethod]
        public void StressRateCurveTestEODAUDBbrBbsw_6M()
        {
            ExecuteCurveStressing(CurveConst.TEST_EOD, "RateCurve", "AUD-BBR-BBSW-6M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw_1M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-1M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw_2M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-2M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw_3M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-3M");
        }

        [TestMethod]
        public void StressRateCurveQRLiveAUDBbrBbsw_6M()
        {
            ExecuteCurveStressing(CurveConst.QR_LIVE, "RateCurve", "AUD-BBR-BBSW-6M");
        }

        #endregion

        #region FpML Example Tests

        #region XCCY Tests

        [TestMethod]
        public void ValueFpMLXccySwapTw9235Debug()
        {
            const string tradeName = "Orion.V5r3.Trade.Reporting.Murex.swap.TW9235";
            string portfolioId = InitializePortfolio(tradeName);
            //string[] curveTypes = new string[] { "RateCurve" };
            //ExecuteCurveStressing(TestMarketNames, curveTypes, Generate_All_RateCurveNames());
            ValueTradeDebug(portfolioId);
            AssertNotModified<Trade>(tradeName);
        }

        #endregion

        #endregion

        #region Calypso Tests

        //#region XCCY Tests

        //[TestMethod]
        //public void ValueCalypsoXccySwap167582Debug()
        //{
        //    const string tradeName = "Calypso.Trade.CrossCurrencySwap.167582";
        //    string portfolioId = InitializePortfolio(tradeName);
        //    var curveTypes = new[] { "RateCurve" };
        //    ExecuteCurveStressing(TestMarketNames, curveTypes, GenerateAllRateCurveNames());
        //    ValueAndTestPortfolioDebug(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoXccySwap167582()
        //{
        //    const string tradeName = "Calypso.Trade.CrossCurrencySwap.167582";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoXccySwap173245()
        //{
        //    const string tradeName = "Calypso.Trade.CrossCurrencySwap.173245";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoXccySwapPortfolio()
        //{
        //    var tradeNames = new[] { "Calypso.Trade.CrossCurrencySwap.167582", "Calypso.Trade.CrossCurrencySwap.173245" };
        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //#endregion

        //#region Interest Rate Swap Tests

        //[TestMethod]
        //public void ValueCalypsoIrSwap101617()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.101617";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap102203()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.102203";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap105220()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.105220";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap148515()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.148515";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap150441()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.150441";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap175629()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.175629";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[Ignore] // trade does not exist as at 30-Aug-2010
        //[TestMethod]
        //public void ValueCalypsoIrSwap185576()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.185576";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap30233Detail()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.30233";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap30233()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.30233";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap30233Multiple()
        //{
        //    var tradeNames = new[]
        //                      {"Calypso.Trade.InterestRateSwap.30233", "Calypso.Trade.InterestRateSwap.30233",
        //                      "Calypso.Trade.InterestRateSwap.30233", "Calypso.Trade.InterestRateSwap.30233"};
        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwap43106()
        //{
        //    const string tradeName = "Calypso.Trade.InterestRateSwap.43106";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwapPortfolio()
        //{
        //    var tradeNames = new[]
        //                      {
        //                          "Calypso.Trade.InterestRateSwap.101617", "Calypso.Trade.InterestRateSwap.102203",
        //                          "Calypso.Trade.InterestRateSwap.105220", "Calypso.Trade.InterestRateSwap.148515",
        //                          "Calypso.Trade.InterestRateSwap.150441", "Calypso.Trade.InterestRateSwap.175629",
        //                          "Calypso.Trade.InterestRateSwap.30233", "Calypso.Trade.InterestRateSwap.43106"
        //                      };
        //    // "Calypso.Trade.InterestRateSwap.185576"

        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //[TestMethod]
        //public void ValueCalypsoIrSwapPortfolioDetail()
        //{
        //    var tradeNames = new[]
        //                      {
        //                          "Calypso.Trade.InterestRateSwap.101617", "Calypso.Trade.InterestRateSwap.102203",
        //                          "Calypso.Trade.InterestRateSwap.105220", "Calypso.Trade.InterestRateSwap.148515",
        //                          "Calypso.Trade.InterestRateSwap.150441", "Calypso.Trade.InterestRateSwap.175629",
        //                          "Calypso.Trade.InterestRateSwap.30233", "Calypso.Trade.InterestRateSwap.43106"
        //                      };
        //    // "Calypso.Trade.InterestRateSwap.185576"

        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //#endregion

        //#region Forward Rate Agreement Tests

        //[TestMethod]
        //public void ValueCalypsoFRA100597()
        //{
        //    const string tradeName = "Calypso.Trade.FRA.100597";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoFRA177912()
        //{
        //    const string tradeName = "Calypso.Trade.FRA.177912";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void ValueCalypsoFRATradesRepeat()
        //{
        //    var tradeNames = new[] { "Calypso.Trade.FRA.100597", "Calypso.Trade.FRA.100597" };
        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //[TestMethod]
        //public void ValueCalypsoFRATradesRepeat2()
        //{
        //    var tradeNames = new[] { "Calypso.Trade.FRA.177912", "Calypso.Trade.FRA.177912" };
        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //[TestMethod]
        //public void ValueCalypsoFRATrades()
        //{
        //    var tradeNames = new[] { "Calypso.Trade.FRA.100597", "Calypso.Trade.FRA.177912" };
        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //#endregion

        #endregion

        #region Murex Tests

        //#region FX Spot Tests

        //[TestMethod]
        //public void Value_Murex_FxSpot_184469()
        //{
        //    const string tradeName = "Murex.Trade.FxSpot.184469";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void Value_Murex_FxSpot_184682()
        //{
        //    const string tradeName = "Murex.Trade.FxSpot.184682";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void Value_Murex_FxSpot_184696()
        //{
        //    const string tradeName = "Murex.Trade.FxSpot.184696";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //#endregion

        //#region FX Forward Tests

        //[TestMethod]
        //public void Value_Murex_FxForward_13111()
        //{
        //    const string tradeName = "Murex.Trade.FxForward.13111";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void Value_Murex_FxForward_171834()
        //{
        //    const string tradeName = "Murex.Trade.FxForward.171834";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void Value_Murex_FxForward_179572()
        //{
        //    const string tradeName = "Murex.Trade.FxForward.179572";
        //    string portfolioId = InitializePortfolio(tradeName);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeName);
        //}

        //[TestMethod]
        //public void Value_Murex_FxForward_Portfolio()
        //{
        //    string[] tradeNames = new[] { "Murex.Trade.FxForward.13111", "Murex.Trade.FxForward.171834", "Murex.Trade.FxForward.179572" };
        //    string portfolioId = InitializePortfolio(tradeNames);

        //    ValueAndTestPortfolio(portfolioId);
        //    AssertNotModified<Trade>(tradeNames);
        //}

        //#endregion

        #endregion

        #endregion
    }
}
