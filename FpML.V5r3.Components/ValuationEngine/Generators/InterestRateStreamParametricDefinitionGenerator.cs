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

#region Using directives

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Generators
{
    public class InterestRateStreamParametricDefinitionGenerator
    {
        #region Generate methods

        public static InterestRateStream GenerateStreamDefinition(SwapLegParametersRange legParametersRange)
        {
            InterestRateStream result;
            if (legParametersRange.LegType == LegType.Fixed)
            {
                result = GenerateFixedStreamDefinition(legParametersRange);
            }
            else if (legParametersRange.LegType == LegType.Floating)
            {
                result = GenerateFloatingStreamDefinition(legParametersRange);
            }
            else
            {
                throw new NotImplementedException(String.Format("'{0}' leg type is not suported!", legParametersRange.LegType));
            }
            //  Payer/Receiver references
            //
            result.payerPartyReference = PartyReferenceFactory.Create(legParametersRange.Payer);
            result.receiverPartyReference = PartyReferenceFactory.Create(legParametersRange.Receiver);
            //  Principal exchanges
            //
            result.principalExchanges = new PrincipalExchanges();
            result.principalExchanges.initialExchange =
                result.principalExchanges.finalExchange =
                result.principalExchanges.intermediateExchange = legParametersRange.GeneratePrincipalExchanges;
            return result;
        }

        public static InterestRateStream GenerateStreamDefinition(CapFloorLegParametersRange legParametersRange)
        {
            InterestRateStream stream = GenerateCapFloorStreamDefinition(legParametersRange);
            //  Payer/Receiver references
            //
            stream.payerPartyReference = PartyReferenceFactory.Create(legParametersRange.Payer);
            stream.receiverPartyReference = PartyReferenceFactory.Create(legParametersRange.Receiver);
            //  Principal exchanges
            //
            stream.principalExchanges = new PrincipalExchanges();
            stream.principalExchanges.initialExchange =
                stream.principalExchanges.finalExchange =
                stream.principalExchanges.intermediateExchange = legParametersRange.GeneratePrincipalExchanges;
            return stream;
        }

        private static PayRelativeToEnum   DiscountingTypeToPayRelativeTo(DiscountingTypeEnum? discountingType)
        {
            if (discountingType != null)
            {
                switch (discountingType)
                {
                    case DiscountingTypeEnum.Standard:
                        {
                            return PayRelativeToEnum.CalculationPeriodStartDate;
                        }
                    case DiscountingTypeEnum.FRA:
                        {
                            return PayRelativeToEnum.CalculationPeriodStartDate;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(discountingType));
                }
            }
            return PayRelativeToEnum.CalculationPeriodEndDate;
        }

        private static InterestRateStream GenerateFloatingStreamDefinition(SwapLegParametersRange legParametersRange)
        {
            Discounting discounting = null;
            InterestRateStream stream;
            if (legParametersRange.DiscountingType != null && legParametersRange.DiscountingType.ToUpper() != "NONE")
            {
                discounting = new Discounting{ discountingType = EnumHelper.Parse<DiscountingTypeEnum>(legParametersRange.DiscountingType) };
                // Create the stream object
                stream = InterestRateStreamFactory.CreateFloatingRateStream(DiscountingTypeToPayRelativeTo(discounting.discountingType));
            }
            else
            {
                // Create the stream object
                //
                stream = InterestRateStreamFactory.CreateFloatingRateStream(DiscountingTypeToPayRelativeTo(null));
            }
            // Set effective and termination dates of the stream.
            //
            SetEffectiveAndTerminationDates(stream, legParametersRange.EffectiveDate, legParametersRange.MaturityDate, legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            //Set the FirstRegularPeriodStartDate
            SetFirstRegularPeriodStartDate(stream, legParametersRange.FirstRegularPeriodStartDate);
            //Set the LastRegularPeriodEndDate
            SetLastRegularPeriodEndDate(stream, legParametersRange.LastRegularPeriodEndDate);
            // Adjusted or unadjusted swap
            //Set the stub period type
            var dateAdjustments = AdjustedType.Adjusted != legParametersRange.AdjustedType 
                ? BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.NONE, legParametersRange.PaymentCalendar) 
                : BusinessDayAdjustmentsHelper.Create(legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            stream.calculationPeriodDates.calculationPeriodDatesAdjustments = dateAdjustments;
            stream.calculationPeriodDates.calculationPeriodFrequency = CalculationPeriodFrequencyHelper.Parse(legParametersRange.PaymentFrequency, legParametersRange.RollConvention);
            if (legParametersRange.FirstCouponType == FirstCouponType.Full)
            {
                var firstCouponStartDate = new AdjustableDate
                                               {dateAdjustments = dateAdjustments, id = "FullFirstCoupon"};
                SetFirstPeriodStartDate(stream, firstCouponStartDate);
            }
            //Set the payment dates
            stream.paymentDates.paymentFrequency = PeriodHelper.Parse(legParametersRange.PaymentFrequency).ToFrequency();
            stream.paymentDates.paymentDatesAdjustments = BusinessDayAdjustmentsHelper.Create(legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            stream.resetDates.fixingDates = RelativeDateOffsetHelper.Create(legParametersRange.PaymentFrequency, DayTypeEnum.Business, BusinessDayConventionEnum.NONE.ToString(), legParametersRange.FixingCalendar, "resetDates");//"NONE" & "resedDates" - hardcoded
            stream.resetDates.resetFrequency = ResetFrequencyHelper.Parse(legParametersRange.PaymentFrequency);
            stream.resetDates.resetDatesAdjustments = BusinessDayAdjustmentsHelper.Create(legParametersRange.FixingBusinessDayAdjustments, legParametersRange.FixingCalendar);
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);         
            //  Set discounting type
            //
            calculation.discounting = discounting;
            // Set notional amount (as the initial value in notional schedule)
            //
            SetNotional(calculation, legParametersRange.NotionalAmount, legParametersRange.Currency);
            // Set floating rate index name
            //
            string indexTenor = legParametersRange.PaymentFrequency;            
            //string indexName = legParametersRange.ForecastCurve;
            string indexName = legParametersRange.ForecastIndexName;
            FloatingRateCalculation floatingRateCalculation = FloatingRateCalculationFactory.Create(indexName, indexTenor, legParametersRange.FloatingRateSpread);
            XsdClassesFieldResolver.CalculationSetFloatingRateCalculation(calculation, floatingRateCalculation);
            // Set day count convention
            //
            calculation.dayCountFraction = DayCountFractionHelper.Parse(legParametersRange.DayCount);
            return stream;
        }

        private static InterestRateStream GenerateCapFloorStreamDefinition(CapFloorLegParametersRange legParametersRange)
        {
            Discounting discounting = null;
            InterestRateStream stream;
            if (legParametersRange.DiscountingType != null && legParametersRange.DiscountingType.ToUpper() != "NONE")
            {
                discounting = new Discounting
                {
                    discountingType =
                        EnumHelper.Parse<DiscountingTypeEnum>(legParametersRange.DiscountingType)
                };
                stream = InterestRateStreamFactory.CreateFloatingRateStream(DiscountingTypeToPayRelativeTo(discounting.discountingType));
            }
            // Create the stream object
            //
            else
            {
                stream = InterestRateStreamFactory.CreateFloatingRateStream(DiscountingTypeToPayRelativeTo(null));
            }
            // Set effective and termination dates of the stream.
            //
            SetEffectiveAndTerminationDates(stream, legParametersRange.EffectiveDate, legParametersRange.MaturityDate, legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            // Adjusted or unadjusted swap
            //
            stream.calculationPeriodDates.calculationPeriodDatesAdjustments = AdjustedType.Adjusted != legParametersRange.AdjustedType ? BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.NONE, legParametersRange.PaymentCalendar) : BusinessDayAdjustmentsHelper.Create(legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            stream.calculationPeriodDates.calculationPeriodFrequency = CalculationPeriodFrequencyHelper.Parse(legParametersRange.PaymentFrequency, legParametersRange.RollConvention);
            stream.paymentDates.paymentFrequency = PeriodHelper.Parse(legParametersRange.PaymentFrequency).ToFrequency();
            stream.paymentDates.paymentDatesAdjustments = BusinessDayAdjustmentsHelper.Create(legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            stream.resetDates.fixingDates = RelativeDateOffsetHelper.Create(legParametersRange.PaymentFrequency, DayTypeEnum.Business, BusinessDayConventionEnum.NONE.ToString(), legParametersRange.FixingCalendar, "resetDates");//"NONE" & "resedDates" - hardcoded
            stream.resetDates.resetFrequency = ResetFrequencyHelper.Parse(legParametersRange.PaymentFrequency);
            stream.resetDates.resetDatesAdjustments = BusinessDayAdjustmentsHelper.Create(legParametersRange.FixingBusinessDayAdjustments, legParametersRange.FixingCalendar);
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            //  Set discounting type
            //
            calculation.discounting = discounting;
            // Set notional amount (as the initial value in notional schedule)
            //
            SetNotional(calculation, legParametersRange.NotionalAmount, legParametersRange.Currency);
            // Set floating rate index name
            //
            string indexTenor = legParametersRange.PaymentFrequency;
            string indexName = legParametersRange.ForecastIndexName;
            FloatingRateCalculation floatingRateCalculation = FloatingRateCalculationFactory.Create(indexName, indexTenor, legParametersRange.FloatingRateSpread);
            XsdClassesFieldResolver.CalculationSetFloatingRateCalculation(calculation, floatingRateCalculation);
            if (legParametersRange.CapOrFloor == CapFloorType.Cap)
            {
                floatingRateCalculation.capRateSchedule = new[] { new StrikeSchedule() };
                floatingRateCalculation.capRateSchedule[0].initialValue = legParametersRange.StrikeRate;
                floatingRateCalculation.capRateSchedule[0].buyer = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Receiver };
                floatingRateCalculation.capRateSchedule[0].seller = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Payer };
            }
            else
            {
                floatingRateCalculation.floorRateSchedule = new[] { new StrikeSchedule() };
                floatingRateCalculation.floorRateSchedule[0].initialValue = legParametersRange.StrikeRate;
                floatingRateCalculation.floorRateSchedule[0].buyer = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Receiver };
                floatingRateCalculation.floorRateSchedule[0].seller = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Payer };
            }
            // Set day count convention
            //
            calculation.dayCountFraction = DayCountFractionHelper.Parse(legParametersRange.DayCount);
            return stream;
        }

        private static InterestRateStream GenerateFixedStreamDefinition(SwapLegParametersRange legParametersRange)
        {
            var discountingType = legParametersRange.DiscountingType;
            InterestRateStream stream;
            Discounting discounting = null;
            if (discountingType != null && discountingType.ToUpper()!="NONE")
            {
                discounting = new Discounting{ discountingType =  EnumHelper.Parse<DiscountingTypeEnum>(legParametersRange.DiscountingType), discountingTypeSpecified = true};
                stream = InterestRateStreamFactory.CreateFixedRateStream(DiscountingTypeToPayRelativeTo(discounting.discountingType));
            }
            else
            {
                stream = InterestRateStreamFactory.CreateFixedRateStream(DiscountingTypeToPayRelativeTo(null));
            }
            // Set effective and termination dates of the stream.
            //
            SetEffectiveAndTerminationDates(stream, legParametersRange.EffectiveDate, legParametersRange.MaturityDate, legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            //Set the FirstRegularPeriodStartDate
            SetFirstRegularPeriodStartDate(stream, legParametersRange.FirstRegularPeriodStartDate);
            //Set the LastRegularPeriodEndDate
            SetLastRegularPeriodEndDate(stream, legParametersRange.LastRegularPeriodEndDate);
            // Adjusted or unadjusted swap
            //
            var dateAdjustments = AdjustedType.Adjusted != legParametersRange.AdjustedType ? BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.NONE, legParametersRange.PaymentCalendar) : BusinessDayAdjustmentsHelper.Create(legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            stream.calculationPeriodDates.calculationPeriodDatesAdjustments = dateAdjustments;
            stream.calculationPeriodDates.calculationPeriodFrequency = CalculationPeriodFrequencyHelper.Parse(legParametersRange.PaymentFrequency, legParametersRange.RollConvention);
            //Set FirstPeriodStartDate i.e. Full or Partial period.
            if (legParametersRange.FirstCouponType == FirstCouponType.Full)
            {
                var firstCouponStartDate = new AdjustableDate
                                               {dateAdjustments = dateAdjustments, id = "FullFirstCoupon"};
                SetFirstPeriodStartDate(stream, firstCouponStartDate);
            }
            // Set payment dates frequency and adjustments
            //
            stream.paymentDates.paymentFrequency = PeriodHelper.Parse(legParametersRange.PaymentFrequency).ToFrequency();
            stream.paymentDates.paymentDatesAdjustments = BusinessDayAdjustmentsHelper.Create(legParametersRange.PaymentBusinessDayAdjustments, legParametersRange.PaymentCalendar);
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            //  Set discounting type
            //
            calculation.discounting = discounting;          
            // Set notional amount (as the initial value in notional schedule)
            //
            SetNotional(calculation, legParametersRange.NotionalAmount, legParametersRange.Currency);
            // Set fixed rate (as the initial value in fixed-rate schedule)
            //
            Schedule fixedRateSchedule = ScheduleHelper.Create(legParametersRange.CouponOrLastResetRate);
            XsdClassesFieldResolver.CalculationSetFixedRateSchedule(calculation, fixedRateSchedule);
            // Set the 'day count convention'
            //
            calculation.dayCountFraction = DayCountFractionHelper.Parse(legParametersRange.DayCount);
            // Initial stub
            //
            //if (paymentCalendar==null)
            //{
            //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, stream.paymentDates.paymentDatesAdjustments.businessCenters);
            //}
            //ProcessStubs(stream, legParametersRange, paymentCalendar);
            return stream;
        }

        #endregion

        #region Set schedules (fixed rate, spread, cap rate, floor rate, notional) methods 

        public static void SetFixedRateSchedule(InterestRateStream stream, Schedule fixedRateSchedule)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            XsdClassesFieldResolver.CalculationSetFixedRateSchedule(calculation, fixedRateSchedule);
        }

        public static void SetSpreadSchedule(InterestRateStream stream, Schedule spreadSchedule)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            var schedule = new SpreadSchedule {initialValue = spreadSchedule.initialValue, step = spreadSchedule.step};
            floatingRateCalculation.spreadSchedule = new[] { schedule };
        }

        public static void SetCapRateSchedule(InterestRateStream stream, Schedule capRateSchedule, bool isReceiverBuyer)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);           
            var schedule = new StrikeSchedule {initialValue = capRateSchedule.initialValue, step = capRateSchedule.step};
            floatingRateCalculation.capRateSchedule = new[] { schedule };
            if (isReceiverBuyer)
            {
                floatingRateCalculation.capRateSchedule[0].buyer = new IdentifiedPayerReceiver {Value = PayerReceiverEnum.Receiver};
                floatingRateCalculation.capRateSchedule[0].seller = new IdentifiedPayerReceiver {Value = PayerReceiverEnum.Payer};
            }
            else
            {
                floatingRateCalculation.capRateSchedule[0].buyer = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Payer };
                floatingRateCalculation.capRateSchedule[0].seller = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Receiver };
            }
        }

        public static void SetFloorRateSchedule(InterestRateStream stream, Schedule floorRateSchedule, bool isReceiverBuyer)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);
            var schedule = new StrikeSchedule
                               {
                                   initialValue = floorRateSchedule.initialValue,
                                   step = floorRateSchedule.step
                               };
            floatingRateCalculation.floorRateSchedule = new[] { schedule };
            if (isReceiverBuyer)
            {
                floatingRateCalculation.floorRateSchedule[0].buyer = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Receiver };
                floatingRateCalculation.floorRateSchedule[0].seller = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Payer };
            }
            else
            {
                floatingRateCalculation.floorRateSchedule[0].buyer = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Payer };
                floatingRateCalculation.floorRateSchedule[0].seller = new IdentifiedPayerReceiver { Value = PayerReceiverEnum.Receiver };
            }
        }

        //public static void SetNotionalSchedule(InterestRateStream stream, AmountSchedule amountSchedule)
        //{
        //    Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmount_GetCalculation(stream.calculationPeriodAmount);
        //    var notional = new Notional { notionalStepSchedule = amountSchedule };
        //    XsdClassesFieldResolver.Calculation_SetNotionalSchedule(calculation, notional);
        //}
        public static void SetNotionalSchedule(InterestRateStream stream, NonNegativeAmountSchedule amountSchedule)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            var notional = new Notional { notionalStepSchedule = amountSchedule };
            XsdClassesFieldResolver.CalculationSetNotionalSchedule(calculation, notional);
        }

        #endregion

        //private static void ProcessStubs(InterestRateStream stream, SwapLegParametersRange_Old swapTermParametersRange, IBusinessCalendar paymentCalendar)
        //{
        //    //bool isFullFirstCoupon = swapTermParametersRange.FirstCouponType == FirstCouponType.Full?true:false;
        //    DateTime adjustedEffectiveDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, XsdClassesFieldResolver.CalculationPeriodDates_GetEffectiveDate(stream.calculationPeriodDates));
        //    DateTime adjustedTerminationDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, XsdClassesFieldResolver.CalculationPeriodDates_GetTerminationDate(stream.calculationPeriodDates));
        //    StubPeriodTypeEnum? initialStubType = null;
        //    if (!string.IsNullOrEmpty(swapTermParametersRange.InitialStubType))
        //    {
        //        initialStubType = EnumHelper.Parse<StubPeriodTypeEnum>(swapTermParametersRange.InitialStubType);
        //    }
        //    StubPeriodTypeEnum? finalStubType = null;
        //    if (!string.IsNullOrEmpty(swapTermParametersRange.FinalStubType))
        //    {
        //        finalStubType = EnumHelper.Parse<StubPeriodTypeEnum>(swapTermParametersRange.FinalStubType);
        //    }
        //    //Only calculate a stub if required i.e.
        //    //If a FirsteRegularPeriod is specified -> Roll forward
        //    //If a lastRegularePeriod is specified --> Roll Backwards
        //    //If intialstub is not null
        //    //if finalStubType is not null
        //    //Otherwise, check if the maturity - effective date is modulo the frequency.
        //    //If yes, there is no stub.
        //    var periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(adjustedEffectiveDate,
        //                                                                                adjustedTerminationDate,
        //                                                                                swapTermParametersRange.FirstRegularPeriodStartDate,
        //                                                                                stream.calculationPeriodDates.calculationPeriodFrequency,
        //                                                                                stream.calculationPeriodDates.calculationPeriodDatesAdjustments,
        //                                                                                initialStubType,
        //                                                                                finalStubType,
        //                                                                                paymentCalendar);
        //    //var periods = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(adjustedEffectiveDate,
        //    //                                                                            adjustedTerminationDate,
        //    //                                                                            stream.calculationPeriodDates.firstRegularPeriodStartDate,
        //    //                                                                            stream.calculationPeriodDates.lastRegularPeriodEndDate,
        //    //                                                                            stream.calculationPeriodDates.calculationPeriodFrequency,
        //    //                                                                            stream.calculationPeriodDates.calculationPeriodDatesAdjustments,
        //    //                                                                            paymentCalendar);
        //    if (!stream.calculationPeriodDates.firstRegularPeriodStartDateSpecified && periods.HasInitialStub)
        //    {
        //        stream.calculationPeriodDates.firstRegularPeriodStartDate = periods.FirstRegularPeriodUnadjustedStartDate;
        //        stream.calculationPeriodDates.firstRegularPeriodStartDateSpecified = true;
        //    }
        //    if (!stream.calculationPeriodDates.lastRegularPeriodEndDateSpecified && periods.HasFinalStub)
        //    {
        //        stream.calculationPeriodDates.lastRegularPeriodEndDate = periods.LastRegularPeriodUnadjustedEndDate;
        //        stream.calculationPeriodDates.lastRegularPeriodEndDateSpecified = true;
        //    }
        //}

        private static void SetNotional(Calculation calculation, 
                                        decimal amount, 
                                        string currencyAsString)
        {
            Money notionalAsMoney = MoneyHelper.GetAmount(amount, currencyAsString);
            Notional notionalSchedule = NotionalFactory.Create(notionalAsMoney);
            XsdClassesFieldResolver.CalculationSetNotionalSchedule(calculation, notionalSchedule);
        }


        private static void SetEffectiveAndTerminationDates(InterestRateStream stream, 
                                                            DateTime rawEffectiveDate, 
                                                            DateTime rawTerminationDate, 
                                                            string terminationDateBusinessDayAdjustments,
                                                            string terminationDateBusinessDayCalendar)
        {
            AdjustableDate effectiveDate = DateTypesHelper.ToAdjustableDate(rawEffectiveDate);
            XsdClassesFieldResolver.CalculationPeriodDatesSetEffectiveDate(stream.calculationPeriodDates, effectiveDate);
            AdjustableDate terminationDate = DateTypesHelper.ToAdjustableDate(rawTerminationDate, terminationDateBusinessDayAdjustments, terminationDateBusinessDayCalendar);
            XsdClassesFieldResolver.CalculationPeriodDatesSetTerminationDate(stream.calculationPeriodDates, terminationDate);
        }

        private static void SetFirstRegularPeriodStartDate(InterestRateStream stream,
                                                           DateTime firstRegularPeriodStartDate)
        {
            if (firstRegularPeriodStartDate != new DateTime())
            {
                XsdClassesFieldResolver.CalculationPeriodDatesSetFirstRegularPeriodStartDate(
                    stream.calculationPeriodDates, firstRegularPeriodStartDate);
            }
        }

        private static void SetLastRegularPeriodEndDate(InterestRateStream stream,
                                                        DateTime lastRegularPeriodEndDate)
        {
            if (lastRegularPeriodEndDate != new DateTime())
            {
                XsdClassesFieldResolver.CalculationPeriodDatesSetLastRegularPeriodEndDate(
                    stream.calculationPeriodDates, lastRegularPeriodEndDate);
            }
        }

        private static void SetFirstPeriodStartDate(InterestRateStream stream,
                                                        AdjustableDate firstPeriodStartDate)
        {
            if (firstPeriodStartDate != null)
            {
                XsdClassesFieldResolver.CalculationPeriodDatesSetFirstPeriodStartDate(
                    stream.calculationPeriodDates, firstPeriodStartDate);
            }
        }
    }
}