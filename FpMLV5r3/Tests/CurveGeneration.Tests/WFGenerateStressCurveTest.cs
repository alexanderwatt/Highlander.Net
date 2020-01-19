using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;
using Highlander.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Contracts;
using Orion.V5r3.CurveGeneration.Tests.Properties;
using Orion.Workflow.CurveGeneration;
using Orion.UnitTestEnv;
//using Orion.CurveEngine.Tests.Helpers;

namespace Orion.V5r3.CurveGeneration.Tests
{
    /// <summary>
    ///This is a test class for WFGenerateStressCurveTest and is intended
    ///to contain all WFGenerateStressCurveTest Unit Tests
    ///</summary>
    [TestClass]
    public class WFGenerateStressCurveTest
    {
        #region Setup

        private static ILogger Logger_obs { get; set; }
        private static UnitTestEnvironment UTE { get; set; }
        private static CurveEngine.CurveEngine Engine { get; set; }
        private static string NameSpace { get; set; }

        private const string Aud3M = "Market.GlobalIB_EOD.RateCurve.AUD-BBR-BBSW-3M";
        private const string Aud1M = "Market.GlobalIB_EOD.RateCurve.AUD-ZERO-BANK-1M";
        private const string AudUsd = "Market.GlobalIB_EOD.RateCurve.AUD-USD-BASIS";

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            NameSpace = UTE.NameSpace;
            //Set the calendar engine
            Engine = new CurveEngine.CurveEngine(UTE.Logger, UTE.Cache, NameSpace);
            SaveCurve(Aud3M, new NamedValueSet(Resources.AUD_BBR_BBSW_3M_AppProps), Resources.AUD_BBR_BBSW_3M_Data);
            SaveCurveBasis(Aud1M, new NamedValueSet(Resources.AUD_BBR_BBSW_3M_AppProps), Resources.AUD_BBR_BBSW_3M_Data,
                new NamedValueSet(Resources.AUD_BBR_BBSW_1M_AppProps), Resources.AUD_BBR_BBSW_1M_Data);
            SaveCurve(AudUsd, new NamedValueSet(Resources.AUD_USD_BASIS_AppProps), Resources.AUD_USD_BASIS_Data);
        }

        private static void SaveCurve(string curveId, NamedValueSet properties, string data)
        {
            var baseMarket = XmlSerializerHelper.DeserializeFromString<Market>(data);
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            // create the curve in Engine, so that same software is used to create it
            var fpml = new Pair<PricingStructure, PricingStructureValuation>(baseMarket.Items[0], baseMarket.Items1[0]);
            var curve = Engine.CreateCurve(fpml, properties, null, null);
            baseMarket = curve.GetMarket();
            Engine.SaveCurve(baseMarket, curveId, properties, new TimeSpan(1, 0, 0));
        }

        private static void SaveCurveBasis(string curveId, NamedValueSet refProperties, string refData, 
                                            NamedValueSet spreadProperties, string spreadData)
        {
            var spreadMarket = XmlSerializerHelper.DeserializeFromString<Market>(spreadData);
            var refMarket = XmlSerializerHelper.DeserializeFromString<Market>(refData);
            spreadProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            // create the curve in Engine, so that same software is used to create it
            var spreadCurve
                = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(spreadMarket.Items[0],
                                                                                          spreadMarket.Items1[0],
                                                                                          spreadProperties);

            var refCurve
                = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(refMarket.Items[0],
                                                                                          refMarket.Items1[0],
                                                                                          refProperties);

            var curve = Engine.CreateCurve(refCurve, spreadCurve);
            var market = curve.GetMarket();

            Engine.SaveCurve(market, curveId, spreadProperties, new TimeSpan(1, 0, 0));
        }

        #endregion

        /// <summary>
        ///A test for OnExecute
        ///</summary>
        [TestMethod]
        public void OnExecute3MTest()
        {
            #region Load the curve

            ICoreItem item = Engine.Cache.LoadItem<Market>(NameSpace + "." + Aud3M);
            var baseMarket = (Market)item.Data;
            NamedValueSet properties = item.AppProps;

            #endregion

            #region Do it
            
            // Execute
            WorkflowOutput<HandlerResponse> actual = RunStressGenerator(properties);

            // Check for thrown errors
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Errors.Length);
            //Assert.AreEqual(0, CurveEngine._Logger.Logs[LogSeverity.Error].Count);

            #endregion

            #region Load the stressed curves back out
            // Check the stressed curves are as expected
            IEnumerable<ICoreItem> stressedItems = GetStressedItems("3M");

