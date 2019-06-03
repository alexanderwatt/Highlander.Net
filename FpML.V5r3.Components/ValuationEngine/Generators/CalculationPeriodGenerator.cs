#region Usings

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.ValuationEngine.Helpers;
using Orion.ModelFramework;

#endregion

namespace Orion.ValuationEngine.Generators
{
    public class CalculationPeriodGenerator
    {

        //public static CalculationPeriodsPrincipalExchangesAndStubs GenerateUnadjustedCalculationPeriods(
        //                                                    IBusinessCalendar paymentCalendar,
        //                                                    DateTime unadjustedStartDate,
        //                                                    DateTime unadjustedEndDate,
        //                                                    CalculationPeriodFrequency frequency,
        //                                                    StubPeriodTypeEnum? initialStubType,
        //                                                    StubPeriodTypeEnum? finalStubType
        //                                                )
        //{
        //    // If first roll date not specified - calculate it from the start date.
        //    //
        //    DateTime unadjustedFirstRollDate = AddPeriod(unadjustedStartDate, frequency, 1);
        //    return GenerateUnadjustedCalculationPeriods(unadjustedStartDate, unadjustedEndDate, unadjustedFirstRollDate, frequency, initialStubType, finalStubType);
        //}

        public static CalculationPeriodsPrincipalExchangesAndStubs GenerateAdjustedCalculationPeriods(
          
            DateTime unadjustedstartDate,
            DateTime unadjustedTerminationDate,
            DateTime? firstunadjustedRegularPeriodStartDate,
            DateTime? lastunadjustedRegularPeriodEndDate,
            CalculationPeriodFrequency frequency,
            BusinessDayAdjustments calculationPeriodDatesAdjustments,
            IBusinessCalendar paymentCalendar)
        {      
            var result = new CalculationPeriodsPrincipalExchangesAndStubs();
            //  Generate periods backwards (toward the unadjustedEffectiveDate)
            //
            GeneratePeriods(unadjustedstartDate, unadjustedTerminationDate, firstunadjustedRegularPeriodStartDate,
                            lastunadjustedRegularPeriodEndDate, frequency, calculationPeriodDatesAdjustments, result, paymentCalendar);
            result.CalculationPeriods.Sort((calculationPeriod1, calculationPeriod2) =>
                                           calculationPeriod1.adjustedEndDate.CompareTo(calculationPeriod2.adjustedEndDate));
            return result;
        }

        public static CalculationPeriodsPrincipalExchangesAndStubs GenerateAdjustedCalculationPeriods(
            DateTime unadjustedEffectiveDate, 
            DateTime unadjustedTerminationDate, 
            DateTime unadjustedFirstRollDate, //this one is still UN adjusted 
            CalculationPeriodFrequency frequency,
            BusinessDayAdjustments calculationPeriodDatesAdjustments,
            StubPeriodTypeEnum? initialStubType,
            StubPeriodTypeEnum? finalStubType,
            IBusinessCalendar paymentCalendar)
        {
            #region Param check

            if (initialStubType.HasValue)
            {
                if ((initialStubType != StubPeriodTypeEnum.LongInitial) && (initialStubType != StubPeriodTypeEnum.ShortInitial))
                {
                    throw new ArgumentOutOfRangeException("initialStubType", initialStubType, "Wrong stub type.");
                }
            }
            if (finalStubType.HasValue)
            {
                if ((finalStubType == StubPeriodTypeEnum.LongFinal) && (initialStubType == StubPeriodTypeEnum.ShortFinal))
                {
                    throw new ArgumentOutOfRangeException("finalStubType", finalStubType, "Wrong stub type.");
                }
            }
//            if (!RollConventionEnumHelper.IsAdjusted(frequency.rollConvention, unadjustedFirstRollDate))
//            {
//                string message = String.Format("First roll date {0}, should be pre-adjusted according to RollConvention {1}", unadjustedFirstRollDate, frequency.rollConvention);
//                throw new Exception(message);
//            }

            #endregion

            var result = new CalculationPeriodsPrincipalExchangesAndStubs();
            //  Start calculations from 'first roll date' and generate dates backwards and forwards
            //
            //  Generate periods backwards (toward the unadjustedEffectiveDate)
            //
            GeneratePeriodsBackward(unadjustedFirstRollDate, frequency, calculationPeriodDatesAdjustments, result, unadjustedEffectiveDate, initialStubType, paymentCalendar);
            //  Generate periods forwards (toward the unadjustedTerminationDate)
            //
            GeneratePeriodsForward(unadjustedFirstRollDate, frequency, calculationPeriodDatesAdjustments, result, unadjustedTerminationDate, finalStubType, paymentCalendar);
            return result;
        }

