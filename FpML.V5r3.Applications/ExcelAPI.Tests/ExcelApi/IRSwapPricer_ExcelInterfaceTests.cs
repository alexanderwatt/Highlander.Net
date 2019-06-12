#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FpML.V5r3.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework.PricingStructures;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Pricers;
using Orion.TestHelpers;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.Helpers.XsdClassesFieldResolver;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    public partial class ExcelAPITests
    {       
        private static ValuationRange CreateValuationRange(DateTime valuationDate)
        {
            var result = new ValuationRange {ValuationDate = valuationDate, BaseParty = _NAB};
            return result;
        }

        public const string CounterParty = "CounterParty";

        private static SwapLegParametersRange_Old CreateSwapLegParametersRange(
            
            string  payer,
            string  receiver,

            decimal notinalAmount, 
            DateTime effectiveDate, 
            DateTime maturityDate, 
            DateTime firstRollDate, 
            AdjustedType adjustedType, 
            string initialStubType, 
            string finalStubType, 
            string rollConvention,           
            LegType legType, 
            string currency, 
            decimal couponOrLastResetRate, 
            decimal floatingRateSpread, 
            string paymentFrequency, 
            string dayCount, 
            string paymentCalendar, 
            string paymentBusinessDayAdjustments, 
            string fixingCalendar, 
            string fixingBusinessDayAdjustments, 
            string discountCurve, 
            string forecastCurve)
        {
            var result = new SwapLegParametersRange_Old
                             {
                                 Payer = payer,
                                 Receiver = receiver,
                                 NotionalAmount = notinalAmount,
                                 EffectiveDate = effectiveDate,
                                 MaturityDate = maturityDate,
                                 FirstRegularPeriodStartDate = firstRollDate,
                                 AdjustedType = adjustedType,
                                 InitialStubType = initialStubType,
                                 FinalStubType = finalStubType,
                                 RollConvention = rollConvention,
                                 LegType = legType,
                                 Currency = currency,
                                 CouponOrLastResetRate = couponOrLastResetRate,
                                 FloatingRateSpread = floatingRateSpread,
                                 PaymentFrequency = paymentFrequency,
                                 DayCount = dayCount,
                                 PaymentCalendar = paymentCalendar,
                                 PaymentBusinessDayAdjustments = paymentBusinessDayAdjustments,
                                 FixingCalendar = fixingCalendar,
                                 FixingBusinessDayAdjustments = fixingBusinessDayAdjustments,
                                 DiscountCurve = discountCurve,
                                 ForecastCurve = forecastCurve,
                                 DiscountingType = "Standard"
                             };
            return result;
        }


        private static SwapLegParametersRange_Old CreateFixedAUD3MSwapLegParametersRange(string payer, string receiver, 
                                                                                         DateTime startDate, decimal couponRate, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve)
        {
            return CreateSwapLegParametersRange(payer, receiver, 
                                                100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(3), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
                                                LegType.Fixed, "AUD", couponRate, 0, "3M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, null);
        }

        private static SwapLegParametersRange_Old CreateFloatingAUD3MSwapLegParametersRange(string payer, string receiver, DateTime startDate, decimal spread, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve, string projectCurve)
        {
            return CreateSwapLegParametersRange(payer, receiver, 
                                                100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(3), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
                                                LegType.Floating, "AUD", 0, spread, "3M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, projectCurve);
        }

        private static SwapLegParametersRange_Old CreateFixedAUD6MSwapLegParametersRange(string payer, string receiver, DateTime startDate, decimal couponRate, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve)
        {
            return CreateSwapLegParametersRange(payer, receiver, 
                                                100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(6), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
                                                LegType.Fixed, "AUD", couponRate, 0, "6M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, null);
        }

        private static SwapLegParametersRange_Old CreateFloatingAUD6MSwapLegParametersRange(string payer, string receiver, DateTime startDate, decimal spread, string dayCount, string paymentCalendar, string paymentBDA, string fixingCalendar, string fixingDBA, string discountCurve, string projectCurve)
        {
            return CreateSwapLegParametersRange(payer, receiver, 
                                                100000000m, startDate, startDate.AddYears(5), startDate.AddMonths(6), AdjustedType.Adjusted, "ShortInitial", "ShortInitial", RollConventionEnumHelper.GetRollConventionAsString(startDate.Day),
                                                LegType.Floating, "AUD", 0, spread, "6M", dayCount, paymentCalendar, paymentBDA, fixingCalendar, fixingDBA, discountCurve, projectCurve);
        }
        
        [TestMethod]
        public void TestPriceAUD3M100M5Y()
        {
            var valuationDate = new DateTime(2008, 02, 29);
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payLegParametersRange = CreateFixedAUD3MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveLegParametersRange = CreateFloatingAUD3MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);           
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, receiveLegParametersRange, valuationRange);
            string valuationResultRangeAsString = ValuationResultRangeToString(resultRange);
            Debug.WriteLine(valuationResultRangeAsString);
        }

        
        [TestMethod]
        public void TestGetDetailedCashflowsAUD3M100M5Y()
        {
            var valuationDate = new DateTime(2008, 02, 14);
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payLegParametersRange = CreateFixedAUD3MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveLegParametersRange = CreateFloatingAUD3MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);              
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            var payCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, valuationRange);
            Assert.AreEqual(payCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
            Debug.WriteLine("Pay cf:");
            Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            var receiveCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveLegParametersRange, valuationRange);
            Assert.AreEqual(receiveCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
        }


        [TestMethod]
        public void VanillaAllOptionalParametersAreNull()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate); 
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            //  Get price of vanilla swap
            //
            ValuationResultRange valuationResultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            List<InputCashflowRangeItem> payCFRangeItemList = null;
            List<InputCashflowRangeItem> receiveCFRangeItemList = null;
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList = null;
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowList = null;
            List<AdditionalPaymentRangeItem> leg1BulletPaymentList = null;
            List<AdditionalPaymentRangeItem> leg2BulletPaymentList = null;
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            Pair<ValuationResultRange, Swap> nonVanillaPriceImpl = InterestRateSwapPricer.GetPriceAndGeneratedFpMLSwap(Engine.Logger, Engine.Cache, Engine.NameSpace, valuationRange,
                                                                                                             payFixed, null, receiveFloat, null,
                                                                                                             payCFRangeItemList, receiveCFRangeItemList,
                                                                                                             leg1PrincipalExchangeCashflowList, leg2PrincipalExchangeCashflowList,
                                                                                                             leg1BulletPaymentList, leg2BulletPaymentList);
            //  Results should be exactly the same.
            //
            Assert.AreEqual(valuationResultRange.PresentValue, nonVanillaPriceImpl.First.PresentValue);
            Assert.AreEqual(valuationResultRange.FutureValue, nonVanillaPriceImpl.First.FutureValue);
            Assert.AreEqual(valuationResultRange.PayLegPresentValue, nonVanillaPriceImpl.First.PayLegPresentValue);
            Assert.AreEqual(valuationResultRange.ReceiveLegPresentValue, nonVanillaPriceImpl.First.ReceiveLegPresentValue);
            Assert.AreEqual(valuationResultRange.PayLegFutureValue, nonVanillaPriceImpl.First.PayLegFutureValue);
            Assert.AreEqual(valuationResultRange.ReceiveLegFutureValue, nonVanillaPriceImpl.First.ReceiveLegFutureValue);
            // NO PExs
            //
            foreach (InterestRateStream interestRateStream in nonVanillaPriceImpl.Second.swapStream)
            {
                CollectionAssertExtension.IsEmpty(interestRateStream.cashflows.principalExchange);
            }
            // No payments
            //
            CollectionAssertExtension.IsEmpty(nonVanillaPriceImpl.Second.additionalPayment);
        }

        [TestMethod]
        public void TestGetDetailedCashflowsAUD3M100M5YAmortisingNotional()
        {
            var valuationDate = new DateTime(2008, 02, 14);
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payLegParametersRange = CreateFixedAUD3MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveLegParametersRange = CreateFloatingAUD3MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);            
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<DateTimeDoubleRangeItem> payNotional = GetAmortNotional(payLegParametersRange, 3);
            List<DetailedCashflowRangeItem> payCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, 
                                                                                                                      payNotional, valuationRange);
            //Debug.WriteLine("Pay cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            Assert.AreEqual(payCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
            List<DateTimeDoubleRangeItem> recNotional = GetAmortNotional(receiveLegParametersRange, 6);//DIFFERENT FROM ROLLING FREQUENCY!
            List<DetailedCashflowRangeItem> receiveCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveLegParametersRange, recNotional, valuationRange);
            //Debug.WriteLine("Receive cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
            Assert.AreEqual(receiveCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
        }

        [TestMethod]
        public void TestGetDetailedCashflowsAUD3M100M5YNotionalScheduleIsNull()
        {
            var valuationDate = new DateTime(2008, 02, 14);
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payLegParametersRange = CreateFixedAUD3MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveLegParametersRange = CreateFloatingAUD3MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);             
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<DateTimeDoubleRangeItem> payNotional = null;
            List<DetailedCashflowRangeItem> payCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, 
                                                                                                                      payNotional, valuationRange);
            //Debug.WriteLine("Pay cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            Assert.AreEqual(payCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
            List<DateTimeDoubleRangeItem> recNotional = GetAmortNotional(receiveLegParametersRange, 6);//DIFFERENT FROM ROLLING FREQUENCY!
            List<DetailedCashflowRangeItem> receiveCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveLegParametersRange, recNotional, valuationRange);
            //Debug.WriteLine("Receive cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
            Assert.AreEqual(receiveCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
        }

        [TestMethod]
        public void TestGetDetailedCashflowsAUD3M100M5YCleanupAmortisingNotional()
        {
            var valuationDate = new DateTime(2008, 02, 14);
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payLegParametersRange = CreateFixedAUD3MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveLegParametersRange = CreateFloatingAUD3MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);              
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<DateTimeDoubleRangeItem> payNotional = GetCleanupAmortNotional(payLegParametersRange, 3, 20);
            List<DetailedCashflowRangeItem> payCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, 
                                                                                                                      payNotional, valuationRange);
            //Debug.WriteLine("Pay cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            Assert.AreEqual(payCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
            List<DateTimeDoubleRangeItem> recNotional = GetCleanupAmortNotional(receiveLegParametersRange, 3, 20);
            List<DetailedCashflowRangeItem> receiveCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveLegParametersRange, recNotional, valuationRange);
            //Debug.WriteLine("Receive cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
            Assert.AreEqual(receiveCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("5Y"), PeriodHelper.Parse("3M")));//number of cfs
        }


        private static Schedule GetAmortNotionalFpML(SwapLegParametersRange_Old leg, int rollEveryNMonth)
        {
            var notionalScheduleList = new List<Pair<DateTime, decimal>>
                                           {
                                               new Pair<DateTime, decimal>(leg.EffectiveDate, leg.NotionalAmount),
                                               new Pair<DateTime, decimal>(
                                                   leg.EffectiveDate.AddMonths(rollEveryNMonth*1),
                                                   leg.NotionalAmount*0.9m),
                                               new Pair<DateTime, decimal>(
                                                   leg.EffectiveDate.AddMonths(rollEveryNMonth*2),
                                                   leg.NotionalAmount*0.8m),
                                               new Pair<DateTime, decimal>(
                                                   leg.EffectiveDate.AddMonths(rollEveryNMonth*3),
                                                   leg.NotionalAmount*0.7m),
                                               new Pair<DateTime, decimal>(
                                                   leg.EffectiveDate.AddMonths(rollEveryNMonth*4),
                                                   leg.NotionalAmount*0.6m)
                                           };

            Schedule notionalSchedule = ScheduleHelper.Create(notionalScheduleList);          
            return notionalSchedule;
        }

        private static List<DateTimeDoubleRangeItem> GetAmortNotional(SwapLegParametersRange_Old leg, int rollEveryNMonth)
        {
            var list = new List<DateTimeDoubleRangeItem>
                           {
                               DateTimeDoubleRangeItem.Create(leg.EffectiveDate, (double) leg.NotionalAmount),
                               DateTimeDoubleRangeItem.Create(leg.EffectiveDate.AddMonths(rollEveryNMonth*1),
                                                              (double) (leg.NotionalAmount*0.9m)),
                               DateTimeDoubleRangeItem.Create(leg.EffectiveDate.AddMonths(rollEveryNMonth*2),
                                                              (double) (leg.NotionalAmount*0.8m)),
                               DateTimeDoubleRangeItem.Create(leg.EffectiveDate.AddMonths(rollEveryNMonth*3),
                                                              (double) (leg.NotionalAmount*0.7m)),
                               DateTimeDoubleRangeItem.Create(leg.EffectiveDate.AddMonths(rollEveryNMonth*4),
                                                              (double) (leg.NotionalAmount*0.6m))
                           };
            return list;
        }

        private static List<DateTimeDoubleRangeItem> GetCleanupAmortNotional(SwapLegParametersRange_Old leg, int rollEveryNMonth, int rollsNumber)
        {
            var list = new List<DateTimeDoubleRangeItem>
                                                     {
                                                         DateTimeDoubleRangeItem.Create(leg.EffectiveDate,
                                                                                        (double) leg.NotionalAmount)
                                                     };
            double amortAmount = (double)leg.NotionalAmount / rollsNumber;
            for (int i = 1; i < rollsNumber; ++i)
            {
                double newNotional = (double)leg.NotionalAmount - (amortAmount * i);
                list.Add(DateTimeDoubleRangeItem.Create(leg.EffectiveDate.AddMonths(rollEveryNMonth * i), newNotional));
            }
            return list;
        }

        [TestMethod]
        public void TestPriceAUD6M100M5Y()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate,0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);              
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            string valuationResultRangeAsString = ValuationResultRangeToString(resultRange);
            Debug.WriteLine(valuationResultRangeAsString);
        }

        [TestMethod]
        public void TestPricePayerAndReceiverAreTheSame()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            const string payerAndReceiver = "payerAndReceiver";
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(payerAndReceiver, payerAndReceiver, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(payerAndReceiver, payerAndReceiver, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);             
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            Assert.AreEqual(0, resultRange.PresentValue);
            Assert.AreEqual(0, resultRange.FutureValue);
            Assert.AreEqual(0, resultRange.PayLegPresentValue);
            Assert.AreEqual(0, resultRange.ReceiveLegPresentValue);
            Assert.AreEqual(0, resultRange.PayLegFutureValue);
            Assert.AreEqual(0, resultRange.ReceiveLegFutureValue);
            Assert.AreEqual(0, resultRange.PresentValue);
            string valuationResultRangeAsString = ValuationResultRangeToString(resultRange);
            Debug.WriteLine(valuationResultRangeAsString);
        }

        [TestMethod]
        public void TestPriceAUD6M100M5YPayReceiveTheSameFixedLeg()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            SwapLegParametersRange_Old payFixedLeg = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFixedLeg = CreateFixedAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);          
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixedLeg, receiveFixedLeg, valuationRange);
            string valuationResultRangeAsString = ValuationResultRangeToString(resultRange);
            Debug.WriteLine(valuationResultRangeAsString);
            Assert.AreEqual(resultRange.PresentValue, 0);
            Assert.AreEqual(resultRange.FutureValue, 0);
            Assert.AreEqual(resultRange.PayLegPresentValue, resultRange.ReceiveLegPresentValue);
            Assert.AreEqual(resultRange.PayLegFutureValue, resultRange.ReceiveLegFutureValue);
        }

        [TestMethod]
        public void TestPriceFixedVsFloatFloatVsFixedFixedVsFixedFloatVsFloat()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old fixedLeg = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old floatLeg = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);            
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            //  FixedVsFloat
            //
            InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, fixedLeg, floatLeg, valuationRange);
            //  FloatVsFixed
            //
            InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, floatLeg, fixedLeg, valuationRange);           
            //  FixedVsFixed
            //
            InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, fixedLeg, fixedLeg, valuationRange);
            //  FloatVsFloat
            //
            InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, floatLeg, floatLeg, valuationRange);
        }

        [TestMethod]
        public void TestPriceAUD6M100M5YPayReceiveTheSameFloatingLeg()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFloatLeg = CreateFloatingAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);  
            SwapLegParametersRange_Old recFloatLeg = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);            
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFloatLeg, recFloatLeg, valuationRange);
            Assert.AreEqual(resultRange.PresentValue, 0);
            Assert.AreEqual(resultRange.FutureValue, 0);
            Assert.AreEqual(resultRange.PayLegPresentValue, resultRange.ReceiveLegPresentValue);
            Assert.AreEqual(resultRange.PayLegFutureValue, resultRange.ReceiveLegFutureValue);
        }

        [TestMethod]
        //increase payment frq -> decrease PV & FV
        public void TestPriceAUD6M100M5YIncreasePaymentFrequencyPayFixed()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);  
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            payFixed.PaymentFrequency = "3M";
            payFixed.FirstRegularPeriodStartDate = receiveFloat.EffectiveDate.AddMonths(3);
            ValuationResultRange resultRangeIncreasedPaymentFrq = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            Assert.IsTrue(resultRangeIncreasedPaymentFrq.PresentValue - resultRange.PresentValue < 0);
            Assert.IsTrue(resultRangeIncreasedPaymentFrq.FutureValue - resultRange.FutureValue < 0);
        }

        [TestMethod]
        //increase receive frq -> increase PV & FV
        public void TestPriceAUD6M100M5YIncreaseReceiveFrequencyPayFloat()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFloat = CreateFloatingAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            SwapLegParametersRange_Old receiveFixed = CreateFixedAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);           
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFloat, receiveFixed, valuationRange);
            receiveFixed.PaymentFrequency = "3M";
            receiveFixed.FirstRegularPeriodStartDate = receiveFixed.EffectiveDate.AddMonths(3);
            ValuationResultRange resultRangeIncreasedPaymentFrq = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFloat, receiveFixed, valuationRange);
            Assert.IsTrue(resultRangeIncreasedPaymentFrq.PresentValue - resultRange.PresentValue > 0);
            Assert.IsTrue(resultRangeIncreasedPaymentFrq.FutureValue - resultRange.FutureValue > 0);
        }

        [TestMethod]
        //increase pay frq -> decrease in PV & FV
        public void TestPriceAUD6M100M5YIncreasePayFrequencyRecFloat()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old recFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);            
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, recFloat, valuationRange);
            payFixed.PaymentFrequency = "3M";
            payFixed.FirstRegularPeriodStartDate = payFixed.EffectiveDate.AddMonths(3);
            ValuationResultRange resultRangeIncreasedReceiveFrq = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, recFloat, valuationRange);
            Assert.IsTrue(resultRangeIncreasedReceiveFrq.PresentValue - resultRange.PresentValue < 0);
            Assert.IsTrue(resultRangeIncreasedReceiveFrq.FutureValue - resultRange.FutureValue < 0);
        }

        [TestMethod]
        //increase receive frq -> increase PV & FV
        public void TestPriceAUD6M100M5YIncreaseReceiveFrequencyPayFixed()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);            
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            receiveFloat.PaymentFrequency = "3M";
            receiveFloat.FirstRegularPeriodStartDate = receiveFloat.EffectiveDate.AddMonths(3);
            ValuationResultRange resultRangeIncreasedReceiveFrq = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            //Assert.IsTrue(resultRangeIncreasedReceiveFrq.PresentValue - resultRange.PresentValue >= 0);
            Assert.IsTrue(resultRangeIncreasedReceiveFrq.FutureValue - resultRange.FutureValue > 0);
        }

        [TestMethod]
        public void GetDetailedCashflowsOverFlowExceptionError()//found on 16.01.2008
        {
            var valuationDate = new DateTime(2004, 02, 29);
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            var payLegParametersRange = new SwapLegParametersRange_Old
                                            {
                                                NotionalAmount = 100000000,
                                                EffectiveDate = DateTime.Parse("21/01/2008 12:00:00 AM"),
                                                MaturityDate = DateTime.Parse("22/01/2018 12:00:00 AM"),
                                                FirstRegularPeriodStartDate = DateTime.Parse("21/04/2008 12:00:00 AM"),
                                                AdjustedType = AdjustedType.Adjusted,
                                                InitialStubType = ""
                                            };
            payLegParametersRange.InitialStubType = "ShortInitial";
            payLegParametersRange.FinalStubType = "ShortFinal";
            payLegParametersRange.RollConvention = "Item21";
            payLegParametersRange.LegType = LegType.Floating;
            payLegParametersRange.Currency = "AUD";
            payLegParametersRange.CouponOrLastResetRate = 0;
            payLegParametersRange.FloatingRateSpread = 0;
            payLegParametersRange.PaymentFrequency = "3m";
            payLegParametersRange.DayCount = "ACT/365.FIXED";
            payLegParametersRange.PaymentCalendar = "Sydney";
            payLegParametersRange.PaymentBusinessDayAdjustments = "FOLLOWING";
            payLegParametersRange.FixingCalendar = "Sydney";
            payLegParametersRange.FixingBusinessDayAdjustments = "NONE";
            payLegParametersRange.DiscountCurve = discountCurveID;
            payLegParametersRange.ForecastCurve = projectionCurveID;
            payLegParametersRange.DiscountingType = "Standard";
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(payLegParametersRange);
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, FixingCalendar, PaymentCalendar);
            Debug.Print(XmlSerializerHelper.SerializeToString(cashflows));
            CalculationPeriod calculationPeriod0 = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(cashflows.paymentCalculationPeriod[0])[0];
            CalculationPeriod calculationPeriod1 = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(cashflows.paymentCalculationPeriod[1])[0];
            Assert.AreNotEqual(calculationPeriod0.adjustedStartDate, calculationPeriod1.adjustedStartDate);
            Assert.AreNotEqual(calculationPeriod0.unadjustedStartDate, calculationPeriod1.unadjustedStartDate);
        }

        [TestMethod]
        public void GetDetailedCashflowsOverFlowExceptionError2()//found on 16.01.2008
        {
            DateTime valuationDate = DateTime.Parse("19/01/2008 12:00:00 AM");
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            var payLegParametersRange = new SwapLegParametersRange_Old
                                                               {
                                                                   NotionalAmount = 100000000,
                                                                   EffectiveDate =
                                                                       DateTime.Parse("21/01/2008 12:00:00 AM"),
                                                                   MaturityDate =
                                                                       DateTime.Parse("22/01/2018 12:00:00 AM"),
                                                                   FirstRegularPeriodStartDate =
                                                                       DateTime.Parse("21/04/2008 12:00:00 AM"),
                                                                   AdjustedType = AdjustedType.Adjusted,
                                                                   InitialStubType = "ShortInitial",
                                                                   FinalStubType = "ShortFinal",
                                                                   RollConvention = "21",
                                                                   LegType = LegType.Floating,
                                                                   Currency = "AUD",
                                                                   CouponOrLastResetRate = 0,
                                                                   FloatingRateSpread = 0,
                                                                   PaymentFrequency = "3m",
                                                                   DayCount = "ACT/365.FIXED",
                                                                   PaymentCalendar = "Sydney",
                                                                   PaymentBusinessDayAdjustments = "FOLLOWING",
                                                                   FixingCalendar = "Sydney",
                                                                   FixingBusinessDayAdjustments = "NONE",
                                                                   DiscountCurve = discountCurveID,
                                                                   ForecastCurve = projectionCurveID,
                                                                   DiscountingType = "Standard"
                                                               };
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<InputCashflowRangeItem> payCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, valuationRange);
            Assert.AreEqual(payCashflowsArray.Count, IntervalHelper.Div(PeriodHelper.Parse("10Y"), PeriodHelper.Parse("3M")));//number of cfs
            //Debug.WriteLine("Pay cf:");
            //Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
        }

        [TestMethod]
        public void GetDetailedCashflowsVanilla()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<InputCashflowRangeItem> payCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            Debug.WriteLine("Pay cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            List<InputCashflowRangeItem> receiveCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            Debug.WriteLine("Receive cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
        }

        [TestMethod]
        public void GetPrincipalExchangesVanilla()
        {
            DateTime valuationDate = DateTime.Today;
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            payFixed.GeneratePrincipalExchanges = true;
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            receiveFloat.GeneratePrincipalExchanges = true;
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<InputCashflowRangeItem> payCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            Debug.WriteLine("Pay cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            List<InputCashflowRangeItem> receiveCashflowsArray = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            Debug.WriteLine("Receive cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
            var notionalSchedule = new List<DateTimeDoubleRangeItem>();
            List<PrincipalExchangeCashflowRangeItem> payPEArray = irSwapPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, notionalSchedule, valuationRange);
            Assert.AreEqual(2, payPEArray.Count);//2 pex - initial and final
            List<PrincipalExchangeCashflowRangeItem> receivePEArray = irSwapPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, notionalSchedule, valuationRange);
            Assert.AreEqual(2, receivePEArray.Count);//2 pex - initial and final
        }

        [TestMethod]
        public void GetPrincipalExchangesAmortNotional()
        {
            DateTime valuationDate = DateTime.Today;
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            payFixed.GeneratePrincipalExchanges = true;
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            receiveFloat.GeneratePrincipalExchanges = true;
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<DateTimeDoubleRangeItem> payNotional = GetAmortNotional(payFixed, 6);
            List<PrincipalExchangeCashflowRangeItem> payPEArray = irSwapPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, payNotional, valuationRange);
            Assert.AreEqual(payNotional.Count + 1, payPEArray.Count);
            List<DateTimeDoubleRangeItem> recNotional = GetAmortNotional(receiveFloat, 6);
            List<PrincipalExchangeCashflowRangeItem> receivePEArray = irSwapPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, recNotional, valuationRange);
            Assert.AreEqual(recNotional.Count + 1, receivePEArray.Count);
        }

        [TestMethod]
        public void GetPrincipalExchangesCleanupNotional()
        {
            var valuationDate = new DateTime(2008, 02, 14);
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            payFixed.GeneratePrincipalExchanges = true;
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            receiveFloat.GeneratePrincipalExchanges = true;
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<DateTimeDoubleRangeItem> payNotional = GetCleanupAmortNotional(payFixed, 6, 10);
            List<DateTimeDoubleRangeItem> recNotional = GetCleanupAmortNotional(receiveFloat, 6, 10);
            List<DetailedCashflowRangeItem> payCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, 
                                                                                                                      payNotional, valuationRange);
            Assert.AreEqual(payCashflowsArray.Count, 10);
            List<DetailedCashflowRangeItem> receiveCashflowsArray = irSwapPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, 
                                                                                                                          recNotional, valuationRange);
            Assert.AreEqual(receiveCashflowsArray.Count, 10);
            List<PrincipalExchangeCashflowRangeItem> payPEArray = irSwapPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, payNotional, valuationRange);
            Assert.AreEqual(payNotional.Count + 1, payPEArray.Count);
            List<PrincipalExchangeCashflowRangeItem> receivePEArray = irSwapPricer.GetPrincipalExchanges(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, recNotional, valuationRange);
            Assert.AreEqual(recNotional.Count + 1, receivePEArray.Count);
        }

        [TestMethod]
        public void GetDetailedCashflowsChangeTwoFloatingCashflowsToFixed()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            List<InputCashflowRangeItem> payDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
