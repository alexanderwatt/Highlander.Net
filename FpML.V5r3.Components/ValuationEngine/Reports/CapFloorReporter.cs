/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System.Diagnostics;
using System.Collections.Generic;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Reports;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Pricers;

#endregion

namespace Orion.ValuationEngine.Reports
{
    public class CapFloorReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is CapFloorPricer interestRateSwap)
            {
                Debug.Print("Number of legs : {0}", interestRateSwap.GetChildren().Count);
                Debug.Print("Receive leg {0} coupons", interestRateSwap.CapFloorLeg.Coupons.Count);
                foreach (var receiveRateCoupon in interestRateSwap.CapFloorLeg.Coupons)
                {
                    Debug.Print("Coupon: coupon type: {0},payment date: {1}, notional amount : {2}, fixed rate : {3}, payment amount: {4}", 
                        receiveRateCoupon.CashflowType, receiveRateCoupon.PaymentDate,
                                receiveRateCoupon.Notional, receiveRateCoupon.Rate, receiveRateCoupon.PaymentAmount);
                }
            }
            return null;
        }

        public override object[,] DoXLReport(InstrumentControllerBase instrument)
        {
            var capFloor = instrument as CapFloorPricer;
            object[,] result = null;
            if (capFloor != null)
            {
                var capLegExchangesCount = 0;
                if (capFloor.CapFloorLeg.PriceablePrincipalExchanges != null)
                {
                    capLegExchangesCount = capFloor.CapFloorLeg.PriceablePrincipalExchanges.Count;
                }
                var rows = capFloor.CapFloorLeg.Coupons.Count + 1 + capLegExchangesCount;
                result = new object[rows, 15];
                var index = 1;
                //Headings First:
                result[0, 0] = "Leg";
                result[0, 1] = "Coupon";
                result[0, 2] = "PriceableCouponType";
                //result[0, 3] = "PaymentDate";
                result[0, 3] = "NotionalAmount";
                result[0, 4] = "Rate";
                result[0, 5] = "ForecastAmount";
                result[0, 6] = "Currency";
                result[0, 7] = "AccrualStartDate";
                result[0, 8] = "AccrualEndDate";
                result[0, 9] = "PaymentDate";
                result[0, 10] = "PaymentDiscountFactor";
                result[0, 11] = "NPV";
                result[0, 12] = "CouponYearFraction";
                result[0, 13] = "IsDiscounted";
                result[0, 14] = "IsRealised";
                foreach (var capFloorCoupon in capFloor.CapFloorLeg.Coupons)
                {
                    result[index, 0] = "ReceiveLeg";
                    result[index, 1] = "Coupon_" + index;
                    result[index, 2] = capFloorCoupon.PriceableCouponType.ToString();
                    //result[index, 3] = receiveRateCoupon.PaymentDate;
                    result[index, 3] = capFloorCoupon.NotionalAmount.amount;
                    result[index, 4] = capFloorCoupon.Rate;
                    result[index, 5] = capFloorCoupon.ForecastAmount.amount;
                    result[index, 6] = capFloorCoupon.NotionalAmount.currency.Value;
                    result[index, 7] = capFloorCoupon.AccrualStartDate;
                    result[index, 8] = capFloorCoupon.AccrualEndDate;
                    result[index, 9] = capFloorCoupon.PaymentDate;
                    result[index, 10] = capFloorCoupon.PaymentDiscountFactor;
                    result[index, 11] = capFloorCoupon.NPV.amount;
                    result[index, 12] = capFloorCoupon.CouponYearFraction;
                    result[index, 13] = capFloorCoupon.IsDiscounted;
                    result[index, 14] = capFloorCoupon.IsRealised;
                    index++;
                }
                if (capFloor.CapFloorLeg.Exchanges != null)
                {
                    var thirdIndex = 1;
                    foreach (var principal in capFloor.CapFloorLeg.Exchanges)
                    {
                        result[index, 0] = "ReceiveLeg";
                        result[index, 1] = "Principal_" + thirdIndex;
                        result[index, 2] = "PrincipalExchange";
                        //result[index, 3] = principal.RiskMaturityDate;
                        result[index, 3] = principal.PaymentAmount.amount;
                        result[index, 4] = 0.0m;
                        result[index, 5] = principal.PaymentAmount.amount;
                        result[index, 6] = principal.PaymentAmount.currency.Value;
                        result[index, 7] = principal.PaymentDate;
                        result[index, 8] = principal.PaymentDate;
                        result[index, 9] = principal.PaymentDate;
                        result[index, 10] = principal.PaymentDiscountFactor;
                        var npv = 0.0m;
                        if (principal.NPV != null)
                        {
                            npv = principal.NPV.amount;
                        }
                        result[index, 11] = npv;
                        result[index, 12] = principal.YearFractionToCashFlowPayment;
                        result[index, 13] = false;
                        result[index, 14] = principal.IsRealised;
                        index++;
                        thirdIndex++;
                    }
                }
            }
            return result;
        }

        public override List<object[]> DoExpectedCashflowReport(InstrumentControllerBase instrument)
        {
            var capFloor = instrument as CapFloorPricer;
            var result = new List<object[]>();
            if (capFloor != null)
            {
                foreach (var leg in capFloor.Legs)
                {
                    var expectedCashFlows = CashflowReportHelper.DoCashflowReport(instrument.Id, leg);
                    result.AddRange(expectedCashFlows);
                }
                if (capFloor.AdditionalPayments != null)
                {
                    foreach (var payment in capFloor.AdditionalPayments)
                    {
                        var pay = CashflowReportHelper.DoCashflowReport(instrument.Id, payment);
                        pay[0] = instrument.Id;
                        result.Add(pay);
                    }
                }
            }
            return result;
        }

        public static object[,] DoXLReport(CapFloorPricer capFloor, bool receiveLeg)
        {
            if (capFloor != null)
            {
                var result = new object[capFloor.CapFloorLeg.Coupons.Count, 6];
                if (receiveLeg)
                {
                    var index = 0;
                    foreach (var receiveRateCoupon in capFloor.CapFloorLeg.Coupons)
                    {
                        result[index, 0] = "ReceiveLeg_Coupon_" + index;
                        result[index, 1] = receiveRateCoupon.PriceableCouponType.ToString();
                        result[index, 2] = receiveRateCoupon.PaymentDate;
                        result[index, 3] = receiveRateCoupon.NotionalAmount.amount;
                        result[index, 4] = receiveRateCoupon.Rate;
                        result[index, 5] = receiveRateCoupon.PaymentAmount.amount;
                        index++;
                    }
                }         
            }
            return null;
        }

        /// <summary>
        /// Returns the report for that particular product type.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            if (product is CapFloor capFloor)
            {
                var coupons1 = capFloor.capFloorStream.cashflows.paymentCalculationPeriod.Length;
                var result = new object[coupons1, 9];
                var index = 0;
                foreach (var coupon1 in capFloor.capFloorStream.cashflows.paymentCalculationPeriod)
                {
                    var calcPeriod1 = coupon1.Items[0] as CalculationPeriod;
                    result[index, 0] = "CapFloor_Coupon_" + index;
                    result[index, 1] = coupon1.adjustedPaymentDate;
                    if (calcPeriod1 != null)
                    {
                        result[index, 2] = calcPeriod1.adjustedStartDate;
                        result[index, 3] = calcPeriod1.adjustedEndDate;
                        result[index, 4] = calcPeriod1.dayCountYearFraction;
                        result[index, 5] = calcPeriod1.calculationPeriodNumberOfDays;
                        result[index, 6] = calcPeriod1.Item ?? 0.0m;
                        result[index, 7] = 0.0m;
                        result[index, 8] = 0.0m;
                        if (calcPeriod1.Item1 != null) //TODO Set the strike!
                        {
                            var frd = calcPeriod1.Item1 as FloatingRateDefinition;
                            if (frd?.capRate != null)
                            {
                                result[index, 7] = frd.capRate[0].strikeRate;
                            }
                            if (frd?.floorRate != null)
                            {
                                result[index, 8] = frd.floorRate[0].strikeRate;
                            }
                        }
                    }
                    index++;
                }
                return result;
            }
            return null;
        }

        public static object CouponDataReport(CapFloor capFloor)
        {
            if (capFloor != null)
            {
                var coupons1 = capFloor.capFloorStream.cashflows.paymentCalculationPeriod.Length;
                var result = new object[coupons1, 4];            
                var index = 0;
                foreach (var coupon1 in capFloor.capFloorStream.cashflows.paymentCalculationPeriod)
                {
                    result[index, 0] = "CapFloor_Coupon_" + index;
                    result[index, 1] = coupon1.adjustedPaymentDate;
                    result[index, 2] = coupon1.discountFactor;
                    result[index, 3] = coupon1.forecastPaymentAmount.amount;
                    result[index, 4] = coupon1.forecastPaymentAmount.currency.Value;
                    index++;
                }
                return result;
            }
            return null;
        }
    }
}