        public static void GeneratePeriods(DateTime unadjustedStartDate,
                                           DateTime unadjustedTerminationDate,
                                           DateTime? firstunadjustedRegularPeriodStartDate,
                                           DateTime? lastunadjustedRegularPeriodEndDate,
                                           //StubPeriodTypeEnum? initialStubType,
                                           //StubPeriodTypeEnum? finalStubType,
                                           CalculationPeriodFrequency frequency,
                                           BusinessDayAdjustments calculationPeriodDatesAdjustments,
                                           CalculationPeriodsPrincipalExchangesAndStubs result, 
                                           IBusinessCalendar paymentCalendar)
        {
            DateTime periodEndDate = unadjustedTerminationDate;
            DateTime periodStartDate;
            bool isInitialStub = firstunadjustedRegularPeriodStartDate != null;
            var startDate = unadjustedStartDate;
            if (isInitialStub)
            {
                startDate = (DateTime) firstunadjustedRegularPeriodStartDate;
            }
            var isFinalStub = lastunadjustedRegularPeriodEndDate != null;
            if (isFinalStub)
            {
                periodStartDate = (DateTime)lastunadjustedRegularPeriodEndDate;
            }
            else
            {
                periodStartDate = AddPeriod(periodEndDate, IntervalHelper.FromFrequency(frequency), -1);
            }
            var isFirstPeriod = true;
            do
            {
                var calculationPeriod = Create(periodStartDate, periodEndDate, frequency,
                                               calculationPeriodDatesAdjustments, paymentCalendar, isFirstPeriod);
                isFirstPeriod = false;
                //adjust for roll convention.
                periodStartDate = calculationPeriod.unadjustedStartDate;
                periodEndDate = calculationPeriod.unadjustedEndDate;
                if (isFinalStub)//Always at least one period.
                {
                    result.FinalStubCalculationPeriod = calculationPeriod;
                    periodEndDate = periodStartDate;
                    periodStartDate = AddPeriod(periodEndDate, IntervalHelper.FromFrequency(frequency), -1);
                    isFinalStub = false;
                }
                else if (calculationPeriod.unadjustedStartDate.Date >= startDate.Date)
                {
                    result.Add(calculationPeriod);
                    periodEndDate = periodStartDate;
                    periodStartDate = AddPeriod(periodEndDate, IntervalHelper.FromFrequency(frequency), -1);
                }
                else if (calculationPeriod.unadjustedStartDate.Date == startDate.Date)//first period - not stub
                {
                    result.Add(calculationPeriod);
                    break;
                }
                else if (calculationPeriod.unadjustedStartDate.Date < startDate.Date)//first period - not stub
                {
                    var number = result.CalculationPeriods.Count;
                    result.CalculationPeriods[number-1].unadjustedStartDate = startDate.Date;
                    result.CalculationPeriods[number-1].adjustedStartDate = startDate.Date;
                    break;
                }
                if(calculationPeriod.unadjustedEndDate.Date <= startDate.Date)//first period - not stub
                {
                    break;
                }
            } while (true);
            if (isInitialStub)//TODO -Fix the unadjustedstartdate which is affected by the roll convention.
            {
                var calculationPeriod = CreateStub(unadjustedStartDate, startDate.Date,
                                               calculationPeriodDatesAdjustments, paymentCalendar);
                result.InitialStubCalculationPeriod = calculationPeriod;
            }
        }

