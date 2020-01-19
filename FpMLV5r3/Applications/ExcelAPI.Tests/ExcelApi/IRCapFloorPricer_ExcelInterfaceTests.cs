/*
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

#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Highlander.Constants;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Options;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.TestHelpers.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Serialisation;
using Highlander.ValuationEngine.V5r3.Generators;
using Highlander.ValuationEngine.V5r3.Pricers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Excel.Tests.V5r3.ExcelApi
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

                ValuationRange valuationRange = ExcelAPITests.CreateValuationRange(valuationDate);
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
                ValuationRange valuationRange = ExcelAPITests.CreateValuationRangeForNAB(valuationDate);
                List<DateTimeDoubleRangeItem> notional = GetAmortNotional(capLeg, 3);
                List<DetailedCashflowRangeItem> cashflowList =
                    irCapFloorPricer.GetDetailedCashflowsWithNotionalSchedule(Engine.Logger, Engine.Cache, Engine.NameSpace, null, null, capLeg, notional, valuationRange);
                TradeRange tradeRange = null;
                var leg1BulletPaymentList = new List<AdditionalPaymentRangeItem>();
                List<FeePaymentRangeItem> feePaymentRangeItems = ExcelAPITests.GetFeeList("counterparty", "book");
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
                Debug.Print(ExcelAPITests.ValuationResultRangeToString(nonVanillaPriceImpl.First));
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
                var tradeRange = new TradeRange {TradeDate = DateTime.Now, Id = "1234"};
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
                var valuationReport = Engine.Cache.LoadObject<ValuationReport>(id);              
                Debug.Print(XmlSerializerHelper.SerializeToString((object) valuationReport));
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
        public void TestGetCapPremiumAUD6M100M5YExpiry05Vol20Pct()
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
