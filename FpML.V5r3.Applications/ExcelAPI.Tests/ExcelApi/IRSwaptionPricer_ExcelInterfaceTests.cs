#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Constants;
//using Orion.CurveEngine.Tests.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
//using Orion.ModelFramework;
//using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ValuationEngine.Pricers;
//using Orion.CurveEngine.Tests;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    public partial class ExcelAPITests
    {
        //private static SwapLegParametersRange_Old CreateSwapLegParametersRange(

        //    string payer,
        //    string receiver,

        //    decimal notionalAmount,
        //    DateTime effectiveDate,
        //    DateTime maturityDate,
        //    DateTime firstRollDate,
        //    AdjustedType adjustedType,
        //    string initialStubType,
        //    string finalStubType,
        //    string rollConvention,

        //    LegType legType,
        //    string currency,
        //    decimal couponOrLastResetRate,
        //    decimal floatingRateSpread,
        //    string paymentFrequency,
        //    string dayCount,
        //    string paymentCalendar,
        //    string paymentBusinessDayAdjustments,
        //    string fixingCalendar,
        //    string fixingBusinessDayAdjustments,
        //    string discountCurve,
        //    string forecastCurve)
        //{
        //    SwapLegParametersRange_Old result = new SwapLegParametersRange_Old();

        //    result.Payer = payer;
        //    result.Receiver = receiver;

        //    result.NotionalAmount = notionalAmount;
        //    result.EffectiveDate = effectiveDate;
        //    result.MaturityDate = maturityDate;
        //    result.FirstRegularPeriodStartDate = firstRollDate;
        //    result.AdjustedType = adjustedType;
        //    result.InitialStubType = initialStubType;
        //    result.FinalStubType = finalStubType;
        //    result.RollConvention = rollConvention;

        //    result.LegType = legType;
        //    result.Currency = currency;
        //    result.CouponOrLastResetRate = couponOrLastResetRate;
        //    result.FloatingRateSpread = floatingRateSpread;
        //    result.PaymentFrequency = paymentFrequency;
        //    result.DayCount = dayCount;
        //    result.PaymentCalendar = paymentCalendar;
        //    result.PaymentBusinessDayAdjustments = paymentBusinessDayAdjustments;
        //    result.FixingCalendar = fixingCalendar;
        //    result.FixingBusinessDayAdjustments = fixingBusinessDayAdjustments;
        //    result.DiscountCurve = discountCurve;
        //    result.ForecastCurve = forecastCurve;
        //    result.DiscountingType = "Standard";


        //    return result;
        //}


//        private static SwapLegParametersRange_Old Create_Fixed_AUD_3M_SwapLegParametersRange(string payer, string receiver,
//            DateTime startDate, decimal couponRate, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve)
//        {
//            return CreateSwapLegParametersRange(payer, receiver,
//                100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(3), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
//                LegType.Fixed, "AUD", couponRate, 0, "3M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, null);
//        }
//
//        private static SwapLegParametersRange_Old Create_Floating_AUD_3M_SwapLegParametersRange(string payer, string receiver, DateTime startDate, decimal spread, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve, string projectCurve)
//        {
//            return CreateSwapLegParametersRange(payer, receiver,
//                100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(3), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
//                LegType.Floating, "AUD", 0, spread, "3M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, projectCurve);
//        }

        //private static SwapLegParametersRange_Old Create_Fixed_AUD_6M_SwapLegParametersRange(string payer, string receiver, DateTime startDate, decimal couponRate, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve)
        //{
        //    return CreateSwapLegParametersRange(payer, receiver,
        //                                        100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(6), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
        //                                        LegType.Fixed, "AUD", couponRate, 0, "6M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, null);
        //}

        //private static SwapLegParametersRange_Old Create_Floating_AUD_6M_SwapLegParametersRange(string payer, string receiver, DateTime startDate, decimal spread, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve, string projectCurve)
        //{
        //    return CreateSwapLegParametersRange(payer, receiver,
        //                                        100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(6), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
        //                                        LegType.Floating, "AUD", 0, spread, "6M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, projectCurve);
        //}
        [TestMethod]
        public void CreateSwaptionValuation()
        {
            DateTime valuationDate = DateTime.Today;

            SwaptionPricer irSwaptionPricer = new InterestRateSwaptionPricer();

            string discountCurveID = BuildAndCacheRateCurve(valuationDate); //RateCurveExcelInterfaceTests.ExcelInterface_CreateAUDCurveFromDepostSwapsFuturesFras_WithDates(valuationDate, valuationDate);
            string projectionCurveID = discountCurveID;

            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            var payCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            payCFRangeItemList[0].CouponType = "fixed";// that should test case insensitive nature of coupons
            payCFRangeItemList[1].CouponType = "Fixed";//
            var receiveCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            receiveCFRangeItemList[0].CouponType = "float";// that should test case insensitive nature of coupons
            receiveCFRangeItemList[1].CouponType = "Float";//          
            var tradeRange = new TradeRange {Id = "TradeId_12345", TradeDate = valuationDate};
            var leg1PrincipalExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg2PrincipalExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            var swaptionParametersRange = new SwaptionParametersRange
                {
                    Premium = 456789.12m,
                    PremiumCurrency = "AUD",
                    PremiumPayer = CounterParty,
                    PremiumReceiver = _NAB,
                    ExpirationDate = valuationDate.AddDays(10),
                    ExpirationDateCalendar = "AUSY-GBLO",
                    ExpirationDateBusinessDayAdjustments = "FOLLOWING",
                    PaymentDate = valuationDate.AddDays(20),
                    PaymentDateCalendar = "USNY-GBLO",
                    PaymentDateBusinessDayAdjustments = "MODFOLLOWING",
                    EarliestExerciseTime = new TimeSpan(10, 0, 0).TotalDays,
                    ExpirationTime = new TimeSpan(11, 0, 0).TotalDays,
                    AutomaticExcercise = false
                };
            List<PartyIdRangeItem> partyList = GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = GetOtherPartyPaymentList("counterparty", "cost center");
            List<FeePaymentRangeItem> feePaymentRangeItems = GetFeeList("counterparty", "book");          
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            string valuatonId = irSwaptionPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null,
                swaptionParametersRange,
                CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                payFixed, receiveFloat,
                payCFRangeItemList, receiveCFRangeItemList,
                leg1PrincipalExchangeCashflowList, leg2PrincipalExchangeCashflowList,
                leg1BulletPaymentList, leg2BulletPaymentList,
                partyList, otherPartyPaymentRangeItems,
                feePaymentRangeItems);
            var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuatonId);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
        }

        //private readonly GenericInMemoryCollection<ValuationReport> _valuationReportCollection = GenericInMemoryCollection<ValuationReport>.Instance;


        //private static List<StringObjectRangeItem> CreateValuationSetList(double npv, double dv01)
        //{
        //    List<StringObjectRangeItem> list = new List<StringObjectRangeItem>();

        //    StringObjectRangeItem npvItem = new StringObjectRangeItem();
        //    npvItem.StringValue = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.NPV);
        //    npvItem.ObjectValue = npv;
        //    list.Add(npvItem);

        //    StringObjectRangeItem dv01Item = new StringObjectRangeItem();
        //    dv01Item.StringValue = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.DE_R);
        //    dv01Item.ObjectValue = dv01;
        //    list.Add(dv01Item);

        //    return list;
        //}

        //const string _NAB = "NAB";
        //const string _counterParty = "CounterParty";

        //private static ValuationRange CreateValuationRangeForNAB(DateTime valuationDate)
        //{
        //    ValuationRange result = new ValuationRange();

        //    result.ValuationDate = valuationDate;
        //    result.BaseParty = _NAB;

        //    return result;
        //}

