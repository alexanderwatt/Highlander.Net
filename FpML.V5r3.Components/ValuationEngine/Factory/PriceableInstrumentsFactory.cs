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

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.CalendarEngine.Helpers;
using Orion.ModelFramework;
using Orion.ValuationEngine.Instruments;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Factory
{
    public class PriceableInstrumentsFactory
    {
        /// <summary>
        /// Creates a fixed coupon calculation period.
        /// </summary>
        /// <param name="accrualStartDate"></param>
        /// <param name="accrualEndDate"></param>
        /// <param name="notionalAmount"></param>
        /// <param name="calculation"></param>
        /// <returns></returns>
        public static CalculationPeriod[] CreateSimpleCouponItem(DateTime accrualStartDate, DateTime accrualEndDate,
                                                    Money notionalAmount, Calculation calculation)
        {
            IDayCounter dayCounter = DayCounterHelper.Parse(calculation.dayCountFraction.Value);
            var calculationPeriod = new CalculationPeriod();
            int numDays = dayCounter.DayCount(accrualStartDate, accrualEndDate);
            calculationPeriod.adjustedStartDate = accrualStartDate;
            calculationPeriod.adjustedStartDateSpecified = true;
            calculationPeriod.adjustedEndDate = accrualEndDate;
            calculationPeriod.adjustedEndDateSpecified = true;
            calculationPeriod.dayCountYearFraction = (decimal)dayCounter.YearFraction(accrualStartDate, accrualEndDate);
            calculationPeriod.dayCountYearFractionSpecified = true;
            calculationPeriod.calculationPeriodNumberOfDays = numDays.ToString(CultureInfo.InvariantCulture);
            calculationPeriod.Item = notionalAmount.amount;
            calculationPeriod.unadjustedEndDateSpecified = false;
            calculationPeriod.unadjustedStartDateSpecified = false;
            calculationPeriod.Item1 = calculation;
            var rate = ((Schedule)calculation.Items[0]).initialValue;
            calculationPeriod.forecastRate = rate;
            calculationPeriod.forecastRateSpecified = true;
            calculationPeriod.forecastAmount = MoneyHelper.Mul(notionalAmount,
                                                               calculationPeriod.dayCountYearFraction *
                                                               calculationPeriod.forecastRate);
            var calculationPeriods = new List<CalculationPeriod> { calculationPeriod };
            return calculationPeriods.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buyerIsBase"></param>
        /// <param name="calculation"></param>
        /// <param name="coupon"></param>
        /// <param name="fOCalculationMethod"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static PriceableCapFloorCoupon CreatePriceableCapFloorCoupon(bool buyerIsBase, 
            Calculation calculation, PaymentCalculationPeriod coupon, bool fOCalculationMethod
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var notional = (Notional)calculation.Item;
            var currency = notional.notionalStepSchedule.currency;
            var dayCountFraction = calculation.dayCountFraction;
            var paymentDate = coupon.adjustedPaymentDateSpecified
                                  ? coupon.adjustedPaymentDate
                                  : coupon.unadjustedPaymentDate;
            var calculationPeriods =
                XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(coupon);
            if (calculationPeriods.Length == 1)
            {
                var calculationPeriod = calculationPeriods[0];
                //Money expectedCashFlow = null;
                decimal notionalamount = XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriod);
                Money money = buyerIsBase ? MoneyHelper.GetAmount(-1 * notionalamount, currency) : MoneyHelper.GetAmount(notionalamount, currency);
                var accrualStartDate = calculationPeriod.adjustedStartDateSpecified
                                           ? calculationPeriod.adjustedStartDate
                                           : calculationPeriod.unadjustedStartDate;
                var accrualEndDate = calculationPeriod.adjustedEndDateSpecified
                                         ? calculationPeriod.adjustedEndDate
                                         : calculationPeriod.unadjustedEndDate;
                //  If has a fixed rate (fixed rate coupon)
                var isThereDiscounting = XsdClassesFieldResolver.CalculationHasDiscounting(calculation);
                if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    //The floating rate definition.
                    FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
                    //The floatingrateCalculation.
                    Debug.Assert(calculation.Items != null);
                    Debug.Assert(calculation.Items.Length > 0);
                    Debug.Assert(calculation.Items[0] is FloatingRateCalculation);
                    var floatingRateCalculation = (FloatingRateCalculation)calculation.Items[0];
                    //The forecast rate index.
                    var floatingRateIndex = floatingRateCalculation.floatingRateIndex;
                    var indexTenor = floatingRateCalculation.indexTenor.ToString();
                    var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex.Value, indexTenor);
                    //The rate observation
                    // Apply spread from schedule if it hasn't been specified yet.
                    decimal margin = 0m;
                    if (floatingRateDefinition.spreadSpecified)
                    {
                        margin = floatingRateDefinition.spread;
                    }
                    //The observed rate.
                    Decimal? observedRate = null;
                    Decimal? capStrike = null;
                    Decimal? floorStrike = null;
                    if (floatingRateDefinition.capRate!=null)
                    {
                        capStrike = floatingRateDefinition.capRate[0].strikeRate;
                    }
                    if (floatingRateDefinition.floorRate != null)
                    {
                        floorStrike = floatingRateDefinition.floorRate[0].strikeRate;
                    }
                    if (floatingRateDefinition.rateObservation != null)
                    {
                        var rateObservation = floatingRateDefinition.rateObservation[0];
                        if (rateObservation.observedRateSpecified)
                        {
                            observedRate = rateObservation.observedRate;
                        }
                        PriceableCapFloorCoupon rateCoupon;
                        if (isThereDiscounting)
                        {
                            var discounting = XsdClassesFieldResolver.CalculationGetDiscounting(calculation);
                            var floatingCouponWithDiscounting = new PriceableCapFloorCoupon(coupon.id, buyerIsBase,
                                                                         capStrike, floorStrike, accrualStartDate, accrualEndDate,
                                                                         rateObservation.adjustedFixingDate, dayCountFraction,
                                                                         margin, observedRate, money, paymentDate,
                                                                         forecastRate, discounting.discountingType,
                                                                         observedRate, null, fixingCalendar, paymentCalendar);
                            if (fOCalculationMethod)
                            {
                                floatingCouponWithDiscounting.ForecastRateInterpolation = true;
                            }
                            rateCoupon = floatingCouponWithDiscounting;
                            return rateCoupon;
                        }
                        var floatingCoupon = new PriceableCapFloorCoupon(coupon.id, buyerIsBase, capStrike, floorStrike, 
                                                                     accrualStartDate, accrualEndDate,
                                                                     rateObservation.adjustedFixingDate, dayCountFraction,
                                                                     margin, observedRate, money, paymentDate,
                                                                     forecastRate, null, null, null,
                                                                     fixingCalendar, paymentCalendar);
                        if (fOCalculationMethod)
                        {
                            floatingCoupon.ForecastRateInterpolation = true;
                        }
                        rateCoupon = floatingCoupon;
                        return rateCoupon;
                    }
                    throw new NotImplementedException("Need to return a rate coupon, Alex!");
                }
                throw new System.Exception("CalculationPeriod has neither fixedRate nor floatingRateDefinition.");
            }
            throw new System.Exception("PaymentCalculationPeriod has zero, or multiple CalculationPeriods.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payerIsBase"></param>
        /// <param name="coupons"></param>
        /// <param name="calculation"></param>
        /// <param name="fOCalculationMethod"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static List<PriceableRateCoupon> CreatePriceableCoupons(bool payerIsBase, PaymentCalculationPeriod[] coupons, 
            Calculation calculation, bool fOCalculationMethod
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            //result = coupons.Select(cashflow => CreatePriceableCoupon(payerIsBase, calculation, cashflow, fOCalculationMethod, fixingCalendar, paymentCalendar)).ToList();
            return coupons?.Select(coupon => CreatePriceableCoupon(payerIsBase, calculation, coupon, fOCalculationMethod, fixingCalendar, paymentCalendar)).Where(rateCoupon => rateCoupon != null).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coupons"></param>
        /// <param name="calculation"></param>
        /// <param name="fOCalculationMethod"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static List<PriceableRateCoupon> CreatePriceableCoupons(PaymentCalculationPeriod[] coupons
            , Calculation calculation, bool fOCalculationMethod
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var priceableCashflows = CreatePriceableCoupons(false, coupons, calculation, fOCalculationMethod, fixingCalendar, paymentCalendar);
            return priceableCashflows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payerIsBase"></param>
        /// <param name="exchanges"></param>
        /// <param name="currency"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static List<PriceablePrincipalExchange> CreatePriceablePrincipalExchanges(bool payerIsBase, PrincipalExchange[] exchanges, 
            string currency, IBusinessCalendar paymentCalendar)
        {
            List<PriceablePrincipalExchange> priceableCashflows = null;
            var multiplier = payerIsBase ? -1 : 1;
            if (exchanges != null)
            {
                priceableCashflows = new List<PriceablePrincipalExchange>();
                var index = 0;
                foreach (var principalExchange in exchanges)
                {
                    var adjustedPaymentDate = principalExchange.adjustedPrincipalExchangeDate;
                    var amount = multiplier * principalExchange.principalExchangeAmount;
                    var exchange = new PriceablePrincipalExchange("PrincipalExchange_" + index, payerIsBase, amount, currency, adjustedPaymentDate, paymentCalendar);
                    priceableCashflows.Add(exchange);
                    index++;
                }
            }
            return priceableCashflows;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseParty"></param>
        /// <param name="fxOptionLeg"></param>
        /// <returns></returns>
        public static IList<PriceablePayment> CreatePriceableFxOptionLegPayment(string baseParty, FxOption fxOptionLeg)
        {
            //string id1 = fxLeg.exchangedCurrency1.id ?? "Payment_" + fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
            //string id2 = fxLeg.exchangedCurrency2.id ?? "Payment_" + fxLeg.exchangedCurrency2.paymentAmount.currency.Value;

            var priceablePayments = new List<PriceablePayment>();
            //bool payerIsBase = false;
            ////payment1
            //var paymentAmount1 = fxLeg.exchangedCurrency1.paymentAmount.amount;
            //if (baseParty == fxLeg.exchangedCurrency1.payerPartyReference.href)
            //{
            //    paymentAmount1 *= -1; // *fxLeg.exchangedCurrency1.paymentAmount.amount;
            //    payerIsBase = true;
            //}
            //var paymentDate = fxLeg.Items[0];
            //var currency1 = fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
            //priceablePayments.Add(new PriceablePayment(id1, fxLeg.exchangedCurrency1.payerPartyReference.href,
            //    fxLeg.exchangedCurrency2.payerPartyReference.href, payerIsBase,
            //    paymentAmount1, currency1, paymentDate, null));
            ////payment2
            //var paymentAmount2 = fxLeg.exchangedCurrency2.paymentAmount.amount;
            //if (baseParty == fxLeg.exchangedCurrency2.payerPartyReference.href)
            //{
            //    paymentAmount2 *= -1; // *fxLeg.exchangedCurrency2.paymentAmount.amount;
            //    payerIsBase = true;
            //}
            //var currency2 = fxLeg.exchangedCurrency2.paymentAmount.currency.Value;
            //priceablePayments.Add(new PriceablePayment(id2, fxLeg.exchangedCurrency1.payerPartyReference.href,
            //    fxLeg.exchangedCurrency2.payerPartyReference.href, payerIsBase, paymentAmount2, currency2, paymentDate, null));
            return priceablePayments;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseParty"></param>
        /// <param name="fxLeg"></param>
        /// <returns></returns>
        public static IList<PriceablePayment> CreatePriceableFxLegPayment(string baseParty, FxSwapLeg fxLeg)//TODO Fix this.
        {
            string id1 = fxLeg.exchangedCurrency1.id ?? "Payment_" + fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
            string id2 = fxLeg.exchangedCurrency2.id ?? "Payment_" + fxLeg.exchangedCurrency2.paymentAmount.currency.Value;

            var priceablePayments = new List<PriceablePayment>();
            bool payerIsBase = false;
            //payment1
            var paymentAmount1 = fxLeg.exchangedCurrency1.paymentAmount.amount;
            if (baseParty == fxLeg.exchangedCurrency1.payerPartyReference.href)
            {
                paymentAmount1 *= -1; // *fxLeg.exchangedCurrency1.paymentAmount.amount;
                payerIsBase = true;
            }
            var paymentDate = fxLeg.Items[0];
            var currency1 = fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
            priceablePayments.Add(new PriceablePayment(id1, fxLeg.exchangedCurrency1.payerPartyReference.href,
                fxLeg.exchangedCurrency2.payerPartyReference.href, payerIsBase,
                paymentAmount1, currency1, paymentDate, null));
            //payment2
            var paymentAmount2 = fxLeg.exchangedCurrency2.paymentAmount.amount;
            if (baseParty == fxLeg.exchangedCurrency2.payerPartyReference.href)
            {
                paymentAmount2 *= -1; // *fxLeg.exchangedCurrency2.paymentAmount.amount;
                payerIsBase = true;
            }
            var currency2 = fxLeg.exchangedCurrency2.paymentAmount.currency.Value;
            priceablePayments.Add(new PriceablePayment(id2, fxLeg.exchangedCurrency1.payerPartyReference.href,
                fxLeg.exchangedCurrency2.payerPartyReference.href, payerIsBase, paymentAmount2, currency2, paymentDate, null));
            return priceablePayments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseParty"></param>
        /// <param name="fxLeg"></param>
        /// <returns></returns>
        public static IList<PriceablePayment> CreatePriceableFxLegPayment(string baseParty, FxSingleLeg fxLeg)//TODO Fix this.
        {
            string id1 = fxLeg.exchangedCurrency1.id ?? "Payment_" + fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
            string id2 = fxLeg.exchangedCurrency2.id ?? "Payment_" + fxLeg.exchangedCurrency2.paymentAmount.currency.Value;
            
            var priceablePayments = new List<PriceablePayment>();
            bool payerIsBase = false;
            //payment1
            var paymentAmount1 = fxLeg.exchangedCurrency1.paymentAmount.amount;
            if(baseParty == fxLeg.exchangedCurrency1.payerPartyReference.href)
            {
                paymentAmount1 *= -1; // *fxLeg.exchangedCurrency1.paymentAmount.amount;
                payerIsBase = true;
            }
            var paymentDate = new DateTime();
            if (fxLeg.Items1ElementName != null && fxLeg.Items1 != null)
            {
                var index = 0;
                foreach (var element in fxLeg.Items1ElementName)
                {
                    if (element == Items1ChoiceType.valueDate)
                    {
                        paymentDate = fxLeg.Items1[index];
                        index++;
                    }
                }
            }
            var currency1 = fxLeg.exchangedCurrency1.paymentAmount.currency.Value;
            priceablePayments.Add(new PriceablePayment(id1, fxLeg.exchangedCurrency1.payerPartyReference.href, 
                fxLeg.exchangedCurrency2.payerPartyReference.href, payerIsBase,
                paymentAmount1, currency1, paymentDate, null));
            //payment2
            var paymentAmount2 = fxLeg.exchangedCurrency2.paymentAmount.amount;
            if (baseParty == fxLeg.exchangedCurrency2.payerPartyReference.href)
            {
                paymentAmount2 *= -1; // *fxLeg.exchangedCurrency2.paymentAmount.amount;
                payerIsBase = true;
            }
            var currency2 = fxLeg.exchangedCurrency2.paymentAmount.currency.Value;
            priceablePayments.Add(new PriceablePayment(id2, fxLeg.exchangedCurrency1.payerPartyReference.href,
                fxLeg.exchangedCurrency2.payerPartyReference.href, payerIsBase, paymentAmount2, currency2, paymentDate, null));
            return priceablePayments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseParty"></param>
        /// <param name="payment"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static PriceablePayment CreatePriceablePayment(string baseParty, Payment payment, IBusinessCalendar paymentCalendar)
        {
            const int index = 0;
            bool payerIsBase = false;
            string id;
            if (payment.id == null)
            {
                id = "Payment_" + index;
            }
            else
            {
                id = payment.id;
            }
            if (baseParty == payment.payerPartyReference.href)
            {
                payerIsBase = true;
            }
            var multiplier = payerIsBase ? -1.0m : 1.0m;
            if (payment.paymentDate != null)
            {
                return new PriceablePayment(id, payment.payerPartyReference.href,
                payment.receiverPartyReference.href, payerIsBase, MoneyHelper.Mul(payment.paymentAmount, multiplier),
                                                        payment.paymentDate, paymentCalendar);
            }
            var amount =  MoneyHelper.Mul( payment.paymentAmount, multiplier);
            return new PriceablePayment(id, payment.payerPartyReference.href,
                                        payment.receiverPartyReference.href, payerIsBase, amount, payment.paymentDate, paymentCalendar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="baseDateForRelativeOffset"></param>
        /// <param name="baseParty"></param>
        /// <param name="optionPremia"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"> </param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static PriceableFxOptionPremium CreatePriceableFxOptionPremium(ICoreCache cache, String nameSpace,
            DateTime? baseDateForRelativeOffset, string baseParty, FxOptionPremium optionPremia, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            bool payerIsBase = baseParty == optionPremia.payerPartyReference.href;
            var multiplier = payerIsBase ? -1.0m : 1.0m;
            var settlementDate = AdjustedDateHelper.GetAdjustedDate(cache, nameSpace, fixingCalendar, baseDateForRelativeOffset, optionPremia.paymentDate);
            if (settlementDate == null) return null;
            var date = new PriceableFxOptionPremium("FxOptionPremiumPayment", optionPremia.payerPartyReference.href,
                                                    optionPremia.receiverPartyReference.href, payerIsBase,
                                                    MoneyHelper.Mul(optionPremia.paymentAmount, multiplier),
                                                    (DateTime) settlementDate, optionPremia.quote,
                                                    optionPremia.settlementInformation, paymentCalendar);
            return date;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseParty"></param>
        /// <param name="payments"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static List<PriceablePayment> CreatePriceablePayments(string baseParty, Payment[] payments, IBusinessCalendar paymentCalendar)
        {
            return payments.Select(payment => CreatePriceablePayment(baseParty, payment, paymentCalendar)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculation"></param>
        /// <param name="coupon"></param>
        /// <param name="fOCalculationMethod"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static PriceableRateCoupon CreatePriceableCoupon(Calculation calculation, PaymentCalculationPeriod coupon, bool fOCalculationMethod, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var rateCoupon = CreatePriceableCoupon(false, calculation, coupon, fOCalculationMethod, fixingCalendar, paymentCalendar);
            return rateCoupon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payerIsBase"></param>
        /// <param name="calculation"></param>
        /// <param name="coupon"></param>
        /// <param name="fOCalculationMethod"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static PriceableRateCoupon CreatePriceableCoupon(bool payerIsBase, Calculation calculation, PaymentCalculationPeriod coupon, bool fOCalculationMethod
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var notional = (Notional)calculation.Item;
            var currency = notional.notionalStepSchedule.currency;
            var dayCountFraction = calculation.dayCountFraction;
            var paymentDate = coupon.adjustedPaymentDateSpecified
                                  ? coupon.adjustedPaymentDate
                                  : coupon.unadjustedPaymentDate;
            var calculationPeriods = XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(coupon);
            var discountFactorSpecified = coupon.discountFactorSpecified;
            if (calculationPeriods.Length == 1)
            {
                var calculationPeriod = calculationPeriods[0];
                var notionalAmount = XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriod);
                var money = payerIsBase ? MoneyHelper.GetAmount(-1 * notionalAmount, currency) : MoneyHelper.GetAmount(notionalAmount, currency);               
                var accrualStartDate = calculationPeriod.adjustedStartDateSpecified
                                           ? calculationPeriod.adjustedStartDate
                                           : calculationPeriod.unadjustedStartDate;
                var accrualEndDate = calculationPeriod.adjustedEndDateSpecified
                                         ? calculationPeriod.adjustedEndDate
                                         : calculationPeriod.unadjustedEndDate;
                //  If has a fixed rate (fixed rate coupon)
                var isThereDiscounting = XsdClassesFieldResolver.CalculationHasDiscounting(calculation);
                PriceableRateCoupon rateCoupon;
                if (XsdClassesFieldResolver.CalculationPeriodHasFixedRate(calculationPeriod))
                {
                    decimal finalRate = XsdClassesFieldResolver.CalculationPeriodGetFixedRate(calculationPeriod);
                    //The discount rate must be set. It would normally be the final rate. The assumption is that, it the discounting rate is zero then the final rate should be used.
                    if (isThereDiscounting)
                    {
                        var discounting = XsdClassesFieldResolver.CalculationGetDiscounting(calculation);
                        //This test works because if the rate is zero, then the coupon is not discounted and discounting Type should be null.
                        var discountRate = discounting.discountRate == 0.0m ? finalRate : discounting.discountRate;
                        rateCoupon = new PriceableFixedRateCoupon(coupon.id, payerIsBase, accrualStartDate, accrualEndDate,
                                                            dayCountFraction, finalRate, money, null, paymentDate, discounting.discountingType, discountRate,
                                                            null, paymentCalendar);
                        if (discountFactorSpecified)
                        {
                            rateCoupon.PaymentDiscountFactor = coupon.discountFactor;
                        }
                        return rateCoupon;
                    }
                    rateCoupon = new PriceableFixedRateCoupon(coupon.id, payerIsBase, accrualStartDate, accrualEndDate,
                                                              dayCountFraction, finalRate, money, null, paymentDate, null, null,
                                                              null, paymentCalendar);
                    if (discountFactorSpecified)
                    {
                        rateCoupon.PaymentDiscountFactor = coupon.discountFactor;
                    }
                    return rateCoupon;
                }
                if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    //The floating rate definition.
                    FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
                    //The floating rate Calculation.
                    Debug.Assert(calculation.Items != null);
                    Debug.Assert(calculation.Items.Length > 0);
                    Debug.Assert(calculation.Items[0] is FloatingRateCalculation);
                    var floatingRateCalculation = (FloatingRateCalculation)calculation.Items[0];
                    //The forecast rate index.
                    var floatingRateIndex = floatingRateCalculation.floatingRateIndex;
                    var indexTenor = floatingRateCalculation.indexTenor.ToString();
                    var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex.Value, indexTenor);
                    //The rate observation
                    // Apply spread from schedule if it hasn't been specified yet.
                    var margin = 0m;
                    if (floatingRateDefinition.spreadSpecified)
                    {
                        margin = floatingRateDefinition.spread;
                    }
                    //The observed rate.
                    Decimal? observedRate = null;
                    Decimal? capStrike = null;
                    Decimal? floorStrike = null;
                    if (floatingRateDefinition.capRate != null)
                    {
                        capStrike = floatingRateDefinition.capRate[0].strikeRate;
                    }
                    if (floatingRateDefinition.floorRate != null)
                    {
                        floorStrike = floatingRateDefinition.floorRate[0].strikeRate;
                    }
                    if (floatingRateDefinition.rateObservation != null)//TODO This is a big problem. Need to handle the case of no fixing date!
                    {
                        var rateObservation = floatingRateDefinition.rateObservation[0];
                        if (rateObservation.observedRateSpecified)
                        {
                            observedRate = rateObservation.observedRate;
                        }
                        //Removed because Igor's old code populates these fields when the trade is created. This means the coupon is not recalculated!
                        //Now the coupon will ignore any previous calculations and only treat as a fixed coupon if the observed rate has been specified.
                        if (isThereDiscounting)
                        {
                            var discounting = XsdClassesFieldResolver.CalculationGetDiscounting(calculation);
                            if (capStrike != null || floorStrike != null)
                            {
                                rateCoupon = new PriceableCapFloorCoupon(coupon.id, !payerIsBase,
                                                                         capStrike, floorStrike, accrualStartDate, accrualEndDate,
                                                                         rateObservation.adjustedFixingDate, dayCountFraction,
                                                                         margin, observedRate, money, paymentDate,
                                                                         forecastRate, discounting.discountingType,
                                                                         observedRate, null, fixingCalendar, paymentCalendar);
                            }
                            else
                            {
                                rateCoupon = new PriceableFloatingRateCoupon(coupon.id, !payerIsBase, accrualStartDate, accrualEndDate,
                                                                             rateObservation.adjustedFixingDate, dayCountFraction,
                                                                             margin, observedRate, money, paymentDate,
                                                                             forecastRate, discounting.discountingType,
                                                                             observedRate, null, fixingCalendar, paymentCalendar);
                            }
                        }
                        else
                        {
                            if (capStrike != null || floorStrike != null)
                            {
                                rateCoupon = new PriceableCapFloorCoupon(coupon.id, !payerIsBase, capStrike, floorStrike, 
                                                                     accrualStartDate, accrualEndDate,
                                                                     rateObservation.adjustedFixingDate, dayCountFraction,
                                                                     margin, observedRate, money, paymentDate,
                                                                     forecastRate, null, null, null,
                                                                     fixingCalendar, paymentCalendar);
                            }
                            else
                            {
                                rateCoupon = new PriceableFloatingRateCoupon(coupon.id, payerIsBase, accrualStartDate, accrualEndDate,
                                                                             rateObservation.adjustedFixingDate, dayCountFraction,
                                                                             margin, observedRate, money, paymentDate,
                                                                             forecastRate, null, null, null,
                                                                             fixingCalendar, paymentCalendar);
                            }
                        }
                        if (fOCalculationMethod)
                        {
                            ((PriceableFloatingRateCoupon)rateCoupon).ForecastRateInterpolation = true;
                        }
                        if (discountFactorSpecified)
                        {
                            rateCoupon.PaymentDiscountFactor = coupon.discountFactor;
                        }
                        return rateCoupon;
                    }
                    throw new NotImplementedException("Need to return a rate coupon, Alex!");
                }
                throw new System.Exception("CalculationPeriod has neither fixedRate nor floatingRateDefinition.");
            }
            throw new System.Exception("PaymentCalculationPeriod has zero, or multiple CalculationPeriods.");
        }

        /// <summary>
        /// Gets the unadjusted payment date.
        /// </summary>
        /// <param name="unadjustedStartDate">The unadjusted start date.</param>
        /// <param name="unadjustedEndDate">The unadjusted end date.</param>
        /// <param name="payRelativeTo">The pay relative to.</param>
        /// <returns></returns>
        public static DateTime GetUnadjustedPaymentDate(DateTime unadjustedStartDate, DateTime unadjustedEndDate, PayRelativeToEnum payRelativeTo)
        {
            DateTime unadjustedPaymentDate;
            switch (payRelativeTo)
            {
                case PayRelativeToEnum.CalculationPeriodStartDate:
                    unadjustedPaymentDate = unadjustedStartDate;
                    break;
                case PayRelativeToEnum.CalculationPeriodEndDate:
                    unadjustedPaymentDate = unadjustedEndDate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{payRelativeTo} Payment relative not supported!");
            }
            return unadjustedPaymentDate;
        }
    }
}