            // ZeroStress
            var zeroStressItem = stressedItems.Where(a => a.AppProps.GetString("StressName", true) == "ZeroStress").Single();
            var zeroStressMarket = (Market)zeroStressItem.Data;
            // PercentUp1
            var pup1StressItem = stressedItems.Where(a => a.AppProps.GetString("StressName", true) == "PercentUp1").Single();
            var pup1StressMarket = (Market)pup1StressItem.Data;
            #endregion

            #region Check the inputs
            IEnumerable<decimal> baseInputs = ((YieldCurveValuation)baseMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);

            // Check the zeroStress curve
            IEnumerable<decimal> zeroStressedInputs = ((YieldCurveValuation)zeroStressMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);
            CollectionAssert.AreEqual(baseInputs.ToList(), zeroStressedInputs.ToList());

            // Check the PercentUp1 curve
            IEnumerable<decimal> pup1StressedInputs = ((YieldCurveValuation)pup1StressMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);
            CollectionAssert.AreNotEqual(baseInputs.ToList(), pup1StressedInputs.ToList());
            #endregion

            #region Check the zero rates
            IEnumerable<decimal> baseZeroRates = ((YieldCurveValuation)baseMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);

            // Check the zeroStress curve
            IEnumerable<decimal> zeroStressedZeroRates = ((YieldCurveValuation)zeroStressMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);
            CollectionAssert.AreEqual(baseZeroRates.ToList(), zeroStressedZeroRates.ToList());