//            Debug.WriteLine("ORIGINAL (unchanged):");
//            Debug.WriteLine("Pay cashflows:");
//            Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            List<InputCashflowRangeItem> receiveDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
//            Debug.WriteLine("Receive cashflows:");
//            Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
            foreach (int cashflowNumber in new[] {0,1})
            {
                Assert.AreSame("Float", receiveDetailedCashflowsList[cashflowNumber].CouponType);
                receiveDetailedCashflowsList[cashflowNumber].CouponType = "Fixed";
                receiveDetailedCashflowsList[cashflowNumber].Rate += 0.001;//10bp inc
            }
            Debug.WriteLine("MODIFIED):");
            Debug.WriteLine("Pay cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(payDetailedCashflowsList)));
            Debug.WriteLine("Receive cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(receiveDetailedCashflowsList)));
            ValuationResultRange modifiedSwapResultRange = InterestRateSwapPricer.GetPriceFromCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange, payDetailedCashflowsList, receiveDetailedCashflowsList);
            // PV should be up since the rate on rec. cashflows been increased.
            // pay stream stays unchanged (hasn't been modified).
            //
            AssertExtension.Greater(modifiedSwapResultRange.PresentValue, resultRange.PresentValue);
            Assert.AreEqual(modifiedSwapResultRange.PayLegFutureValue, resultRange.PayLegFutureValue);
            Assert.AreEqual(modifiedSwapResultRange.PayLegPresentValue, resultRange.PayLegPresentValue);
        }

        [TestMethod]
        public void GetDetailedCashflowsChangeRollDatesOfFirstTwoCashflows()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            List<InputCashflowRangeItem> payDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
