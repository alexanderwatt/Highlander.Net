#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FpML.V5r3.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
//using Orion.CurveEngine.Tests.Helpers;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Valuations;
using Orion.CurveEngine.Markets;
//using Orion.ModelFramework;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
//using Orion.CurveEngine.PricingStructures.Curves;
using Valuation = Orion.ValuationEngine.Valuations.Valuation;
//using Orion.ValuationEngine.Tests;
//using Orion.CurveEngine.Tests;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    public partial class ExcelAPITests
    {
        // GetInfo tests todo:
        //  swap,
        //  swaption
        //  swap + cap
        //  swap + floor 
        //  swap + cap + floor
        //
        public IFxCurve TestFxCurve(DateTime baseDate)
        {
            const string curveName = "AUD-USD";
            const string algorithm = "LinearForward";
            //const double tolerance = 0.00000001;
            var fxProperties = new NamedValueSet();
            fxProperties.Set(CurveProp.PricingStructureType, "FxCurve");
            fxProperties.Set("BuildDateTime", baseDate);
            fxProperties.Set(CurveProp.BaseDate, baseDate);
            fxProperties.Set(CurveProp.Market, "LIVE");
            fxProperties.Set("Identifier", "FxCurve.AUD-USD");
            fxProperties.Set(CurveProp.Currency1, "AUD");
            fxProperties.Set(CurveProp.CurrencyPair, curveName);
            fxProperties.Set(CurveProp.CurveName, curveName);
            fxProperties.Set("Algorithm", algorithm);
            fxProperties.Set(CurveProp.OptimizeBuild, false);
            string[] instruments =  
                {   "AUDUSD-FxForward-0D", "AUDUSD-FxSpot-SP", "AUDUSD-FxForward-1M", "AUDUSD-FxForward-2M", "AUDUSD-FxForward-3M"
                };
            decimal[] rates = { 0.90m, 0.90m, 0.90m, 0.90m, 0.90m };
            var curve = Engine.CreateCurve(fxProperties, instruments, rates, null, null, null) as IFxCurve;
            return curve;
        }

        public IRateCurve TestRateCurve(DateTime baseDate)
        {
            const string curveName = "AUD-LIBOR-BBA-3M";
            const string indexTenor = "3M";
            const string algorithm = "FastLinearZero";
            const string marketEnvironment = "Bob";
            const double tolerance = 0.00000001;

            var props = new object[11, 2];
            props[0, 0] = CurveProp.CurveName;
            props[0, 1] = curveName;
            props[1, 0] = "Algorithm";
            props[1, 1] = algorithm;
            props[2, 0] = CurveProp.PricingStructureType;
            props[2, 1] = "RateCurve";
            props[3, 0] = "BuildDateTime";
            props[3, 1] = baseDate;
            props[4, 0] = CurveProp.IndexName;
            props[4, 1] = "AUD-LIBOR-BBA";
            props[5, 0] = CurveProp.IndexTenor;
            props[5, 1] = indexTenor;
            props[6, 0] = "Identifier";
            props[6, 1] = "Alex";
            props[7, 0] = CurveProp.Market;
            props[7, 1] = marketEnvironment;
            props[8, 0] = "BaseDate";
            props[8, 1] = baseDate;
            props[9, 0] = "Tolerance";
            props[9, 1] = tolerance;
            props[10, 0] = CurveProp.OptimizeBuild;
            props[10, 1] = false;
            var namevalues = new NamedValueSet(props);

            string[] instruments =  
                {   "AUD-Deposit-1D", "AUD-Deposit-1M", "AUD-Deposit-2M", "AUD-Deposit-3M",
                    "AUD-IRFuture-IR-0", "AUD-IRFuture-IR-1", "AUD-IRFuture-IR-2", "AUD-IRFuture-IR-3", 
                    "AUD-IRFuture-IR-4", "AUD-IRFuture-IR-5", "AUD-IRFuture-IR-6", "AUD-IRFuture-IR-7",
                    "AUD-IRSwap-3Y", "AUD-IRSwap-4Y", "AUD-IRSwap-5Y", "AUD-IRSwap-6Y", 
                    "AUD-IRSwap-7Y", "AUD-IRSwap-8Y", "AUD-IRSwap-9Y", "AUD-IRSwap-10Y", 
                    "AUD-IRSwap-12Y", "AUD-IRSwap-15Y", "AUD-IRSwap-20Y", "AUD-IRSwap-25Y", "AUD-IRSwap-30Y"
                };

            decimal[] rates =      {0.0725m,    0.0755m,    0.0766m,    0.07755m, 
                                    0.0781m,    0.07865m,   0.0794m,    0.07862m, 
                                    0.07808m,   0.07745m,   0.07752m,   0.0764m,
                                    0.06915m,   0.06745m,   0.06745m,   0.0785m,
                                    0.0786m,    0.0795m,    0.0725m,    0.0785m,
                                    0.0785m,    0.0785m,    0.0786m,    0.0787m, 0.0788m
                                   };

            decimal[] additional = {      
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m,       0.0m,       0.0m,       0.0m,
                                    0.0m
                                   };

            var curve = Engine.CreateCurve(namevalues, instruments, rates, additional, null, null);
            return curve as IRateCurve;
        }

        public ISwapLegEnvironment CreateInterestRateStreamTestEnvironment(DateTime baseDate)
        {
            var market = new SwapLegEnvironment();
            var curve = TestRateCurve(baseDate);
            var fxcurve = TestFxCurve(baseDate);
            market.AddPricingStructure("DiscountCurve", curve);
            market.AddPricingStructure("ForecastCurve", curve);
            market.AddPricingStructure("ReportingCurrencyFxCurve", fxcurve);
            return market;
        }

        public string BuildAndCacheRateCurve(DateTime baseDate)
        {
            var curve = TestRateCurve(baseDate);
            string curveId = curve.GetPricingStructureId().UniqueIdentifier;
            Engine.SaveCurve(curve);
            return curveId;
        }

        [TestMethod]
        public void GetInfoTwoSwaps()
        {
            string valReportId1 = Guid.NewGuid().ToString();
            ValuationReport valuationReport1 = CreateSwapValuationReport(valReportId1);
            string valReportId2 = Guid.NewGuid().ToString();
            ValuationReport valuationReport2 = CreateSwapValuationReport(valReportId2);
            Engine.Cache.SaveObject(valuationReport1, Engine.NameSpace + "." + valReportId1, null);
            Engine.Cache.SaveObject(valuationReport2, Engine.NameSpace + "." + valReportId2, null);
            var excelValuation = new Valuation();
            string mergedReportId = excelValuation.Merge(Engine.Cache, Engine.NameSpace, valReportId1, valReportId2, null, null, null, null, null, null, null, null);
            //var valuation = Engine.Cache.LoadObject<ValuationReport>(mergedReportId);
            List<ValuationInfoRangeItem> valuationInfoRangeItems = excelValuation.GetInfo(Engine.Cache, Engine.NameSpace, mergedReportId);
            object[,] range = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(valuationInfoRangeItems);
            Debug.Print("GetInfo (two swaps)");
            Debug.Print(ParameterFormatter.FormatObject(range));            
            Assert.AreEqual(3, valuationInfoRangeItems.Count);
            Assert.AreEqual(mergedReportId, valuationInfoRangeItems[0].Id);
            Assert.AreEqual("envelope", valuationInfoRangeItems[0].Description);
            Assert.AreEqual(valReportId1, valuationInfoRangeItems[1].Id);
            Assert.AreEqual("swap(fixed/float)", valuationInfoRangeItems[1].Description);
            Assert.AreEqual(valReportId2, valuationInfoRangeItems[2].Id);
            Assert.AreEqual("swap(fixed/float)", valuationInfoRangeItems[2].Description);
        }

        [TestMethod]
        public void TestMergeTwoValuationReports()
        {
            string valReportId1 = Guid.NewGuid().ToString();
            ValuationReport valuationReport1 = CreateSwapValuationReport(valReportId1);
            Assert.AreEqual(
                valReportId1,
                (((Trade) valuationReport1.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            string valReportId2 = Guid.NewGuid().ToString();
            ValuationReport valuationReport2 = CreateSwapValuationReport(valReportId2);
            Assert.AreEqual(
                valReportId2,
                ((valuationReport2.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            Engine.Cache.SaveObject(valuationReport1, Engine.NameSpace + "." + valReportId1, null);
            Engine.Cache.SaveObject(valuationReport2, Engine.NameSpace + "." + valReportId2, null);
            var excelValuation = new Valuation();
            string mergedReportId = excelValuation.Merge(Engine.Cache, Engine.NameSpace, valReportId1, valReportId2, null, null, null, null, null, null, null, null);
            var mergedReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + mergedReportId);
            Assert.AreEqual(2, mergedReport.tradeValuationItem.Length);
            Assert.AreNotEqual(mergedReport.header.messageId.Value, valReportId1);
            Assert.AreNotEqual(mergedReport.header.messageId.Value, valReportId2);
            Assert.AreEqual(
                valReportId1,
                ((mergedReport.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            Assert.AreEqual(
                valReportId2,
                ((mergedReport.tradeValuationItem[1].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value 
                );
        }

        [TestMethod]
        public void TestMergeThreeValuationReports()
        {
            string valReportId1 = Guid.NewGuid().ToString();
            ValuationReport valuationReport1 = CreateSwapValuationReport(valReportId1);
            Assert.AreEqual(
                valReportId1,
                (((Trade) valuationReport1.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            string valReportId2 = Guid.NewGuid().ToString();
            ValuationReport valuationReport2 = CreateSwapValuationReport(valReportId2);
            Assert.AreEqual(
                valReportId2,
                (((Trade) valuationReport2.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );

            string valReportId3 = Guid.NewGuid().ToString();
            ValuationReport valuationReport3 = CreateSwapValuationReport(valReportId3);
            Assert.AreEqual(
                valReportId3,
                (((Trade) valuationReport3.tradeValuationItem[0].Items[0]).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            Engine.Cache.SaveObject(valuationReport1, Engine.NameSpace + "." + valReportId1, null);
            Engine.Cache.SaveObject(valuationReport2, Engine.NameSpace + "." + valReportId2, null);
            Engine.Cache.SaveObject(valuationReport3, Engine.NameSpace + "." + valReportId3, null);
            var excelValuation = new Valuation();
            string mergedReport123Id = excelValuation.Merge(Engine.Cache, Engine.NameSpace, valReportId1, valReportId2, valReportId3, null, null, null, null, null, null, null);
            string mergedReport312Id = excelValuation.Merge(Engine.Cache, Engine.NameSpace, valReportId3, valReportId1, valReportId2, null, null, null, null, null, null, null);
            var merged123Report = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + mergedReport123Id);
            Assert.AreEqual(3, merged123Report.tradeValuationItem.Length);
            Assert.AreNotEqual(merged123Report.header.messageId.Value, valReportId1);
            Assert.AreNotEqual(merged123Report.header.messageId.Value, valReportId2);
            Assert.AreNotEqual(merged123Report.header.messageId.Value, valReportId3);
            Assert.AreEqual(
                valReportId1,
                ((merged123Report.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            Assert.AreEqual(
                valReportId2,
                ((merged123Report.tradeValuationItem[1].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value 
                );
            Assert.AreEqual(
                valReportId3,
                ((merged123Report.tradeValuationItem[2].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value 
                );
            var merged312Report = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + mergedReport312Id);
            Assert.AreEqual(3, merged312Report.tradeValuationItem.Length);
            Assert.AreNotEqual(merged312Report.header.messageId.Value, valReportId1);
            Assert.AreNotEqual(merged312Report.header.messageId.Value, valReportId2);
            Assert.AreNotEqual(merged312Report.header.messageId.Value, valReportId3);
            Assert.AreEqual(
                valReportId3,
                ((merged312Report.tradeValuationItem[0].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value
                );
            Assert.AreEqual(
                valReportId1,
                ((merged312Report.tradeValuationItem[1].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value 
                );
            Assert.AreEqual(
                valReportId2,
                ((merged312Report.tradeValuationItem[2].Items[0] as Trade).tradeHeader.partyTradeIdentifier[0].Items[0] as TradeId).Value 
                );
        }

        private ValuationReport CreateSwapValuationReport(string valuationReportId)
        {
            var rateCurve = CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Swap swap = GenerateSwapParametricWithCashflows();
            var fpml = rateCurve.GetDiscountRateCurveFpML();
            var pair = new Pair<YieldCurve, YieldCurveValuation>(fpml.First, fpml.Second);
            var marketFactory = new MarketFactory();
            marketFactory.AddYieldCurve(pair);
            Market market = marketFactory.Create();          
            const string baseParty = "NAB";
            string tradeId = valuationReportId;
            DateTime tradeDate = DateTime.Today;
            return ValuationReportGenerator.Generate(valuationReportId, baseParty, tradeId, tradeDate, swap, market, new AssetValuation());
        }

        private const string _NAB = "NAB";

        [TestMethod]
        public void TestCreateSwaptionValuationReport()
        {
            var curves = CreateInterestRateStreamTestEnvironment(DateTime.Now);
            Swaption swaption = GenerateSwaptionParametricWithCashflows();
            var marketFactory = new MarketFactory();
            marketFactory.AddYieldCurve(curves.GetForecastRateCurveFpML());
            Market market = marketFactory.Create();
            const string baseParty = _NAB;
            var assetValuation = new AssetValuation();
            var listOfQuotations = new List<Quotation>();
            IEnumerable<StringDoubleRangeItem> valuationSet = CreateValuationSetList(54321, 123.5);
            foreach (StringDoubleRangeItem item in valuationSet)
            {
                var quotation = new Quotation
                    {
                        measureType = AssetMeasureTypeHelper.Parse(item.StringValue),
                        value = (decimal) item.DoubleValue,
                        valueSpecified = true
                    };

                listOfQuotations.Add(quotation);
            }
            assetValuation.quote = listOfQuotations.ToArray();
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, "0001", DateTime.Now, swaption, market, assetValuation);
            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(typeof(Document), valuationReport));
            string s1 = XmlSerializerHelper.SerializeToString(typeof(Document), valuationReport);
            XmlSerializerHelper.DeserializeFromString<ValuationReport>(typeof(Document), s1);
            XmlSerializerHelper.SerializeToFile(typeof(Document), valuationReport, "some-valuation-Id");
            XmlSerializerHelper.DeserializeFromFile<ValuationReport>(typeof(Document), "some-valuation-Id");
        }

        private static IEnumerable<StringDoubleRangeItem> CreateValuationSetList(double npv, double dv01)
        {
            var list = new List<StringDoubleRangeItem>();
            var npvItem = new StringDoubleRangeItem {StringValue = AssetMeasureEnum.NPV.ToString(), DoubleValue = npv};
            list.Add(npvItem);
            var dv01Item = new StringDoubleRangeItem {StringValue = "DE@R", DoubleValue = dv01};
            list.Add(dv01Item);
            return list;
        }

        [TestMethod]
        public void TestCreateFraValuationReport1()
        {
            var rateCurve = TestRateCurve(new DateTime(2009, 7, 15));
            var fra = new Fra();
            var pair = rateCurve.GetFpMLData();
            var marketFactory = new MarketFactory();
            marketFactory.AddPricingStructure(pair);
            Market market = marketFactory.Create();
            const string baseParty = _NAB;
            var assetValuation = new AssetValuation();
            var listOfQuotations = new List<Quotation>();
            IEnumerable<StringDoubleRangeItem> valuationSet = CreateValuationSetList(54321, 123.5);
            foreach (StringDoubleRangeItem item in valuationSet)
            {
                var quotation = new Quotation
                    {
                        measureType = AssetMeasureTypeHelper.Parse(item.StringValue),
                        value = (decimal) item.DoubleValue,
                        valueSpecified = true
                    };
                listOfQuotations.Add(quotation);
            }
            assetValuation.quote = listOfQuotations.ToArray();
            ValuationReport valuationReport = ValuationReportGenerator.Generate("some-valuation-Id", baseParty, fra, market, assetValuation);
            Debug.WriteLine("ValuationReport:");
            Debug.WriteLine(XmlSerializerHelper.SerializeToString(valuationReport));
        }

        public Swap GenerateSwapParametricWithCashflows()
        {
            var payLeg = new SwapLegParametersRange_Old
                {
                    AdjustedType = AdjustedType.Unadjusted,
                    EffectiveDate = new DateTime(1994, 12, 14),
                    MaturityDate = new DateTime(1999, 12, 14),
                    FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                    RollConvention = "14",
                    InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                    FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                    NotionalAmount = 1000000,
                    LegType = LegType.Fixed,
                    Currency = "AUD",
                    CouponOrLastResetRate = 0.08m,
                    PaymentFrequency = "6M",
                    DayCount = "Actual360",
                    PaymentCalendar = "AUSY",
                    PaymentBusinessDayAdjustments = "FOLLOWING",
                    FixingCalendar = "AUSY-GBLO",
                    FixingBusinessDayAdjustments = "MODFOLLOWING",
                    DiscountCurve = "AUD-LIBOR",
                    DiscountingType = "Standard"
                };

            var receiveLeg = new SwapLegParametersRange_Old
                {
                    AdjustedType = AdjustedType.Unadjusted,
                    EffectiveDate = new DateTime(1994, 12, 14),
                    MaturityDate = new DateTime(1999, 12, 14),
                    FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                    RollConvention = "14",
                    InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                    FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                    NotionalAmount = 1000000,
                    LegType = LegType.Floating,
                    Currency = "AUD",
                    FloatingRateSpread = 0,
                    PaymentFrequency = "6M",
                    DayCount = "Actual360",
                    PaymentCalendar = "AUSY",
                    PaymentBusinessDayAdjustments = "FOLLOWING",
                    FixingCalendar = "AUSY-GBLO",
                    FixingBusinessDayAdjustments = "MODFOLLOWING",
                    DiscountCurve = "AUD-LIBOR",
                    ForecastCurve = "AUD-LIBOR",
                    DiscountingType = "Standard"
                };

            var marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            marketEnvironment.Id = "1234567";
            MarketEnvironmentRegistry.Add(marketEnvironment);
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            MarketEnvironmentRegistry.Remove("1234567");
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            return swap;
        }

        public Swaption GenerateSwaptionParametricWithCashflows()
        {
            var payLeg = new SwapLegParametersRange_Old
                {
                    AdjustedType = AdjustedType.Unadjusted,
                    EffectiveDate = new DateTime(1994, 12, 14),
                    MaturityDate = new DateTime(1999, 12, 14),
                    FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                    RollConvention = "14",
                    InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                    FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                    NotionalAmount = 1000000,
                    LegType = LegType.Fixed,
                    Currency = "AUD",
                    CouponOrLastResetRate = 0.08m,
                    PaymentFrequency = "6M",
                    DayCount = "Actual360",
                    PaymentCalendar = "AUSY",
                    PaymentBusinessDayAdjustments = "FOLLOWING",
                    FixingCalendar = "AUSY-GBLO",
                    FixingBusinessDayAdjustments = "MODFOLLOWING",
                    DiscountCurve = "AUD-LIBOR",
                    DiscountingType = "Standard"
                };

            var receiveLeg = new SwapLegParametersRange_Old
                {
                    AdjustedType = AdjustedType.Unadjusted,
                    EffectiveDate = new DateTime(1994, 12, 14),
                    MaturityDate = new DateTime(1999, 12, 14),
                    FirstRegularPeriodStartDate = new DateTime(1995, 6, 14),
                    RollConvention = "14",
                    InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                    FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                    NotionalAmount = 1000000,
                    LegType = LegType.Floating,
                    Currency = "AUD",
                    FloatingRateSpread = 0,
                    PaymentFrequency = "6M",
                    DayCount = "Actual360",
                    PaymentCalendar = "AUSY",
                    PaymentBusinessDayAdjustments = "FOLLOWING",
                    FixingCalendar = "AUSY-GBLO",
                    FixingBusinessDayAdjustments = "MODFOLLOWING",
                    DiscountCurve = "AUD-LIBOR",
                    ForecastCurve = "AUD-LIBOR",
                    DiscountingType = "Standard"
                };
            var marketEnvironment = CreateInterestRateStreamTestEnvironment(new DateTime(1994, 12, 14));
            var valuationDT = new DateTime(1994, 12, 20);
            Swap swap = SwapGenerator.GenerateDefinitionCashflowsAmounts(Engine.Logger, Engine.Cache, Engine.NameSpace, payLeg, null, receiveLeg, null, null, null, null, marketEnvironment, valuationDT);
            Assert.AreEqual(swap.swapStream.Length, 2);
            Assert.IsNotNull(swap.swapStream[0].cashflows);
            Assert.IsNotNull(swap.swapStream[1].cashflows);
            var swaption = new Swaption {swap = swap};
            return swaption;
        }
    }
}