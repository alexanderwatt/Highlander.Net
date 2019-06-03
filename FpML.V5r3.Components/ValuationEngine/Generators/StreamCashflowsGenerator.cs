#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using FpML.V5r3.Reporting;
using Orion.ValuationEngine.Helpers;
using Orion.ModelFramework;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Generators
{
    public class StreamCashflowsGenerator
    {
        #region Private functions

        private static IEnumerable<List<CalculationPeriod>> SplitCalculationPeriodsByPaymentPeriods(CalculationPeriodsPrincipalExchangesAndStubs calculationPeriods, int calculationPeriodsInPaymentPeriod)
        {
            var result = new List<List<CalculationPeriod>>();
            if (0 != calculationPeriods.CalculationPeriods.Count % calculationPeriodsInPaymentPeriod)
            {
                throw new System.Exception(String.Format("Invalid number of calculation periods {0}. Calculation periods per payment period : {1}", calculationPeriods.CalculationPeriods.Count, calculationPeriodsInPaymentPeriod));
            }
            int calculationPeriodIndex = 0;
            while (calculationPeriodIndex < calculationPeriods.CalculationPeriods.Count)
            {
                List<CalculationPeriod> item = calculationPeriods.CalculationPeriods.GetRange(calculationPeriodIndex, calculationPeriodsInPaymentPeriod);               
                result.Add(item);
                calculationPeriodIndex += calculationPeriodsInPaymentPeriod;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interestRateStream"></param>
        /// <param name="listCalculationPeriods"></param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static List<PaymentCalculationPeriod> GetPaymentCalculationPeriods(InterestRateStream interestRateStream, 
                                                                                  CalculationPeriodsPrincipalExchangesAndStubs listCalculationPeriods,
                                                                                  IBusinessCalendar paymentCalendar)
        {
            var result = new List<PaymentCalculationPeriod>();
            // Calculate how much calculation periods comprise one payment calculation period
            //
            PaymentDates paymentDates = interestRateStream.paymentDates;
            Period paymentFrequency = IntervalHelper.FromFrequency(paymentDates.paymentFrequency);
            Period calculationPeriodFrequency = IntervalHelper.FromFrequency(interestRateStream.calculationPeriodDates.calculationPeriodFrequency);
            //  If the payment frequency is less frequent than the frequency defined in the calculation period dates component 
            //then more than one calculation period will contribute to e payment amount.            
            //  A payment frequency more frequent than the calculation period frequency 
            //or one that is not a multiple of the calculation period frequency is invalid.
            if (paymentFrequency.period != calculationPeriodFrequency.period)
            {
                throw new NotSupportedException(String.Format("Payment period type ({0}) and calculation period type ({1}) are different. This is not supported.", paymentFrequency.period, calculationPeriodFrequency.period));
            }
            if (0 != int.Parse(paymentFrequency.periodMultiplier) % int.Parse(calculationPeriodFrequency.periodMultiplier))
            {
                throw new NotSupportedException(String.Format("Payment period frequency  is not a multiple of the calculation period frequency. This is not supported."));
            }
            if (int.Parse(paymentFrequency.periodMultiplier) < int.Parse(calculationPeriodFrequency.periodMultiplier))
            {
                throw new NotSupportedException(String.Format("A payment frequency more frequent than the calculation period frequency. This is not supported."));
            }
            int calculationPeriodsInPaymentPeriod = int.Parse(paymentFrequency.periodMultiplier) / int.Parse(calculationPeriodFrequency.periodMultiplier);
            IEnumerable<List<CalculationPeriod>> listOfCalculationPeriodsInSinglePayPeriod = SplitCalculationPeriodsByPaymentPeriods(listCalculationPeriods, calculationPeriodsInPaymentPeriod);
            Offset paymentDaysOffset = paymentDates.paymentDaysOffset;
            BusinessDayAdjustments paymentDatesBusinessDayAdjustments = paymentDates.paymentDatesAdjustments;
            PayRelativeToEnum payRelativeTo = paymentDates.payRelativeTo;
            //if (paymentCalendar == null)
            //{
            //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, paymentDatesBusinessDayAdjustments.businessCenters);
            //}
            // Initial stub
            //
            if (listCalculationPeriods.HasInitialStub)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod
                                                   {
                                                       adjustedPaymentDate =
                                                           CalculatePaymentDate(paymentDates, payRelativeTo,
                                                                                new List<CalculationPeriod>(
                                                                                    new[]
                                                                                        {
                                                                                            listCalculationPeriods.
                                                                                                InitialStubCalculationPeriod
                                                                                        }), paymentDaysOffset,
                                                                                paymentDatesBusinessDayAdjustments, paymentCalendar),
                                                       adjustedPaymentDateSpecified = true
                                                   };
                XsdClassesFieldResolver.SetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod, new[] { listCalculationPeriods.InitialStubCalculationPeriod });                
                result.Add(paymentCalculationPeriod);
            }
            //  Regular periods
            //
            foreach (List<CalculationPeriod> calculationPeriodsInPamentPeriod in listOfCalculationPeriodsInSinglePayPeriod)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod
                                                   {
                                                       adjustedPaymentDate =
                                                           CalculatePaymentDate(paymentDates, payRelativeTo,
                                                                                calculationPeriodsInPamentPeriod,
                                                                                paymentDaysOffset,
                                                                                paymentDatesBusinessDayAdjustments, paymentCalendar),
                                                       adjustedPaymentDateSpecified = true
                                                   };
                XsdClassesFieldResolver.SetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod, calculationPeriodsInPamentPeriod.ToArray());
                result.Add(paymentCalculationPeriod);
            }
            // Final stub
            //
            if (listCalculationPeriods.HasFinalStub)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod
                                                   {
                                                       adjustedPaymentDate =
                                                           CalculatePaymentDate(paymentDates, payRelativeTo,
                                                                                new List<CalculationPeriod>(
                                                                                    new[]
                                                                                        {
                                                                                            listCalculationPeriods.
                                                                                                FinalStubCalculationPeriod
                                                                                        }), paymentDaysOffset,
                                                                                paymentDatesBusinessDayAdjustments, paymentCalendar),
                                                       adjustedPaymentDateSpecified = true
                                                   };
                XsdClassesFieldResolver.SetPaymentCalculationPeriodCalculationPeriodArray(paymentCalculationPeriod, new[] { listCalculationPeriods.FinalStubCalculationPeriod });
                result.Add(paymentCalculationPeriod);
            }
            return result;
        }

        private static DateTime CalculatePaymentDate(PaymentDates paymentDates, 
                                                     PayRelativeToEnum payRelativeTo, 
                                                     IList<CalculationPeriod> calculationPeriodsInPamentPeriod, 
                                                     Offset paymentDaysOffset, 
                                                     BusinessDayAdjustments paymentDatesBusinessDayAdjustments,
                                                     IBusinessCalendar paymentCalendar)
        {
            switch (payRelativeTo)
            {
                case PayRelativeToEnum.CalculationPeriodStartDate:
                    {
                        // To get the calculation period start date - obtain a reference to FIRST calculation period in the payment period.
                        //
                        CalculationPeriod firstCalculatinPeriodInPaymentPeriod = calculationPeriodsInPamentPeriod[0];
                        BusinessDayAdjustments paymentDatesAdjustments = paymentDates.paymentDatesAdjustments;
                        // Adjust using paymentDatesAdjustments...
                        //
                        DateTime adjustedPaymentDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, firstCalculatinPeriodInPaymentPeriod.unadjustedStartDate, paymentDatesAdjustments);
                        // Apply offset 
                        //
                        if (null != paymentDaysOffset)
                        {
                            return AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustedPaymentDate, paymentDatesBusinessDayAdjustments, paymentDaysOffset);
                        }
                        return adjustedPaymentDate;
                    }
                case PayRelativeToEnum.CalculationPeriodEndDate:
                    {
                        // To get the calculation period end date - obtain a reference to LAST calculation period in the payment period.
                        //
                        CalculationPeriod lastCalculatinPeriodInPaymentPeriod = calculationPeriodsInPamentPeriod[calculationPeriodsInPamentPeriod.Count - 1];
                        BusinessDayAdjustments paymentDatesAdjustments = paymentDates.paymentDatesAdjustments;
                        // Adjust using paymentDatesAdjustments...
                        //
                        DateTime adjustedPaymentDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, lastCalculatinPeriodInPaymentPeriod.unadjustedEndDate, paymentDatesAdjustments);
                        // Apply offset (if present)
                        //
                        return null != paymentDaysOffset ? AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustedPaymentDate, paymentDatesBusinessDayAdjustments, paymentDaysOffset) : adjustedPaymentDate;
                    }
                case PayRelativeToEnum.ResetDate:
                    {
                        throw new NotImplementedException("payRelativeTo to ResetDate");
                    }
                default:
                    {
                        throw new NotImplementedException("payRelativeTo");
                    }
            }
        }

        public static CalculationPeriodsPrincipalExchangesAndStubs GenerateCalculationPeriodsPrincipalExchangesAndStubs(
            InterestRateStream interestRateStream, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            CalculationPeriodDates calculationPeriodDates = interestRateStream.calculationPeriodDates;
            AdjustableDate adjustableEffectiveDate = XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(calculationPeriodDates);
            AdjustableDate adjustableTerminationDate = XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(calculationPeriodDates);
            AdjustableDate adjustableFirstPeriodDate = adjustableEffectiveDate;            
            DateTime? firstRegularPeriodStartDate =
                XsdClassesFieldResolver.CalculationPeriodDatesGetFirstRegularPeriodStartDate(calculationPeriodDates);
            var tempDate = XsdClassesFieldResolver.CalculationPeriodDatesGetFirstPeriodStartDate(calculationPeriodDates);
            if (tempDate != null && firstRegularPeriodStartDate != null)
            {
                adjustableFirstPeriodDate = tempDate;
                Frequency frequency = calculationPeriodDates.calculationPeriodFrequency;
                var startDate  =  CalculationPeriodGenerator.AddPeriod((DateTime)firstRegularPeriodStartDate, IntervalHelper.FromFrequency(frequency), -1);
                adjustableFirstPeriodDate.unadjustedDate = IdentifiedDateHelper.Create(startDate);
            }
            DateTime? lastRegularPeriodEndDate =
                XsdClassesFieldResolver.CalculationPeriodDatesGetLastRegularPeriodEndDate(calculationPeriodDates);
            //            This assumes automatic adjustment of calculationperiods.
            CalculationPeriodsPrincipalExchangesAndStubs result = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                                                                                                                                adjustableFirstPeriodDate.unadjustedDate.Value,
                                                                                                                                adjustableTerminationDate.unadjustedDate.Value,
                                                                                                                                firstRegularPeriodStartDate,
                                                                                                                                lastRegularPeriodEndDate,
                                                                                                                                calculationPeriodDates.calculationPeriodFrequency,
                                                                                                                                calculationPeriodDates.calculationPeriodDatesAdjustments,
                                                                                                                                paymentCalendar);
            //Determine whether the reset dates must be calcuated.
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(interestRateStream.calculationPeriodAmount);
            //  Add principle exchanges if this need is defined in parametric representation of the interest rate steam.
            //
            if (null != interestRateStream.principalExchanges)
            {
                //if (paymentCalendar == null)
                //{
                //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableEffectiveDate.dateAdjustments.businessCenters);
                //}
                //  Initial PE
                //
                if (interestRateStream.principalExchanges.initialExchange)
                {
                    PrincipalExchange initialExchange = PrincipalExchangeHelper.Create(AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustableEffectiveDate));
                    result.InitialPrincipalExchange = initialExchange;
                }
                //  intermediatory PE
                //
                if (interestRateStream.principalExchanges.intermediateExchange)
                {
                    // Generate a list of intermediatory PE exchanges 
                    //
                    Notional notionalSchedule = XsdClassesFieldResolver.CalculationGetNotionalSchedule(calculation);
                    if (null != notionalSchedule.notionalStepSchedule.step)//there should be steps - otherwise NO interm. exchanges.
                    {
                        foreach (DateTime stepDate in ScheduleHelper.GetStepDates(notionalSchedule.notionalStepSchedule))
                        {
                            PrincipalExchange intermediatoryExchange = PrincipalExchangeHelper.Create(stepDate);
                            result.Add(intermediatoryExchange);
                        }
                    }
                }
                //  Final PE
                // Assume the same calendar is used for the termination date as well!
                if (interestRateStream.principalExchanges.finalExchange)
                {
                    PrincipalExchange finalExchange = PrincipalExchangeHelper.Create(AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustableTerminationDate));
                    result.FinalPrincipalExchange = finalExchange;
                }
            }
            //Only does upfront resetRelativeTo start date.
            if (interestRateStream.resetDates != null && calculation.Items[0].GetType() == typeof(FloatingRateCalculation))
            {
                //Get the fixing date convention.
                var fixingDateConvention = interestRateStream.resetDates.resetDatesAdjustments;
                //if (fixingCalendar == null)
                //{
                //    fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixingDateConvention.businessCenters);
                //}
                foreach (var calculationPeriod in result.CalculationPeriods)
                {
                    if (calculationPeriod.adjustedStartDateSpecified)
                    {
                        //Set the adjusted fixing date.
                        var adjustedFixingDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, calculationPeriod.adjustedStartDate,
                                                                                fixingDateConvention);
                        var floatingRateDefinition = new FloatingRateDefinition();
                        var rateObservation = new RateObservation
                                                  {
                                                      observedRateSpecified = false,
                                                      adjustedFixingDateSpecified = true,
                                                      adjustedFixingDate = adjustedFixingDate
                                                  };
                        floatingRateDefinition.rateObservation = new[] { rateObservation };
                        calculationPeriod.Item1 = floatingRateDefinition;
                    }
                }
                //The initial stub period.
                if (result.InitialStubCalculationPeriod != null)
                {
                    if (result.InitialStubCalculationPeriod.adjustedStartDateSpecified)
                    {
                        //Set the adjusted fixing date.
                        var adjustedFixingDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, result.InitialStubCalculationPeriod.adjustedStartDate,
                                                                                fixingDateConvention);
                        var floatingRateDefinition = new FloatingRateDefinition();
                        var rateObservation = new RateObservation
                                                  {
                                                      observedRateSpecified = false,
                                                      adjustedFixingDateSpecified = true,
                                                      adjustedFixingDate = adjustedFixingDate
                                                  };
                        floatingRateDefinition.rateObservation = new[] { rateObservation };
                        result.InitialStubCalculationPeriod.Item1 = floatingRateDefinition;
                    }
                }
                //The final stub period
                if (result.FinalStubCalculationPeriod != null)
                {
                    if (result.FinalStubCalculationPeriod.adjustedStartDateSpecified)
                    {
                        //Set the adjusted fixing date.
                        var adjustedFixingDate = AdjustedDateHelper.ToAdjustedDate(fixingCalendar, result.FinalStubCalculationPeriod.adjustedStartDate,
                                                                                fixingDateConvention);
                        var floatingRateDefinition = new FloatingRateDefinition();
                        var rateObservation = new RateObservation
                                                  {
                                                      observedRateSpecified = false,
                                                      adjustedFixingDateSpecified = true,
                                                      adjustedFixingDate = adjustedFixingDate
                                                  };
                        floatingRateDefinition.rateObservation = new[] { rateObservation };
                        result.FinalStubCalculationPeriod.Item1 = floatingRateDefinition;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Don't generate stubs?
        /// </summary>
        /// <param name="interestRateStream"></param>
        /// <param name="rollDates">from #1st roll date to last roll date (last roll dates is effectively the end of the swap)</param>
        /// <param name="paymentCalendar"></param>
        /// <returns></returns>
        public static CalculationPeriodsPrincipalExchangesAndStubs GenerateCalculationPeriodsPrincipalExchangesAndStubsFromRollDates(InterestRateStream interestRateStream,
                                                                                                                                     List<DateTime> rollDates, 
                                                                                                                                     IBusinessCalendar paymentCalendar)
        {
            CalculationPeriodDates calculationPeriodDates = interestRateStream.calculationPeriodDates;            
            AdjustableDate adjustableEffectiveDate = XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(calculationPeriodDates);
            var result = new CalculationPeriodsPrincipalExchangesAndStubs();
            for(int rollDateIndex = 0; rollDateIndex < rollDates.Count - 1; ++rollDateIndex)
            {
                DateTime startOfThePeriod = rollDates[rollDateIndex];
                DateTime endOfThePeriod = rollDates[rollDateIndex + 1];
                var calculationPeriod = new CalculationPeriod();
                //  Set adjusted period dates
                //
                CalculationPeriodHelper.SetAdjustedDates(calculationPeriod,
                                                         startOfThePeriod,
                                                         endOfThePeriod);
                result.Add(calculationPeriod);
            }             
            //  Add principle exchanges if this need is defined in parametric representation of the interest rate steam.
            //
            if (null != interestRateStream.principalExchanges)
            {
                //  Initial PE
                //
                if (interestRateStream.principalExchanges.initialExchange)
                {
                    //if (paymentCalendar == null)
                    //{
                    //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableEffectiveDate.dateAdjustments.businessCenters);
                    //}
                    PrincipalExchange initialExchange = PrincipalExchangeHelper.Create(AdjustedDateHelper.ToAdjustedDate(paymentCalendar, adjustableEffectiveDate));
                    result.InitialPrincipalExchange = initialExchange;
                }
                //  intermediatory PE
                //
                if (interestRateStream.principalExchanges.intermediateExchange)
                {
                    // Generate a list of intermediatory PE exchanges 
                    //
                    Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(interestRateStream.calculationPeriodAmount);
                    Notional notionalSchedule = XsdClassesFieldResolver.CalculationGetNotionalSchedule(calculation);
                    if (null != notionalSchedule.notionalStepSchedule.step)//there should be steps - otherwise NO interm. exchanges.
                    {
                        foreach (DateTime stepDate in ScheduleHelper.GetStepDates(notionalSchedule.notionalStepSchedule))
                        {
                            PrincipalExchange intermediatoryExchange = PrincipalExchangeHelper.Create(stepDate);
                            result.Add(intermediatoryExchange);
                        }
                    }
                }
                //AdjustableDate adjustableTerminationDate = XsdClassesFieldResolver.CalculationPeriodDates_GetTerminationDate(calculationPeriodDates);
                DateTime lastRollDate = rollDates[rollDates.Count - 1];
                //  Final PE
                //
                if (interestRateStream.principalExchanges.finalExchange)
                {
                    //PrincipalExchange finalExchange = PrincipalExchangeHelper.Create(DateTypesHelper.ToAdjustedDate(adjustableTerminationDate));
                    PrincipalExchange finalExchange = PrincipalExchangeHelper.Create(lastRollDate);
                    result.FinalPrincipalExchange = finalExchange;
                }
            }            
            return result;
        }

        /// <summary>
        /// Update calculation periods with rates/notional.
        /// </summary>
        /// <param name="interestRateStream"></param>
        /// <param name="calculationPeriodsPrincipalExchangesAndStubs"></param>
        public static void UpdateCalculationPeriodsData(InterestRateStream interestRateStream, CalculationPeriodsPrincipalExchangesAndStubs calculationPeriodsPrincipalExchangesAndStubs)
        {
            Calculation calculation = XsdClassesFieldResolver.CalculationPeriodAmountGetCalculation(interestRateStream.calculationPeriodAmount);
            Notional notionalSchedule = XsdClassesFieldResolver.CalculationGetNotionalSchedule(calculation);

            #region Generate FUTURE amounts for principle exchanges

            //  Initial PE
            //
            if (null != calculationPeriodsPrincipalExchangesAndStubs.InitialPrincipalExchange)
            {
                //  initial OUTflow
                //
                decimal initialNotionalValue = ScheduleHelper.GetValue(notionalSchedule.notionalStepSchedule, calculationPeriodsPrincipalExchangesAndStubs.InitialPrincipalExchange.adjustedPrincipalExchangeDate);
                calculationPeriodsPrincipalExchangesAndStubs.InitialPrincipalExchange.principalExchangeAmount = -initialNotionalValue;
                calculationPeriodsPrincipalExchangesAndStubs.InitialPrincipalExchange.principalExchangeAmountSpecified = true;
            }
            //  intermediatory PE
            //
            foreach (PrincipalExchange intermediatoryExchange in calculationPeriodsPrincipalExchangesAndStubs.IntermediatePrincipalExchanges)
            {
                DateTime principleExchangeDate = intermediatoryExchange.adjustedPrincipalExchangeDate; 
                //value at the day before principle exchange day
                //
                decimal prevNotionalValue = ScheduleHelper.GetValue(notionalSchedule.notionalStepSchedule, principleExchangeDate.AddDays(-1));
                //value at principle exchange day ..
                //
                decimal newNotionalValue = ScheduleHelper.GetValue(notionalSchedule.notionalStepSchedule, principleExchangeDate);
                decimal principalExchangeAmount = prevNotionalValue - newNotionalValue;
                intermediatoryExchange.principalExchangeAmount = principalExchangeAmount;
                intermediatoryExchange.principalExchangeAmountSpecified = true;
            }
            //  Final PE
            //
            if (null != calculationPeriodsPrincipalExchangesAndStubs.FinalPrincipalExchange)
            {
                // pay the rest ot the notional back
                //
                decimal newNotionalValue = ScheduleHelper.GetValue(notionalSchedule.notionalStepSchedule, 
                                                                   calculationPeriodsPrincipalExchangesAndStubs.FinalPrincipalExchange.adjustedPrincipalExchangeDate);
                calculationPeriodsPrincipalExchangesAndStubs.FinalPrincipalExchange.principalExchangeAmount = newNotionalValue;
                calculationPeriodsPrincipalExchangesAndStubs.FinalPrincipalExchange.principalExchangeAmountSpecified = true;
            }

            #endregion

            //  Process standard calculation periods
            //
            foreach(CalculationPeriod calculationPeriod in calculationPeriodsPrincipalExchangesAndStubs.CalculationPeriods)
            {
                UpdateCalculationPeriodData(calculation, calculationPeriod, notionalSchedule);
            }
            // Process stub calculation periods
            //
            if (null != interestRateStream.stubCalculationPeriodAmount)
            {
                if ((null == interestRateStream.stubCalculationPeriodAmount.initialStub) &&
                    (null == interestRateStream.stubCalculationPeriodAmount.finalStub))
                {
                        throw new System.Exception(
                            "interestRateStream.stubCalculationPeriodAmount.initialStub && interestRateStream.stubCalculationPeriodAmount.finalStub are null");
                }
                StubValue initialStub = interestRateStream.stubCalculationPeriodAmount.initialStub;
                StubValue finalStub = interestRateStream.stubCalculationPeriodAmount.finalStub;
                if (null != calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod && null != initialStub)
                {
                    UpdateStubCalculationPeriodData(interestRateStream, calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod, initialStub, notionalSchedule);
                }
                if (null != calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod && null != finalStub)
                {
                    UpdateStubCalculationPeriodData(interestRateStream, calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod, finalStub, notionalSchedule);
                }                 
            }
            else
            {
                foreach (CalculationPeriod calculationPeriod in new[] { calculationPeriodsPrincipalExchangesAndStubs.InitialStubCalculationPeriod, calculationPeriodsPrincipalExchangesAndStubs.FinalStubCalculationPeriod })
                {
                    if (null != calculationPeriod)
                    {
                        UpdateCalculationPeriodData(calculation, calculationPeriod, notionalSchedule);
                    }
                }
            }
        }

        private static void UpdateStubCalculationPeriodData(InterestRateStream interestRateStream, CalculationPeriod stubCalculationPeriod, StubValue stubValue, Notional notinalSchedule)
        {
            StubCalculationPeriodAmountHelper.UpdateStubCalculationPeriod(interestRateStream, stubCalculationPeriod, stubValue);
            decimal notional = NotionalHelper.GetNotionalValue(notinalSchedule, stubCalculationPeriod.adjustedStartDate);
            // Notinal amount
            //
            XsdClassesFieldResolver.CalculationPeriodSetNotionalAmount(stubCalculationPeriod, notional);
        }

        private static void UpdateCalculationPeriodData(Calculation calculation, CalculationPeriod calculationPeriod, Notional notinalSchedule)
        {
            bool hasFloatingRateCalculation = XsdClassesFieldResolver.CalculationHasFloatingRateCalculation(calculation);
            bool hasFixedRate = XsdClassesFieldResolver.CalculationHasFixedRateSchedule(calculation);
            if (!(hasFloatingRateCalculation ^ hasFixedRate))
            {
                throw new System.Exception("at least one type of rate (floating or fixed) must be specified.");
            }            
            decimal notional = NotionalHelper.GetNotionalValue(notinalSchedule, calculationPeriod.adjustedStartDate);
            // Notional amount
            //
            XsdClassesFieldResolver.CalculationPeriodSetNotionalAmount(calculationPeriod, notional);
            // Fixed rate
            //
            if (hasFixedRate)
            {
                Schedule fixedRateSchedule = XsdClassesFieldResolver.CalculationGetFixedRateSchedule(calculation);
                decimal fixedRate = ScheduleHelper.GetValue(fixedRateSchedule, calculationPeriod.adjustedStartDate);
                XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriod, fixedRate);
            }
                //  Floating rate
                //
            else// if (hasFloatingRateCalculation)
            {
                // no observed, no calculated rate, spread == 0.0
                //
                var floatingRateDefinition = (FloatingRateDefinition)calculationPeriod.Item1 ?? new FloatingRateDefinition();
                //  check if stream has spreadSchedule
                //
                if (XsdClassesFieldResolver.CalculationHasSpreadSchedule(calculation))
                {
                    Schedule spreadSchedule = XsdClassesFieldResolver.GetCalculationSpreadSchedule(calculation);
                    decimal spread = ScheduleHelper.GetValue(spreadSchedule, calculationPeriod.adjustedStartDate);
                    floatingRateDefinition.spread = spread;
                    floatingRateDefinition.spreadSpecified = true;
                }
                FloatingRateCalculation floatingRateCalculation = XsdClassesFieldResolver.CalculationGetFloatingRateCalculation(calculation);               
                //  Check if there's a capRateSchedule
                //
                if (null != floatingRateCalculation.capRateSchedule)
                {
                    StrikeSchedule capRateSchedule = floatingRateCalculation.capRateSchedule[0];
                    var capStrike = new Strike();
                    var capRate = ScheduleHelper.GetValue(capRateSchedule, calculationPeriod.adjustedStartDate);
                    capStrike.strikeRate = capRate;
                    floatingRateDefinition.capRate = new[] { capStrike };
                }
                //  Check if there's a capRateSchedule
                //
                if (null != floatingRateCalculation.floorRateSchedule)
                {
                    StrikeSchedule floorRateSchedule = floatingRateCalculation.floorRateSchedule[0];
                    var floorStrike = new Strike();
                    decimal floorRate = ScheduleHelper.GetValue(floorRateSchedule, calculationPeriod.adjustedStartDate);
                    floorStrike.strikeRate = floorRate;
                    floatingRateDefinition.floorRate = new[] { floorStrike };
                }
                calculationPeriod.Item1 = floatingRateDefinition;
            }
        }
    }
}