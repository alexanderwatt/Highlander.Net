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

#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.ValuationEngine.V5r3.Helpers;
using XsdClassesFieldResolver = Highlander.Reporting.V5r3.XsdClassesFieldResolver;

#endregion

namespace Highlander.ValuationEngine.V5r3.Generators
{
    public class FixedAndFloatingRateStreamCashflowGenerator
    {
        /// <summary>
        /// Return a Cashflows object containing array of the payment calculation periods and array of principal exchanges.
        /// </summary>
        /// <param name="interestRateStream">The interest rate stream.</param>
        /// <param name="fixingCalendar"> </param>
        /// <param name="paymentCalendar"> </param>
        /// <returns></returns>
        public static Cashflows GetCashflows(InterestRateStream interestRateStream, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            CalculationPeriodsPrincipalExchangesAndStubs calculationPeriodsPrincipalExchangesAndStubs =
                StreamCashflowsGenerator.GenerateCalculationPeriodsPrincipalExchangesAndStubs(interestRateStream, fixingCalendar, paymentCalendar);
            return GetCashflows(interestRateStream, calculationPeriodsPrincipalExchangesAndStubs, paymentCalendar);
        }

        /// <summary>
        /// Return a Cashflows object containing array of the payment calculation periods and array of principal exchanges.
        /// </summary>
        /// <param name="paymentCalendar"> </param>
        /// <param name="interestRateStream">The interest rate stream.</param>
        /// <param name="rollDates">The list of roll dates.</param>
        /// <returns></returns>
        public static Cashflows GetCashflows(List<DateTime> rollDates, IBusinessCalendar paymentCalendar, InterestRateStream interestRateStream)
        {
            CalculationPeriodsPrincipalExchangesAndStubs calculationPeriodsPrincipalExchangesAndStubs =
                StreamCashflowsGenerator.GenerateCalculationPeriodsPrincipalExchangesAndStubsFromRollDates(interestRateStream, rollDates, paymentCalendar);
            return GetCashflows(interestRateStream, calculationPeriodsPrincipalExchangesAndStubs, paymentCalendar);
        }

        private static Cashflows GetCashflows(
            InterestRateStream interestRateStream, CalculationPeriodsPrincipalExchangesAndStubs calculationPeriodsPrincipalExchangesAndStubs,
            IBusinessCalendar paymentCalendar)
        {
            // Assign notionals, rates, cap/floor rate, etc
            //
            StreamCashflowsGenerator.UpdateCalculationPeriodsData(interestRateStream, calculationPeriodsPrincipalExchangesAndStubs);
            List<PaymentCalculationPeriod> paymentCalculationPeriods = StreamCashflowsGenerator.GetPaymentCalculationPeriods(interestRateStream, calculationPeriodsPrincipalExchangesAndStubs, paymentCalendar);
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(interestRateStream.calculationPeriodAmount);
            UpdateNumberOfDaysAndYearFraction(paymentCalculationPeriods, calculation);
            return CashflowsFactory.Create(paymentCalculationPeriods, calculationPeriodsPrincipalExchangesAndStubs.GetAllPrincipalExchanges(), true);//TODO THe cashflowsmatch paraqmeter at the end could be a problem!
        }

        private static void UpdateNumberOfDaysAndYearFraction(IEnumerable<PaymentCalculationPeriod> paymentCalculationPeriods, Calculation calculation)
        {
            foreach (PaymentCalculationPeriod pcp in paymentCalculationPeriods)
            {
                //  set the calculationPeriodNumberOfDays and dayCountYearFraction fields
                //
                foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(pcp))
                {
                    IDayCounter dayCounter = DayCounterHelper.Parse(calculation.dayCountFraction.Value);
                    calculationPeriod.calculationPeriodNumberOfDays = dayCounter.DayCount(calculationPeriod.adjustedStartDate, calculationPeriod.adjustedEndDate).ToString(CultureInfo.InvariantCulture);
                    calculationPeriod.dayCountYearFraction = (decimal) dayCounter.YearFraction(calculationPeriod.adjustedStartDate, calculationPeriod.adjustedEndDate);
                    calculationPeriod.dayCountYearFractionSpecified = true;
                }
            }
        }