//        [Ignore]
//        [TestMethod]
//        public void Test_GetPremium_AUD_6M_100M_5Y_EXPIRY_05_VOL_20PCT()
//        {
//            DateTime valuationDate = DateTime.Today;
//
//            IRSwaptionPricer swaptionPricer = new IRSwaptionPricer();
//
//            string discountCurveID = RateCurveExcelInterfaceTests.ExcelInterface_CreateAUDCurveFromDepostSwapsFuturesFras_WithDates(valuationDate, valuationDate);
//            string projectionCurveID = discountCurveID;
//
//            SwapLegParametersRange_Old payLegParametersRange = Create_Fixed_AUD_6M_SwapLegParametersRange(valuationDate,0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
//            SwapLegParametersRange_Old receiveLegParametersRange = Create_Floating_AUD_6M_SwapLegParametersRange(valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);  
//            
//            ValuationRange valuationRange = CreateValuationRange(valuationDate);
//
//            SwaptionParametersRange swaptionParametersRange = new SwaptionParametersRange();
//
//            swaptionParametersRange.ExpirationDate = receiveLegParametersRange.EffectiveDate.AddDays(180);
//            swaptionParametersRange.ExpirationDateCalendar = "GBLO-AUSY";
//            swaptionParametersRange.ExpirationDateBusinessDayAdjustments = "FOLLOWING";
//
//            swaptionParametersRange.PaymentDate = receiveLegParametersRange.EffectiveDate;
//            swaptionParametersRange.PaymentDateCalendar = "AUSY";
//            swaptionParametersRange.PaymentDateBusinessDayAdjustments = "FOLLOWING";
//
//            swaptionParametersRange.Premium = 0;//this is what we need to re-calc
//            swaptionParametersRange.StrikeRate = 0.08m;
//            swaptionParametersRange.Volatility = 0.2m;//20%
//
//            //ValuationResultRange resultRange = swaptionPricer.GetPremium(
//            double optionPremium = swaptionPricer.GetPremiumImpl(
//            payLegParametersRange,
//            receiveLegParametersRange,
//            swaptionParametersRange,
//            valuationRange);
//
//            
////            string valuationResultRangeAsString = ValuationResultRangeToString(resultRange);
//            Debug.Print("Premium : '{0}'", optionPremium);
//        }

    }
}
