#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Helpers;
using Orion.Analytics.Options;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
//using Orion.CurveEngine.Tests.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ValuationEngine.Generators;
//using Orion.ModelFramework;
//using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ValuationEngine.Pricers;
using Orion.TestHelpers;
//using Orion.CurveEngine.Tests;

#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    public partial class ExcelAPITests
    {
        private static CapFloorLegParametersRange_Old GetCapFloorInputParameters(DateTime effectiveDate, DateTime firstRollDate, DateTime matDate,
                                                                             CapFloorType capFloorType, string discountingType, string discountCurve, string forwardCurve)
        {
            var result = new CapFloorLegParametersRange_Old
                {
                    EffectiveDate = effectiveDate,
                    FirstRegularPeriodStartDate = firstRollDate,
                    MaturityDate = matDate,
                    Payer = "NAB",
                    Receiver = "CounterParty",
                    RollConvention = "14",
                    InitialStubType = StubPeriodTypeEnum.ShortInitial.ToString(),
                    FinalStubType = StubPeriodTypeEnum.ShortFinal.ToString(),
                    NotionalAmount = 50000000,
                    Currency = "AUD",
                    PaymentFrequency = "1Y",
                    DayCount = "30E/360",
                    PaymentCalendar = "Sydney",
                    PaymentBusinessDayAdjustments = "FOLLOWING",
                    FixingCalendar = "London",
                    FixingBusinessDayAdjustments = "FOLLOWING",
                    DiscountCurve = discountCurve,
                    ForecastCurve = forwardCurve,
                    DiscountingType = discountingType,
                    CapOrFloor = capFloorType,
                    StrikeRate = 0.080m
                };
             return result;
        }

        [TestMethod]
        public void GetDetailedCapFloorCashflowsVanilla()
        {
            DateTime valuationDate = DateTime.Today;
            var irCapFloorPricer = new CapFloorPricer();
            var curveId = BuildAndCacheRateCurve(valuationDate);
            foreach (CapFloorType legType in new[] { CapFloorType.Cap, CapFloorType.Floor })
            {
                Debug.Print("LegType: {0}", legType);
                CapFloorLegParametersRange_Old capLeg = GetCapFloorInputParameters(valuationDate, valuationDate.AddMonths(6), valuationDate.AddYears(5),
                                                                               legType, "Standard", curveId, curveId);

                ValuationRange valuationRange = CreateValuationRange(valuationDate);
                List<DateTimeDoubleRangeItem> notional = GetAmortNotional(capLeg, 3);
                List<DetailedCashflowRangeItem> cashflowList =
                    irCapFloorPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, capLeg, notional, valuationRange);
                object[,] arrayOfCashflows = ObjectToArrayOfPropertiesConverter.ConvertListToHorizontalArrayRange(cashflowList);
                Debug.WriteLine("Cashflows:");
                Debug.WriteLine(ParameterFormatter.FormatObject(arrayOfCashflows));
            }
        }

        [TestMethod]
        public void GetPrice2Vanilla()
        {
            DateTime valuationDate = DateTime.Today;
            var irCapFloorPricer = new CapFloorPricer();
            var curveId = BuildAndCacheRateCurve(valuationDate);
            foreach (CapFloorType capFloorType in new[] { CapFloorType.Cap, CapFloorType.Floor })
            {
                Debug.Print("Type: {0}", capFloorType);
                CapFloorLegParametersRange_Old capLeg = GetCapFloorInputParameters(valuationDate, valuationDate.AddMonths(6), valuationDate.AddYears(5),
                                                                               capFloorType, "Standard", curveId, curveId);
                ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
                List<DateTimeDoubleRangeItem> notional = GetAmortNotional(capLeg, 3);
                List<DetailedCashflowRangeItem> cashflowList =
                    irCapFloorPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, capLeg, notional, valuationRange);
                TradeRange tradeRange = null;
                var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
                List<FeePaymentRangeItem> feePaymentRangeItems = GetFeeList("counterparty", "book");
                //  Get price and swap representation using non-vanilla PRICE function.
                //
                var newCashflowList = cashflowList.Cast<InputCashflowRangeItem>().ToList();
                Pair<ValuationResultRange, CapFloor> nonVanillaPriceImpl = CapFloorPricer.GetPriceAndGeneratedFpML(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, 
                                                                                                                    valuationRange, tradeRange, capLeg,
                                                                                                                     newCashflowList, null, leg1BulletPaymentList, feePaymentRangeItems);
                // NO PExs
                //
                CollectionAssertExtension.IsEmpty(nonVanillaPriceImpl.Second.capFloorStream.cashflows.principalExchange);
                // No payments
                //
                CollectionAssertExtension.IsEmpty(nonVanillaPriceImpl.Second.additionalPayment);
                Debug.Print(ValuationResultRangeToString(nonVanillaPriceImpl.First));
            }
        }

        [TestMethod]
        public void CreateValuationVanilla()
        {
            DateTime valuationDate = DateTime.Today;
            var irCapFloorPricer = new CapFloorPricer();
            var curveId = BuildAndCacheRateCurve(valuationDate);
            foreach (CapFloorType capFloorType in new[] { CapFloorType.Cap, CapFloorType.Floor })
            {
                Debug.Print("Type: {0}", capFloorType);
                CapFloorLegParametersRange_Old capLeg = GetCapFloorInputParameters(valuationDate, valuationDate.AddMonths(6), valuationDate.AddYears(5),
                                                                               capFloorType, "Standard", curveId, curveId);
                ValuationRange valuationRange = CreateValuationRangeForNAB(valuationDate);
                List<DateTimeDoubleRangeItem> notional = GetAmortNotional(capLeg, 3);
                List<DetailedCashflowRangeItem> cashflowList =
                    irCapFloorPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, capLeg, notional, valuationRange);
                switch (capFloorType)
                {
                    case CapFloorType.Cap:
                        cashflowList[0].CouponType = "cap";// that should test case insensitive nature of the coupons
                        cashflowList[1].CouponType = "Cap";//

                        break;
                    case CapFloorType.Floor:
                        cashflowList[0].CouponType = "floor";// that should test case insensitive nature of the coupons
                        cashflowList[1].CouponType = "Floor";//

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var tradeRange = new TradeRange {TradeDate = DateTime.Now};
                var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
                var newCashflowList = cashflowList.Cast<InputCashflowRangeItem>().ToList();
                //  Get price and swap representation using non-vanilla PRICE function.
                //
                List<StringObjectRangeItem> valuationSetList = CreateValuationSetList2(1111, 12);
                List<PartyIdRangeItem> partyList = GetPartyList("NAB", "book", "MCHammer", "counterparty");
                List<OtherPartyPaymentRangeItem> otherPartyPaymentRangeItems = GetOtherPartyPaymentList("counterparty", "cost center");
                List<FeePaymentRangeItem> feePaymentRangeItems = GetFeeList("counterparty", "book");
                string id = irCapFloorPricer.CreateValuation(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, 
                                                             valuationSetList, valuationRange, tradeRange,
                                                             capLeg, newCashflowList, null, leg1BulletPaymentList,
                                                             partyList, otherPartyPaymentRangeItems, feePaymentRangeItems);
                var valuationReport = Engine.Cache.LoadObject<ValuationReport>(Engine.NameSpace + "." + id);              
                Debug.Print(XmlSerializerHelper.SerializeToString(valuationReport));
            }
        }

        static List<DateTimeDoubleRangeItem> GetAmortNotional(CapFloorLegParametersRange_Old leg, int rollEveryNMonth)
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

        [TestMethod]
        public void TestGetCapPremiumAUD_6M100M_5YExpiry05Vol20Pct()
        {

            var valuationDate = new DateTime(1994, 12, 14); 
            var curveId = BuildAndCacheRateCurve(valuationDate);
            CapFloorLegParametersRange_Old capLeg = GetCapFloorInputParameters(valuationDate, valuationDate.AddMonths(6), valuationDate.AddYears(5),
                                                                           CapFloorType.Cap, "Standard", curveId, curveId);
            InterestRateStream floatStream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(capLeg);
            floatStream.cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(floatStream, FixingCalendar, PaymentCalendar);
            double sumOfCapletPremiums = 0;
            var rateCurve = (RateCurve)Engine.GetCurve(curveId, false);
            foreach (PaymentCalculationPeriod paymentCalculationPeriod in floatStream.cashflows.paymentCalculationPeriod)
            {
                DateTime startDate = PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(paymentCalculationPeriod);
                DateTime endDate = PaymentCalculationPeriodHelper.GetCalculationPeriodEndDate(paymentCalculationPeriod);            
                double accrualFactor = (endDate - startDate).TotalDays / 365.0;
                var discountFactor = (double)paymentCalculationPeriod.discountFactor;
                var rate = (double)PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod);
                double rate2 = rateCurve.GetForwardRate(startDate, endDate, "ACT/365.FIXED");
                double diff = rate - rate2;
                Debug.Print("Diff in forward rate: {0}", diff);
                var strikeRate = (double)capLeg.StrikeRate;//fixed - replace with a schedule
                const double volatility = 0.2; //fixed - replace with a schedule
                double timeToExpiry = (startDate - valuationDate).TotalDays / 365.0;
                double optionValue = accrualFactor * BlackModel.GetSwaptionValue(rate, strikeRate, volatility, timeToExpiry) * discountFactor;
                Debug.Print("Expiry:\t{0},\tPremium:\t{1}'", timeToExpiry, optionValue);
                sumOfCapletPremiums += optionValue;
            }
            Debug.Print("Premium : '{0}'", sumOfCapletPremiums * (double)capLeg.NotionalAmount);
        }
    }
}