        private static void GeneratePeriodsForward(DateTime firstRollDate, CalculationPeriodFrequency frequency, 
                                                   BusinessDayAdjustments calculationPeriodDatesAdjustments,
                                                   CalculationPeriodsPrincipalExchangesAndStubs result, 
                                                   DateTime adjustedTerminationDate, StubPeriodTypeEnum? finalStubType,
                                                   IBusinessCalendar paymentCalendar)
        {
            DateTime periodStartDate = firstRollDate;
            DateTime periodEndDate   = AddPeriod(periodStartDate, IntervalHelper.FromFrequency(frequency), 1);
            bool encounteredShortFinalStub = false;
            //if (paymentCalendar == null)
            //{
            //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, calculationPeriodDatesAdjustments.businessCenters);
            //}
            do
            {
                var calculationPeriod = new CalculationPeriod();
                //  Apply ROLL CONVENTION (NOT A BUSINESS DAY CONVENTION!) to unadjusted period start and end dates.
                //
                DateTime rollConventionAdjustedPeriodStartDate = periodStartDate;
                DateTime rollConventionAdjustedPeriodEndDate = periodEndDate;
                var frequencyPeriod = EnumHelper.Parse<PeriodEnum>(frequency.period, true);
                if (frequencyPeriod != PeriodEnum.D) //adjust if the frequency is NOT expressed in days
                {
                    rollConventionAdjustedPeriodStartDate = RollConventionEnumHelper.AdjustDate(frequency.rollConvention, periodStartDate);
                    rollConventionAdjustedPeriodEndDate = RollConventionEnumHelper.AdjustDate(frequency.rollConvention, periodEndDate);
                }
                CalculationPeriodHelper.SetUnadjustedDates(calculationPeriod, rollConventionAdjustedPeriodStartDate, rollConventionAdjustedPeriodEndDate);
                //  Set adjusted period dates
                //
                DateTime adjustedPeriodStartDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, rollConventionAdjustedPeriodStartDate, calculationPeriodDatesAdjustments);
                DateTime adjustedPeriodEndDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, rollConventionAdjustedPeriodEndDate, calculationPeriodDatesAdjustments);
                CalculationPeriodHelper.SetAdjustedDates(calculationPeriod, adjustedPeriodStartDate, adjustedPeriodEndDate);
                //if (calculationPeriod.unadjustedEndDate < adjustedTerminationDate)
                if (calculationPeriod.adjustedEndDate < adjustedTerminationDate)
                {
                    result.Add(calculationPeriod);
                    periodStartDate = periodEndDate;
                    periodEndDate = AddPeriod(periodStartDate, IntervalHelper.FromFrequency(frequency), 1);
                }
                //else if (calculationPeriod.unadjustedEndDate == adjustedTerminationDate)//last period - not a stub
                else if (calculationPeriod.adjustedEndDate == adjustedTerminationDate)//last period - not a stub
                {
                    result.Add(calculationPeriod);
                    break;
                }
                else//last period - short stub (merge with previous period if long stub specified)
                {
                    encounteredShortFinalStub = true;
                    //calculationPeriod.unadjustedEndDate = adjustedTerminationDate;
                    calculationPeriod.adjustedEndDate = adjustedTerminationDate;
                    result.FinalStubCalculationPeriod = calculationPeriod;
                    break;
                }
            } while (true);
            if (encounteredShortFinalStub && finalStubType == StubPeriodTypeEnum.LongFinal)
            {
                result.CreateLongFinalStub();
            }
        }

        private static void GeneratePeriodsBackward(DateTime firstRollDate, CalculationPeriodFrequency frequency,
                                                    BusinessDayAdjustments calculationPeriodDatesAdjustments,
                                                    CalculationPeriodsPrincipalExchangesAndStubs result, 
                                                    DateTime adjustedEffectiveDate, StubPeriodTypeEnum? initialStubType
                                                    , IBusinessCalendar paymentCalendar)
        {
            DateTime periodEndDate = firstRollDate;
            DateTime periodStartDate = AddPeriod(periodEndDate, IntervalHelper.FromFrequency(frequency), -1);
            bool encounteredShortInitialStub = false;
            //if (paymentCalendar == null)
            //{
            //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, calculationPeriodDatesAdjustments.businessCenters);
            //}
            do
            {
                var calculationPeriod = new CalculationPeriod();
                //  Apply ROLL CONVENTION (NOT A BUSINESS DAY CONVENTION!) to unadjusted period start and end dates.
                //
                DateTime rollConventionAdjustedPeriodStartDate = periodStartDate;
                DateTime rollConventionAdjustedPeriodEndDate = periodEndDate;
                var frequencyPeriod = EnumHelper.Parse<PeriodEnum>(frequency.period, true);
                if (frequencyPeriod != PeriodEnum.D)//adjust if the frequency is NOT expressed in days
                {
                    rollConventionAdjustedPeriodStartDate = RollConventionEnumHelper.AdjustDate(frequency.rollConvention, periodStartDate);
                    rollConventionAdjustedPeriodEndDate = RollConventionEnumHelper.AdjustDate(frequency.rollConvention, periodEndDate);
                }
                CalculationPeriodHelper.SetUnadjustedDates(calculationPeriod, rollConventionAdjustedPeriodStartDate, rollConventionAdjustedPeriodEndDate);
                //  Set adjusted period dates
                //
                DateTime adjustedPeriodStartDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, rollConventionAdjustedPeriodStartDate, calculationPeriodDatesAdjustments);
                DateTime adjustedPeriodEndDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, rollConventionAdjustedPeriodEndDate, calculationPeriodDatesAdjustments);
                CalculationPeriodHelper.SetAdjustedDates(calculationPeriod, adjustedPeriodStartDate, adjustedPeriodEndDate);
                //if (calculationPeriod.unadjustedStartDate > adjustedEffectiveDate)
                if (calculationPeriod.adjustedStartDate > adjustedEffectiveDate)
                {
                    result.InsertFirst(calculationPeriod);
                    periodEndDate = periodStartDate;
                    periodStartDate = AddPeriod(periodEndDate, IntervalHelper.FromFrequency(frequency), -1);
                }
                //else if (calculationPeriod.unadjustedStartDate == adjustedEffectiveDate)//first period - not stub
                else if (calculationPeriod.adjustedStartDate == adjustedEffectiveDate)//first period - not stub
                {
                    result.InsertFirst(calculationPeriod);
                    break;
                }
                else//first period - short stub (merge with next period if a long stub specified)
                {
                    encounteredShortInitialStub = true;
                    //calculationPeriod.unadjustedStartDate = adjustedEffectiveDate;
                    calculationPeriod.adjustedStartDate = adjustedEffectiveDate;
                    result.InitialStubCalculationPeriod = calculationPeriod;
                    break;
                }
            } while (true);
            if (encounteredShortInitialStub && initialStubType == StubPeriodTypeEnum.LongInitial)
            {
                result.CreateLongInitialStub();
            }
        }

        public static DateTime AddPeriod(DateTime date, Period interval, int numberOfIntervals)
        {
            DateTime result = interval.Multiply(numberOfIntervals).Add(date);
            return result;
        }

        private static CalculationPeriod Create(DateTime periodStartDate, DateTime periodEndDate
            , CalculationPeriodFrequency frequency, BusinessDayAdjustments dateAdjustments
            , IBusinessCalendar paymentCalendar, bool isFirstPeriodCreated)
        {
            var calculationPeriod = new CalculationPeriod();
            //  Apply ROLL CONVENTION (NOT A BUSINESS DAY CONVENTION!) to unadjusted period start and end dates.
            //
            DateTime rollConventionPeriodStartDate = periodStartDate;
            DateTime rollConventionPeriodEndDate = periodEndDate;
            var frequencyPeriod = EnumHelper.Parse<PeriodEnum>(frequency.period, true);
            if (frequencyPeriod != PeriodEnum.D)//adjust if the frequency is NOT expressed in days
            {
                rollConventionPeriodStartDate = RollConventionEnumHelper.AdjustDate(frequency.rollConvention, periodStartDate);
                rollConventionPeriodEndDate = RollConventionEnumHelper.AdjustDate(frequency.rollConvention, periodEndDate);
            }
            CalculationPeriodHelper.SetUnadjustedDates(calculationPeriod, rollConventionPeriodStartDate, rollConventionPeriodEndDate);
            //  Set adjusted period dates
            //
            DateTime adjustedPeriodStartDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, rollConventionPeriodStartDate, dateAdjustments);
            DateTime adjustedPeriodEndDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, rollConventionPeriodEndDate, dateAdjustments);
            if (isFirstPeriodCreated)
            {

                adjustedPeriodEndDate = periodEndDate;
            }
            CalculationPeriodHelper.SetAdjustedDates(calculationPeriod, adjustedPeriodStartDate, adjustedPeriodEndDate);
            return calculationPeriod;
        }

        private static CalculationPeriod CreateStub(DateTime periodStartDate, DateTime periodEndDate
            , BusinessDayAdjustments dateAdjustments, IBusinessCalendar paymentCalendar)
        {
            var calculationPeriod = new CalculationPeriod();
            //  Apply ROLL CONVENTION (NOT A BUSINESS DAY CONVENTION!) to unadjusted period start and end dates.
            //
            CalculationPeriodHelper.SetUnadjustedDates(calculationPeriod, periodStartDate, periodEndDate);
            //  Set adjusted period dates
            //
            DateTime adjustedPeriodStartDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, periodStartDate, dateAdjustments);
            DateTime adjustedPeriodEndDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, periodEndDate, dateAdjustments);
            CalculationPeriodHelper.SetAdjustedDates(calculationPeriod, adjustedPeriodStartDate, adjustedPeriodEndDate);
            return calculationPeriod;
        }
    }
}