            // Check the PercentUp1 curve
            IEnumerable<decimal> pup1StressedZeroRates = ((YieldCurveValuation)pup1StressMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);
            CollectionAssert.AreNotEqual(baseZeroRates.ToList(), pup1StressedZeroRates.ToList());
            #endregion
        }

        private static IEnumerable<ICoreItem> GetStressedItems(string tenor)
        {
            IExpression expr1 = Expr.IsEQU(Expr.Prop(CurveProp.MarketAndDate), Expr.Const("GlobalIB_EOD"));
            IExpression expr2 = string.IsNullOrEmpty(tenor) 
                                    ? Expr.IsEQU(Expr.Prop(CurveProp.PricingStructureType), Expr.Const("XccySpreadCurve")) 
                                    : Expr.IsEQU(Expr.Prop(CurveProp.IndexTenor), Expr.Const(tenor));
            IExpression expr3 = Expr.IsNotNull(Expr.Prop(CurveProp.StressName));
            IExpression expr = Expr.BoolAND(expr1, expr2, expr3);
            List<ICoreItem> stressedItems = Engine.Cache.LoadItems<Market>(expr);
            Assert.AreEqual(13, stressedItems.Count);
            return stressedItems;
        }

        /// <summary>
        ///A test for OnExecute
        ///</summary>
        [TestMethod]
        public void OnExecute1MTest()
        {
            #region Load the curve

            ICoreItem item = Engine.Cache.LoadItem<Market>(NameSpace + "." + Aud1M);
            var baseMarket = (Market)item.Data;
            NamedValueSet properties = item.AppProps;

            #endregion

            #region Do it

            // Execute
            WorkflowOutput<HandlerResponse> actual = RunStressGenerator(properties);

            // Check for thrown errors
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Errors.Length);
            //Assert.AreEqual(0, CurveEngine.CacheLogger.Logs[LogSeverity.Error].Count);

            #endregion

            #region Load the stressed curves back out
            IEnumerable<ICoreItem> stressedItems = GetStressedItems("1M");

            // ZeroStress
            var zeroStressItem = stressedItems.Where(a => a.AppProps.GetString("StressName", true) == "ZeroStress").Single();
            var zeroStressMarket = (Market)zeroStressItem.Data;
            // PercentUp1
            var pup1StressItem = stressedItems.Where(a => a.AppProps.GetString("StressName", true) == "PercentUp1").Single();
            var pup1StressMarket = (Market)pup1StressItem.Data;
            #endregion

            #region Check the inputs
            IEnumerable<decimal> baseInputs = ((YieldCurveValuation)baseMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);

            // Check the zeroStress curve
            IEnumerable<decimal> zeroStressedInputs = ((YieldCurveValuation)zeroStressMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);
            CollectionAssert.AreEqual(baseInputs.ToList(), zeroStressedInputs.ToList());

            // Check the PercentUp1 curve - don't expect the spreads to change, it is the reference curve that changes
            IEnumerable<decimal> pup1StressedInputs = ((YieldCurveValuation)pup1StressMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);
            CollectionAssert.AreEqual(baseInputs.ToList(), pup1StressedInputs.ToList());
            #endregion

            #region Check the zero rates
            IEnumerable<decimal> baseZeroRates = ((YieldCurveValuation)baseMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);

            // Check the zeroStress curve
            IEnumerable<decimal> zeroStressedZeroRates = ((YieldCurveValuation)zeroStressMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);
            CollectionAssert.AreEqual(baseZeroRates.ToList(), zeroStressedZeroRates.ToList());

            // Check the PercentUp1 curve
            IEnumerable<decimal> pup1StressedZeroRates = ((YieldCurveValuation)pup1StressMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);
            CollectionAssert.AreNotEqual(baseZeroRates.ToList(), pup1StressedZeroRates.ToList());
            #endregion
        }

        /// <summary>
        ///A test for OnExecute
        ///</summary>
        [TestMethod]
        public void OnExecuteXccyTest()
        {
            #region Load the curve

            ICoreItem item = Engine.Cache.LoadItem<Market>(NameSpace + "." + AudUsd);
            var baseMarket = (Market)item.Data;
            NamedValueSet properties = item.AppProps;

            #endregion

            #region Do it

            // Execute
            WorkflowOutput<HandlerResponse> actual = RunStressGenerator(properties);

            // Check for thrown errors
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Errors.Length);
            //Assert.AreEqual(0, CurveEngine._Logger.Logs[LogSeverity.Error].Count);

            #endregion

            #region Load the stressed curves back out
            IEnumerable<ICoreItem> stressedItems = GetStressedItems(null);

            // ZeroStress
            var zeroStressItem = stressedItems.Where(a => a.AppProps.GetString("StressName", true) == "ZeroStress").Single();
            var zeroStressMarket = (Market)zeroStressItem.Data;
            // PercentUp1
            var pup1StressItem = stressedItems.Where(a => a.AppProps.GetString("StressName", true) == "PercentUp1").Single();
            var pup1StressMarket = (Market)pup1StressItem.Data;
            #endregion

            #region Check the inputs
            IEnumerable<decimal> baseInputs = ((YieldCurveValuation)baseMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);

            // Check the zeroStress curve
            IEnumerable<decimal> zeroStressedInputs = ((YieldCurveValuation)zeroStressMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);
            CollectionAssert.AreEqual(baseInputs.ToList(), zeroStressedInputs.ToList());

            // Check the PercentUp1 curve
            IEnumerable<decimal> pup1StressedInputs = ((YieldCurveValuation)pup1StressMarket.Items1[0]).inputs.assetQuote.Select(a => a.quote[0].value);
            CollectionAssert.AreNotEqual(baseInputs.ToList(), pup1StressedInputs.ToList());
            #endregion

            #region Check the zero rates
            IEnumerable<decimal> baseZeroRates = ((YieldCurveValuation)baseMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);

            // Check the zeroStress curve
            IEnumerable<decimal> zeroStressedZeroRates = ((YieldCurveValuation)zeroStressMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);
            CollectionAssert.AreEqual(baseZeroRates.ToList(), zeroStressedZeroRates.ToList());

            // Check the PercentUp1 curve
            IEnumerable<decimal> pup1StressedZeroRates = ((YieldCurveValuation)pup1StressMarket.Items1[0]).zeroCurve.rateCurve.point.Select(a => a.mid);
            CollectionAssert.AreNotEqual(baseZeroRates.ToList(), pup1StressedZeroRates.ToList());
            #endregion
        }

        private static WorkflowOutput<HandlerResponse> RunStressGenerator(NamedValueSet properties)
        {
            var wfGenerateStressCurve = new WFGenerateStressedCurve();
            IWorkContext context = new WorkContext(Engine.Logger, Engine.Cache, "UnitTest");
            wfGenerateStressCurve.Initialise(context);
            var request = new StressedCurveGenRequest
                {
                RequestId = Guid.NewGuid().ToString(),
                RequesterId = new UserIdentity
                    {
                    Name = Engine.Cache.ClientInfo.Name,
                    DisplayName = "Unit Test Agent"
                },
                BaseDate = properties.GetValue<DateTime>("BaseDate", true),
                CurveSelector = new[] { new CurveSelection
                    {
                    NameSpace = properties.GetString("NameSpace", true),
                    CurveName = properties.GetString("CurveName", true),
                    CurveType = properties.GetString("PricingStructureType", true),
                    MarketName = properties.GetString("MarketName", true)
                }}
            };
            return wfGenerateStressCurve.Execute(request);
        }
    }
}
