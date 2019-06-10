/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

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
    public class InterestRateSwapReporter : ReporterBase
    {
        public override object DoReport(InstrumentControllerBase priceable)
        {
            if (priceable is InterestRateSwapPricer interestRateSwap)
            {
                Debug.Print("Number of legs : {0}", interestRateSwap.GetChildren().Count);

                Debug.Print("Receive leg {0} coupons", interestRateSwap.ReceiveLeg.Coupons.Count);
                foreach (var receiveRateCoupon in interestRateSwap.ReceiveLeg.Coupons)
                {
                    Debug.Print("Coupon: coupon type: {0},payment date: {1}, notional amount : {2}, fixed rate : {3}, payment amount: {4}", 
                        receiveRateCoupon.CashflowType, receiveRateCoupon.PaymentDate,
                                receiveRateCoupon.Notional, receiveRateCoupon.Rate, receiveRateCoupon.PaymentAmount);
                }

                Debug.Print("Pay leg {0} coupons", interestRateSwap.PayLeg.Coupons.Count);
                foreach (var payRateCoupon in interestRateSwap.PayLeg.Coupons)
                {
                    Debug.Print("Coupon: coupon type: {0},payment date: {1}, notional amount : {2}, fixed rate : {3}, payment amount: {4}",
                        payRateCoupon.CashflowType, payRateCoupon.PaymentDate,
                                payRateCoupon.Notional, payRateCoupon.Rate, payRateCoupon.PaymentAmount);
                }
            }
            return null;
        }

        public override object[,] DoXLReport(InstrumentControllerBase instrument)
        {
            var interestRateSwap = instrument as InterestRateSwapPricer;
            object[,] result = null;
            if (interestRateSwap!=null)
            {
                var receiveLegExchangesCount = 0;
                if (interestRateSwap.ReceiveLeg.PriceablePrincipalExchanges != null)
                {
                    receiveLegExchangesCount = interestRateSwap.ReceiveLeg.PriceablePrincipalExchanges.Count;
                }
                var payLegExchangesCount = 0;
                if (interestRateSwap.PayLeg.PriceablePrincipalExchanges != null)
                {
                    payLegExchangesCount = interestRateSwap.PayLeg.PriceablePrincipalExchanges.Count;
                }
                var rows = interestRateSwap.ReceiveLeg.Coupons.Count + interestRateSwap.PayLeg.Coupons.Count + 1 + receiveLegExchangesCount + payLegExchangesCount;
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
                foreach (var receiveRateCoupon in interestRateSwap.ReceiveLeg.Coupons)
                {
                    result[index, 0] = "ReceiveLeg";
                    result[index, 1] = "Coupon_" + index;
                    result[index, 2] = receiveRateCoupon.PriceableCouponType.ToString();
                    //result[index, 3] = receiveRateCoupon.PaymentDate;
                    result[index, 3] = receiveRateCoupon.NotionalAmount.amount;
                    result[index, 4] = receiveRateCoupon.Rate;
                    result[index, 5] = receiveRateCoupon.ForecastAmount.amount;
                    result[index, 6] = receiveRateCoupon.NotionalAmount.currency.Value;
                    result[index, 7] = receiveRateCoupon.AccrualStartDate;
                    result[index, 8] = receiveRateCoupon.AccrualEndDate;
                    result[index, 9] = receiveRateCoupon.PaymentDate;
                    result[index, 10] = receiveRateCoupon.PaymentDiscountFactor;
                    result[index, 11] = receiveRateCoupon.NPV.amount;
                    result[index, 12] = receiveRateCoupon.CouponYearFraction;
                    result[index, 13] = receiveRateCoupon.IsDiscounted;
                    result[index, 14] = receiveRateCoupon.IsRealised;
                    index++;
                }
                var secondIndex = 1;
                foreach (var payRateCoupon in interestRateSwap.PayLeg.Coupons)
                {
                    result[index, 0] = "PayLeg";
                    result[index, 1] = "Coupon_" + secondIndex;
                    result[index, 2] = payRateCoupon.PriceableCouponType.ToString();
                    //result[index, 3] = payRateCoupon.PaymentDate;
                    result[index, 3] = payRateCoupon.NotionalAmount.amount;
                    result[index, 4] = payRateCoupon.Rate;
                    result[index, 5] = payRateCoupon.ForecastAmount.amount;
                    result[index, 6] = payRateCoupon.NotionalAmount.currency.Value;
                    result[index, 7] = payRateCoupon.AccrualStartDate;
                    result[index, 8] = payRateCoupon.AccrualEndDate;
                    result[index, 9] = payRateCoupon.PaymentDate;
                    result[index, 10] = payRateCoupon.PaymentDiscountFactor;
                    result[index, 11] = payRateCoupon.NPV.amount;
                    result[index, 12] = payRateCoupon.CouponYearFraction;
                    result[index, 13] = payRateCoupon.IsDiscounted;
                    result[index, 14] = payRateCoupon.IsRealised;
                    index++;
                    secondIndex++;
                } 
                if (interestRateSwap.ReceiveLeg.Exchanges != null)
                {
                    var thirdIndex = 1;
                    foreach (var principal in interestRateSwap.ReceiveLeg.Exchanges)
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
                if (interestRateSwap.PayLeg.Exchanges != null)
                {
                    var fourthIndex = 1;
                    foreach (var principal in interestRateSwap.PayLeg.Exchanges)
                    {
                        result[index, 0] = "PayLeg";
                        result[index, 1] = "Principal_" + fourthIndex;
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
                        fourthIndex++;
                    }
                }
            }
            return result;
        }

        public override List<object[]> DoExpectedCashflowReport(InstrumentControllerBase instrument)
        {
             var interestRateSwap = instrument as InterestRateSwapPricer;
             var result = new List<object[]>();
             if (interestRateSwap != null)
             {
                 foreach (var leg in interestRateSwap.Legs)
                 {
                     var expectedCashFlows = CashflowReportHelper.DoCashflowReport(instrument.Id, leg);
                     result.AddRange(expectedCashFlows);
                 }
                 if (interestRateSwap.AdditionalPayments != null)
                 {
                     foreach (var payment in interestRateSwap.AdditionalPayments)
                     {
                         var pay = CashflowReportHelper.DoCashflowReport(instrument.Id, payment);
                         pay[0] = instrument.Id;
                         result.Add(pay);
                     }
                 }
             }
             return result;
        }

        public static object[,] DoXLReport(InterestRateSwapPricer interestRateSwap, bool receiveLeg)
        {
            if (interestRateSwap != null)
            {
                var result = new object[interestRateSwap.ReceiveLeg.Coupons.Count, 6];
                if (receiveLeg)
                {
                    var index = 0;
                    foreach (var receiveRateCoupon in interestRateSwap.ReceiveLeg.Coupons)
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
                else
                {
                    var index = 0;
                    var secondIndex = 0;
                    foreach (var payRateCoupon in interestRateSwap.PayLeg.Coupons)
                    {
                        result[index, 0] = "PayLeg_Coupon_" + secondIndex;
                        result[index, 1] = payRateCoupon.PriceableCouponType.ToString();
                        result[index, 2] = payRateCoupon.PaymentDate;
                        result[index, 3] = payRateCoupon.NotionalAmount.amount;
                        result[index, 4] = payRateCoupon.Rate;
                        result[index, 5] = payRateCoupon.PaymentAmount.amount;
                        index++;
                        secondIndex++;
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
            if (product is Swap interestRateSwap)
            {
                var coupons1 = interestRateSwap.swapStream[0].cashflows.paymentCalculationPeriod.Length;
                //var pe1 = 0;
                //if (interestRateSwap.swapStream[0].cashflows.principalExchange != null)
                //{
                //    pe1 = interestRateSwap.swapStream[0].cashflows.principalExchange.Length;
                //}
                var coupons2 = interestRateSwap.swapStream[1].cashflows.paymentCalculationPeriod.Length;
                //var pe2 = 0;
                //if (interestRateSwap.swapStream[1].cashflows.principalExchange != null)
                //{
                //    pe2 = interestRateSwap.swapStream[1].cashflows.principalExchange.Length;
                //}
                var result = new object[coupons1 + coupons2, 10];
                var index = 0;
                foreach (var coupon1 in interestRateSwap.swapStream[0].cashflows.paymentCalculationPeriod)
                {
                    result[index, 0] = interestRateSwap.swapStream[0].payerPartyReference.href;
                    result[index, 1] = interestRateSwap.swapStream[0].receiverPartyReference.href;
                    var calcPeriod1 = coupon1.Items[0] as CalculationPeriod;
                    result[index, 2] = "Leg1_Coupon_" + index;
                    result[index, 3] = coupon1.adjustedPaymentDate;
                    if (calcPeriod1 != null)
                    {
                        result[index, 4] = calcPeriod1.adjustedStartDate;
                        result[index, 5] = calcPeriod1.adjustedEndDate;
                        result[index, 6] = calcPeriod1.dayCountYearFraction;
                        result[index, 7] = calcPeriod1.calculationPeriodNumberOfDays;
                        result[index, 8] = calcPeriod1.Item ?? 0.0m;
                        if (calcPeriod1.forecastRateSpecified)
                        {
                            result[index, 9] = calcPeriod1.forecastRate;
                        }
                        else
                        {
                            result[index, 9] = 0.0m;
                        }
                    }
                    index++;
                }
                var secondIndex = 0;
                foreach (var coupon2 in interestRateSwap.swapStream[1].cashflows.paymentCalculationPeriod)
                {
                    result[index, 0] = interestRateSwap.swapStream[1].payerPartyReference.href;
                    result[index, 1] = interestRateSwap.swapStream[1].receiverPartyReference.href;
                    var calcPeriod2 = coupon2.Items[0] as CalculationPeriod;
                    result[index, 2] = "Leg2_Coupon_" + secondIndex;
                    result[index, 3] = coupon2.adjustedPaymentDate;
                    if (calcPeriod2 != null)
                    {
                        result[index, 4] = calcPeriod2.adjustedStartDate;
                        result[index, 5] = calcPeriod2.adjustedEndDate;
                        result[index, 6] = calcPeriod2.dayCountYearFraction;
                        result[index, 7] = calcPeriod2.calculationPeriodNumberOfDays;
                        result[index, 8] = calcPeriod2.Item ?? 0.0m;
                        if (calcPeriod2.forecastRateSpecified)
                        {
                            result[index, 9] = calcPeriod2.forecastRate;
                        }
                        else
                        {
                            result[index, 9] = 0.0m;
                        }
                    }
                    index++;
                    secondIndex++;
                }
                //var thirdIndex = 0;
                //foreach(var principalExchange in interestRateSwap.swapStream[0].cashflows.principalExchange)
                //{
                //    var exchange = principalExchange as PrincipalExchange;
                //    result[index, 0] = "Leg1_Exchange_" + thirdIndex;
                //    result[index, 1] = principalExchange.unadjustedPrincipalExchangeDate;
                //    //result[index, 2] = principalExchange.adjustedStartDate;
                //    //result[index, 3] = principalExchange.adjustedEndDate;
                //    result[index, 4] = principalExchange.dayCountYearFraction;
                //    result[index, 5] = principalExchange.calculationPeriodNumberOfDays;
                //    result[index, 6] = principalExchange.Item ?? 0.0m;
                //}
                return result;
            }
            return null;
        }

        public static object CouponDataReport(Swap interestRateSwap)
        {
            if (interestRateSwap != null)
            {
                var coupons1 = interestRateSwap.swapStream[0].cashflows.paymentCalculationPeriod.Length;
                //var pe1 = 0;
                //if(interestRateSwap.swapStream[0].cashflows.principalExchange!=null)
                //{
                //    pe1 = interestRateSwap.swapStream[0].cashflows.principalExchange.Length;
                //}
                var coupons2 = interestRateSwap.swapStream[1].cashflows.paymentCalculationPeriod.Length;
                //var pe2 = 0;
                //if (interestRateSwap.swapStream[1].cashflows.principalExchange != null)
                //{
                //    pe2 = interestRateSwap.swapStream[1].cashflows.principalExchange.Length;
                //}
                var result = new object[coupons1 + coupons2, 4];            
                var index = 0;
                foreach (var coupon1 in interestRateSwap.swapStream[0].cashflows.paymentCalculationPeriod)
                {
                    result[index, 0] = "Leg1_Coupon_" + index;
                    result[index, 1] = coupon1.adjustedPaymentDate;
                    result[index, 2] = coupon1.discountFactor;
                    result[index, 3] = coupon1.forecastPaymentAmount.amount;
                    result[index, 4] = coupon1.forecastPaymentAmount.currency.Value;
                    index++;
                }
                var secondIndex = 0;
                foreach (var coupon2 in interestRateSwap.swapStream[1].cashflows.paymentCalculationPeriod)
                {
                    result[index, 0] = "Leg2_Coupon_" + secondIndex;
                    result[index, 1] = coupon2.adjustedPaymentDate;
                    result[index, 2] = coupon2.discountFactor;
                    result[index, 3] = coupon2.forecastPaymentAmount.amount;
                    result[index, 4] = coupon2.forecastPaymentAmount.currency.Value;
                    index++;
                    secondIndex++;
                }
                return result;
            }
            return null;
        }
    }
}