//            Debug.WriteLine("ORIGINAL (unchanged):");
//            Debug.WriteLine("Pay cashflows:");
//            Debug.WriteLine(ParameterFormatter.FormatObject(payCashflowsArray));
            List<InputCashflowRangeItem> receiveDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
//            Debug.WriteLine("Receive cashflows:");
//            Debug.WriteLine(ParameterFormatter.FormatObject(receiveCashflowsArray));
            foreach (int cashflowNumber in new[] {0,1})
            {
                Assert.AreEqual(receiveDetailedCashflowsList[cashflowNumber].EndDate, receiveDetailedCashflowsList[cashflowNumber + 1].StartDate);
                receiveDetailedCashflowsList[cashflowNumber].EndDate = receiveDetailedCashflowsList[cashflowNumber].EndDate.AddDays(10);
            }
            Debug.WriteLine("MODIFIED (unfixed)):");
            Debug.WriteLine("Pay cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(payDetailedCashflowsList)));
            Debug.WriteLine("Receive cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(receiveDetailedCashflowsList)));
            ValuationResultRange modifiedSwapResultRange = InterestRateSwapPricer.GetPriceFromCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange, payDetailedCashflowsList, receiveDetailedCashflowsList);
            Assert.AreNotEqual(modifiedSwapResultRange.FutureValue, resultRange.FutureValue);
            Assert.AreNotEqual(modifiedSwapResultRange.PresentValue, resultRange.PresentValue);

        }

        [TestMethod]
        public void GetDetailedCashflowsChangeRollDatesOfLastTwoCashflows()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            ValuationResultRange resultRange = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange);
            List<InputCashflowRangeItem> payDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            List<InputCashflowRangeItem> receiveDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            foreach (int cashflowNumber in new[] { 7, 8 })
            {
                Assert.AreEqual(receiveDetailedCashflowsList[cashflowNumber].EndDate, receiveDetailedCashflowsList[cashflowNumber + 1].StartDate);
                receiveDetailedCashflowsList[cashflowNumber].EndDate = receiveDetailedCashflowsList[cashflowNumber].EndDate.AddDays(10);
            }
            Debug.WriteLine("MODIFIED (unfixed)):");
            Debug.WriteLine("Pay cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(payDetailedCashflowsList)));
            Debug.WriteLine("Receive cashflows:");
            Debug.WriteLine(ParameterFormatter.FormatObject(ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(receiveDetailedCashflowsList)));
            ValuationResultRange modifiedSwapResultRange = InterestRateSwapPricer.GetPriceFromCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, receiveFloat, valuationRange, payDetailedCashflowsList, receiveDetailedCashflowsList);
            Assert.AreNotEqual(modifiedSwapResultRange.FutureValue, resultRange.FutureValue);
            Assert.AreNotEqual(modifiedSwapResultRange.PresentValue, resultRange.PresentValue);
        }

        [TestMethod]
        public void TestGetPriceFromCashflows()
        {
            DateTime valuationDate = DateTime.Today;
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            SwapLegParametersRange_Old payLegParametersRange = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveLegParametersRange = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRange(valuationDate);
            List<InputCashflowRangeItem> payLegDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, valuationRange);
            List<InputCashflowRangeItem> receiveLegDetailedCashflowsList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveLegParametersRange, valuationRange);
            ValuationResultRange valuationResultRangeNotFromCFs = InterestRateSwapPricer.GetPriceOld(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, receiveLegParametersRange,
                                                                                            valuationRange);
            Debug.Print("FROM PARAMS");
            Debug.Print(ValuationResultRangeToString(valuationResultRangeNotFromCFs));
            ValuationResultRange valuationResultRange = InterestRateSwapPricer.GetPriceFromCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payLegParametersRange, receiveLegParametersRange, 
                                                                                                    valuationRange, 
                                                                                                    payLegDetailedCashflowsList, receiveLegDetailedCashflowsList);
            Assert.IsTrue(valuationResultRange.PresentValue != 0);
            Assert.IsTrue(valuationResultRange.FutureValue != 0);
            Assert.IsTrue(valuationResultRange.PayLegPresentValue != 0);
            Assert.IsTrue(valuationResultRange.ReceiveLegPresentValue != 0);
            Assert.IsTrue(valuationResultRange.PayLegFutureValue != 0);
            Assert.IsTrue(valuationResultRange.ReceiveLegFutureValue != 0);
            Assert.IsTrue(valuationResultRange.PresentValue == valuationResultRangeNotFromCFs.PresentValue);
            Assert.IsTrue(valuationResultRange.FutureValue == valuationResultRangeNotFromCFs.FutureValue);
            Assert.IsTrue(valuationResultRange.PayLegPresentValue == valuationResultRangeNotFromCFs.PayLegPresentValue);
            Assert.IsTrue(valuationResultRange.ReceiveLegPresentValue == valuationResultRangeNotFromCFs.ReceiveLegPresentValue);
            Assert.IsTrue(valuationResultRange.PayLegFutureValue == valuationResultRangeNotFromCFs.PayLegFutureValue);
            Assert.IsTrue(valuationResultRange.ReceiveLegFutureValue == valuationResultRangeNotFromCFs.ReceiveLegFutureValue);
            Debug.Print("FROM CF's");
            Debug.Print(ValuationResultRangeToString(valuationResultRange));
        }
  
        private static ValuationRange CreateValuationRangeForNAB(DateTime valuationDate)
        {
            var result = new ValuationRange {ValuationDate = valuationDate, BaseParty = _NAB};
            return result;
        }

        [TestMethod]
        public void CreateValuationTradeIdSupplied()
        {
            string guid = Guid.NewGuid().ToString();
            ValuationReport valuationReport = CreateValuation(guid, false, false);
            Assert.AreEqual(guid, valuationReport.header.messageId.Value);
            Assert.AreEqual(valuationReport.header.messageId.Value, ((Trade)(valuationReport.tradeValuationItem[0].Items[0])).Item.id);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
        }    
        
        [TestMethod]
        public void CreateValuationForecastCurveIsNull()
        {
            string guid = Guid.NewGuid().ToString();
            ValuationReport valuationReport = CreateValuation(guid, true, false);
            Assert.AreEqual(guid, valuationReport.header.messageId.Value);
            Assert.AreEqual(valuationReport.header.messageId.Value, ((Trade)(valuationReport.tradeValuationItem[0].Items[0])).Item.id);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
        }

        [TestMethod]
        public void CreateValuationForecastCurveIsNone()
        {
            string guid = Guid.NewGuid().ToString();
            ValuationReport valuationReport = CreateValuation(guid, false, true);
            Assert.AreEqual(guid, valuationReport.header.messageId.Value);
            Assert.AreEqual(valuationReport.header.messageId.Value, ((Trade)(valuationReport.tradeValuationItem[0].Items[0])).Item.id);
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
        }

        [TestMethod]
        public void CreateValuationTradeIdIsNull()
        {
            ValuationReport valuationReport = CreateValuation(null, false, false);
            Assert.IsNotNull(valuationReport.header.messageId.Value);
            Assert.AreEqual(valuationReport.header.messageId.Value, ((Trade)(valuationReport.tradeValuationItem[0].Items[0])).Item.id);          
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));

        }      
        
        [TestMethod]
        public void CreateValuationTradeIdIsEmpty()
        {
            ValuationReport valuationReport = CreateValuation("", false, false);
            Assert.IsNotNull(valuationReport.header.messageId.Value);
            Assert.AreEqual(valuationReport.header.messageId.Value, ((Trade)(valuationReport.tradeValuationItem[0].Items[0])).Item.id);           
            Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));

        }

        private ValuationReport CreateValuation(string idForTradeRange, bool forecastCurveIsEmpty, bool forecastCurveIsNone)
        {
            DateTime valuationDate = DateTime.Today;
            var irSwapPricer = new InterestRateSwapPricer();
            string discountCurveID = BuildAndCacheRateCurve(valuationDate);
            string projectionCurveID = discountCurveID;
            if (forecastCurveIsEmpty)
            {
                projectionCurveID = null;
            }
            if (forecastCurveIsNone)
            {
                projectionCurveID = "nOne";
            }
            SwapLegParametersRange_Old payFixed = CreateFixedAUD6MSwapLegParametersRange(_NAB, CounterParty, valuationDate, 0.065m, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID);
            SwapLegParametersRange_Old receiveFloat = CreateFloatingAUD6MSwapLegParametersRange(CounterParty, _NAB, valuationDate, 0, "ACT/365.FIXED", "AUSY", "FOLLOWING", "AUSY", "NONE", discountCurveID, projectionCurveID);
            ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
            List<InputCashflowRangeItem> payCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, payFixed, valuationRange);
            payCFRangeItemList[0].CouponType = "fixed";// that should test case insensitive nature of coupons
            payCFRangeItemList[1].CouponType = "Fixed";//
            List<InputCashflowRangeItem> receiveCFRangeItemList = InterestRateSwapPricer.GetDetailedCashflowsTestOnly(Engine.Logger, Engine.Cache, Engine.NameSpace, receiveFloat, valuationRange);
            receiveCFRangeItemList[0].CouponType = "float";// that should test case insensitive nature of coupons
            receiveCFRangeItemList[1].CouponType = "Float";//
            var tradeRange = new TradeRange {Id = idForTradeRange, TradeDate = valuationDate};
            var leg1PrincipleExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg2PrincipleExchangeCashflowList = new List<InputPrincipalExchangeCashflowRangeItem>();
            var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            var leg2BulletPaymentList = new List<AdditionalPaymentRangeItem>();
            List<PartyIdRangeItem> partyList = GetPartyList("NAB", "book", "MCHammer", "counterparty");
            List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = GetOtherPartyPaymentList("counterparty", "cost center");
            IRateCurve curve1 = null;//TODO fix these curves...ALEX
            IRateCurve curve2 = null;
            //  Get price and swap representation using non-vanilla PRICE function.
            //
            string valuatonId = irSwapPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, CreateValuationSetList2(12345.67, -0.321), valuationRange, tradeRange,
                                                             payFixed, curve1, receiveFloat, curve2,
                                                             payCFRangeItemList, receiveCFRangeItemList,
                                                             leg1PrincipleExchangeCashflowList, leg2PrincipleExchangeCashflowList,
                                                             leg1BulletPaymentList, leg2BulletPaymentList, partyList,
                                                             otherPartyPaymentRangeItems);

            return Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + valuatonId);
        }


        public static List<OtherPartyPaymentRangeItem> GetOtherPartyPaymentList(string payer, string receiver)
        {
            var result = new List<OtherPartyPaymentRangeItem>();
            var item1 = new OtherPartyPaymentRangeItem
                            {
                                PaymentDate = DateTime.Today,
                                PaymentType = "Value Add SOFT",
                                Amount = 5000,
                                Payer = payer,
                                Receiver = receiver
                            };
            result.Add(item1);
            var item2 = new OtherPartyPaymentRangeItem
                            {
                                PaymentDate = DateTime.Today,
                                PaymentType = "Value Add HARD",
                                Amount = 15000,
                                Payer = payer,
                                Receiver = receiver
                            };
            result.Add(item2);
            return result;
        }

        public static List<FeePaymentRangeItem> GetFeeList(string payer, string receiver)
        {
            var result = new List<FeePaymentRangeItem>();
            var item1 = new FeePaymentRangeItem
                            {
                                PaymentDate = DateTime.Today,
                                Amount = 5000,
                                Payer = payer,
                                Receiver = receiver
                            };
            result.Add(item1);
            var item2 = new FeePaymentRangeItem
                            {
                                PaymentDate = DateTime.Today,
                                Amount = 15000,
                                Payer = payer,
                                Receiver = receiver
                            };
            result.Add(item2);
            return result; 
        }

        public static List<PartyIdRangeItem>  GetPartyList(string name1, string role1, string name2, string role2)
        {
            var partyIdList = new List<PartyIdRangeItem>();
            var item1 = new PartyIdRangeItem {PartyId = name1, IdOrRole = role1};
            partyIdList.Add(item1);
            var item2 = new PartyIdRangeItem {PartyId = name2, IdOrRole = role2};
            partyIdList.Add(item2);
            return partyIdList;
        }

        private static List<StringObjectRangeItem> CreateValuationSetList2(double npv, double dv01)
        {
            var list = new List<StringObjectRangeItem>();
            var npvItem = new StringObjectRangeItem { StringValue = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.NPV), ObjectValue = npv };
            list.Add(npvItem);
            var dv01Item = new StringObjectRangeItem { StringValue = AssetMeasureScheme.GetEnumString(AssetMeasureEnum.DE_R), ObjectValue = dv01 };
            list.Add(dv01Item);
            return list;
        }

        private static string  ValuationResultRangeToString(ValuationResultRange resultRange)
        {
            string result = $"PV: {resultRange.PresentValue}\n\r" + $"FV: {resultRange.FutureValue}\n\r" +
                            $"Pay PV: {resultRange.PayLegPresentValue}\n\r" +
                            $"Rec PV: {resultRange.ReceiveLegPresentValue}\n\r" +
                            $"Pay FV: {resultRange.PayLegFutureValue}\n\r" +
                            $"Rec FV: {resultRange.ReceiveLegFutureValue}\n\r";

            return result;
        }
    }
}