        /// <summary>
        /// Updates forecastRate, forecastPaymentAmount, discountFactor and presentValueAmount of each paymentCalculation period.
        /// </summary>
        /// <param name="interestRateStream">The interest rate stream.</param>
        /// <param name="forecastCurve">The forecast curve.</param>
        /// <param name="discountCurve">The discount curve.</param>
        /// <param name="valuationDate">The valuation date.</param>
        public static void UpdateCashflowsAmounts(InterestRateStream interestRateStream, 
                                                  IRateCurve forecastCurve, IRateCurve discountCurve, 
                                                  DateTime valuationDate)
        {
            //FixAfterManualUpdate(interestRateStream);//should it be removed, since it might produce subtle errors which will be effectively hidden.
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(interestRateStream.calculationPeriodAmount);
            UpdateNumberOfDaysAndYearFraction(new List<PaymentCalculationPeriod>(interestRateStream.cashflows.paymentCalculationPeriod), calculation);
            FloatingRateCalculation floatingRateCalculation = 
                XsdClassesFieldResolver.CalculationHasFloatingRateCalculation(calculation) 
                    ? XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation) : null;
            // calculate forecast payment amount for each payment calculation period
            //
            foreach (PaymentCalculationPeriod period in interestRateStream.cashflows.paymentCalculationPeriod)
            {
                CalculateForecastPaymentAmount(calculation, floatingRateCalculation, period, forecastCurve, discountCurve, valuationDate);
            }
            //  principle exchanges
            //
            if (interestRateStream.cashflows.principalExchange != null)
            {
                foreach (PrincipalExchange principalExchange in interestRateStream.cashflows.principalExchange)
                {
                    CalculateForecastPaymentAmount(principalExchange, discountCurve, valuationDate);
                }
            }
        }

        private static void CalculateForecastPaymentAmount(Calculation calculation,
                                                           FloatingRateCalculation floatingRateCalculation,
                                                           PaymentCalculationPeriod paymentCalculationPeriod,
                                                           IRateCurve forecastCurve,
                                                           IRateCurve discountCurve,
                                                           DateTime valuationDate)
        {
            var amountAccruedPerPaymentPeriod = new List<Money>();
            decimal interestFromPreviousPeriods = 0;
            Notional notionalSchedule = XsdClassesFieldResolver.CalculationGetNotionalSchedule(calculation);
            Currency notionalCurrency = notionalSchedule.notionalStepSchedule.currency;
            //  Cashflows
            //
            foreach (CalculationPeriod calculationPeriod in XsdClassesFieldResolver.GetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod))
            {
                decimal notional = XsdClassesFieldResolver.CalculationPeriodGetNotionalAmount(calculationPeriod);
                decimal finalRate = 0.0m;
                //  If has a fixed rate (fixed rate coupon)
                //
                if (XsdClassesFieldResolver.CalculationPeriodHasFixedRate(calculationPeriod))
                {
                    finalRate = XsdClassesFieldResolver.CalculationPeriodGetFixedRate(calculationPeriod);
                }
                else if (XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod))
                {
                    if (null != forecastCurve)
                    {
                        FloatingRateDefinition floatingRateDefinition = XsdClassesFieldResolver.CalculationPeriodGetFloatingRateDefinition(calculationPeriod);
                        // Apply spread from schedule if it hasn't been specified yet.
                        //
                        if (!floatingRateDefinition.spreadSpecified)
                        {
                            floatingRateDefinition.spread = floatingRateCalculation.spreadSchedule[0].initialValue;
                            floatingRateDefinition.spreadSpecified = true;
                        }
                        ForecastRateHelper.UpdateFloatingRateDefinition(floatingRateDefinition, floatingRateCalculation,
                                                                        calculation.dayCountFraction,
                                                                        calculationPeriod,
                                                                        forecastCurve);
                        calculationPeriod.Item1 = floatingRateDefinition;
                        decimal calculatedRate = floatingRateDefinition.calculatedRate;
                        //  final rate after application of Cap/Floor  rates
                        //
                        finalRate = calculatedRate;
                        //  If has a Cap rate, finalRate = MAX(0, FinalRate - CapRate)
                        //
                        if (null != floatingRateDefinition.capRate)
                        {
                            Strike strike = floatingRateDefinition.capRate[0];
                            finalRate = System.Math.Max(0, finalRate - strike.strikeRate);
                        }
                        //  If has a Floor rate, finalRate = MAX(0, FloorRate - FinalRate)
                        //
                        if (null != floatingRateDefinition.floorRate)
                        {
                            Strike strike = floatingRateDefinition.floorRate[0];
                            finalRate = System.Math.Max(0, strike.strikeRate - finalRate);
                        }
                    }
                }
                else
                {
                    throw new System.Exception("CalculationPeriod has neither fixedRate nor floatngRateDefinition.");
                }
                // Compound interest accrued during previos calculation periods in this payment period. 
                //
                decimal notionalAdjustedForInterestFromPreviousPeriods = notional + interestFromPreviousPeriods;
                if (calculation.discounting == null)
                {
                    interestFromPreviousPeriods = notionalAdjustedForInterestFromPreviousPeriods * finalRate * calculationPeriod.dayCountYearFraction;
                }
                else if (calculation.discounting.discountingType == DiscountingTypeEnum.FRA || calculation.discounting.discountingType == DiscountingTypeEnum.Standard)
                {
                    interestFromPreviousPeriods = notionalAdjustedForInterestFromPreviousPeriods * (1.0m - 1.0m / (1.0m + finalRate * calculationPeriod.dayCountYearFraction));
                }
                else
                {
                    throw new NotSupportedException("The specified discountingType is not supported.");
                }
                Money amountAccruedPerCalculationPeriod = MoneyHelper.GetAmount(interestFromPreviousPeriods, notionalCurrency);
                amountAccruedPerPaymentPeriod.Add(amountAccruedPerCalculationPeriod);
            }
            paymentCalculationPeriod.forecastPaymentAmount = MoneyHelper.Sum(amountAccruedPerPaymentPeriod);
            paymentCalculationPeriod.discountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, paymentCalculationPeriod.adjustedPaymentDate);
            paymentCalculationPeriod.discountFactorSpecified = true;
            paymentCalculationPeriod.presentValueAmount = MoneyHelper.Mul(paymentCalculationPeriod.forecastPaymentAmount, paymentCalculationPeriod.discountFactor);
        }

        private static void CalculateForecastPaymentAmount(PrincipalExchange principleExchange,
                                                           IRateCurve discountCurve,
                                                           DateTime valuationDate)
        {
            principleExchange.discountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, principleExchange.adjustedPrincipalExchangeDate);
            principleExchange.discountFactorSpecified = true;
            principleExchange.presentValuePrincipalExchangeAmount = MoneyHelper.Mul(MoneyHelper.GetAmount(principleExchange.principalExchangeAmount), principleExchange.discountFactor);
        }
    }
}