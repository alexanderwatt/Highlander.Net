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
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Schedulers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.ModelFramework;
using Math = System.Math;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    #region Helper Classes

    public class BillSwapPricer2CashflowItem
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public double AccrualPeriod;
        public double ImpliedForwardRate;
        public double Notional;
        public double DiscountedValue;//this is a future (projected in case of float leg) value. 
        //don't need to store a PV - since it could be easily calculated on-the-fly
    }

    public class AmortisingScheduleItem
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public int ApplyEveryNthRoll;
        public double AmortisingAmount;
    }

    public class AmortisingResultItem
    {
        public string WasRolled;//indicated whether the roll date been adjusted or not.
        public DateTime RollDate;
        public double AmortisingAmount;
        public double OutstandingValue;
    }

    public class BillsSwapPricer2TermsRange
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public string BuildDates;//"forward/backward"
        public string RollDay;
        public string RollFrequency;
        public double FaceValue;
        public string Calendar;
        public string DateRollConvention;
        public string DayCountConvention;
    }

    public class MetaScheduleRangeItem
    {
        public string RollFrequency;
        public string Period;
        public string RollConvention;
    }

    #endregion

    #region BillsSwapPricer2

    public class BillsSwapPricer2
    {
        public static double ConvertActualToEffectiveRate(double actualRate, double actualRateFrequency, double effectiveRateFrequency)
        {
            double effectiveRate = (Math.Pow((1.0 + actualRate * actualRateFrequency), effectiveRateFrequency / actualRateFrequency) - 1.0) / effectiveRateFrequency;
            return effectiveRate;
        }

        public static List<AmortisingResultItem> GetAmortizationSchedule(List<Pair<DateTime, string>> rollDates, double initialNotional, List<AmortisingScheduleItem> amortisingSchedule)
        {
            var result = rollDates.Select(pair => new AmortisingResultItem
                                                      {
                                                          RollDate = pair.First, WasRolled = pair.Second, OutstandingValue = initialNotional
                                                      }).ToList();
            double currentNotional = initialNotional;
            foreach (AmortisingScheduleItem amortScheduleItem in amortisingSchedule)
            {
                DateTime startDate = amortScheduleItem.StartDate;
                DateTime endDate = amortScheduleItem.EndDate;
                IEnumerable<DateTime> rollDatesForItem = GetRollsForSchedule(rollDates, startDate, endDate);
                int cashflowNumber = 0;
                foreach (DateTime time in rollDatesForItem)
                {
                    if (cashflowNumber % amortScheduleItem.ApplyEveryNthRoll == 0)
                    {
                        //  implement AS EVERY 1ST ROLL (amortisingScheduleItem.ApplyEveryNthRoll)
                        //
                        AmortisingResultItem amortisingResultItem = GetItem(result, time);
                        amortisingResultItem.AmortisingAmount = amortScheduleItem.AmortisingAmount;
                        amortisingResultItem.OutstandingValue = currentNotional + amortisingResultItem.AmortisingAmount;
                        currentNotional = amortisingResultItem.OutstandingValue;
                    }
                    ++cashflowNumber;
                }
            }
            //  normalize notionals 
            //
            for (int i = 0; i < result.Count; ++i)
            {
                AmortisingResultItem currentItem = result[i];
                if (currentItem.AmortisingAmount == 0 && i > 0)
                {
                    AmortisingResultItem prevItem = result[i - 1];
                    currentItem.OutstandingValue = prevItem.OutstandingValue;
                }
            }
            return result;
        }

        private static AmortisingResultItem GetItem(IEnumerable<AmortisingResultItem> amortisingResultItems, DateTime rollDate)
        {
            foreach (AmortisingResultItem item in amortisingResultItems)
            {
                if (item.RollDate == rollDate)
                {
                    return item;
                }
            }
            throw new NotImplementedException();
        }

        private static IEnumerable<DateTime> GetRollsForSchedule(IEnumerable<Pair<DateTime, string>> rollDates, DateTime startDate, DateTime endDate)
        {
            return (from pair in rollDates where pair.First >= startDate && pair.First <= endDate select pair.First).ToList();
        }

        public static double GetEffectiveFrequency(List<AmortisingResultItem> cashflowsSchedule,
                                                   BillsSwapPricer2TermsRange terms)
        {
            IDayCounter dayCounter = DayCounterHelper.Parse(terms.DayCountConvention);
            double effectiveFrequency = GetEffectiveFrequency(cashflowsSchedule, dayCounter);

            return effectiveFrequency;
        }

        public static double GetEffectiveFrequency(List<AmortisingResultItem> cashflowsSchedule, IDayCounter dayCounter)
        {
            List<BillSwapPricer2CashflowItem> cashflows = GenerateFixedCashflowsFromAmortisingResultItems(cashflowsSchedule, dayCounter, 0);
            double totalNotional = cashflows.Sum(item => item.Notional);
            return cashflows.Sum(item => item.AccrualPeriod*(item.Notional/totalNotional));
        }

        public static double GetFixedRate(DateTime valuationDate, List<AmortisingResultItem> fixedCFs, List<AmortisingResultItem> floatCFs, IDayCounter dayCounter, RateCurve curve, double floatRateMargin, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            //  solve for the fixed rate
            //
            IObjectiveFunction objectiveFunction = new BillSwapPricer2SwapParRateObjectiveFunction(valuationDate, fixedCFs, floatCFs, curve, dayCounter, floatRateMargin, bulletPaymentDate, bulletPaymentValue);
            const double accuracy = 10e-14;
            const double guess = 0.1;
            //Are these min and max reasonable?
            var min = -.01;
            var max = .10;
            var solver = new Newton();
            return solver.Solve(objectiveFunction, accuracy, guess, min, max);
        }

        public static double GetFixedSidePV(DateTime valuationDate, List<AmortisingResultItem> fixedCFs, List<AmortisingResultItem> floatCFs, IDayCounter dayCounter, RateCurve curve, double floatRateMargin, double fixedRate, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            //  solve for the fixed rate
            //
            var objectiveFunction = new BillSwapPricer2SwapParRateObjectiveFunction(valuationDate, fixedCFs, floatCFs, curve, dayCounter, floatRateMargin, bulletPaymentDate, bulletPaymentValue);
            double presentValueOfFixedSide = - objectiveFunction.Value(fixedRate);
            return presentValueOfFixedSide;
        }

        public static double GetFixedSideSensitivity(DateTime valuationDate, List<AmortisingResultItem> fixedCFs, List<AmortisingResultItem> floatCFs, 
                                                     IDayCounter dayCounter, 
                                                     RateCurve originalCurve, RateCurve perturbedCurve, 
                                                     double floatRateMargin, double fixedRate, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            //  solve for the fixed rate
            //
            var objectiveFunction = new BillSwapPricer2SwapParRateObjectiveFunction(valuationDate, fixedCFs, floatCFs, originalCurve, dayCounter, floatRateMargin, bulletPaymentDate, bulletPaymentValue);
            double originalPV = objectiveFunction.Value(fixedRate);
            var objectiveFunctionWithPerturbedCurve = new BillSwapPricer2SwapParRateObjectiveFunction(valuationDate, fixedCFs, floatCFs, perturbedCurve, dayCounter, floatRateMargin, bulletPaymentDate, bulletPaymentValue);
            double perturbedPV = objectiveFunctionWithPerturbedCurve.Value(fixedRate);
            return perturbedPV - originalPV;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="originalCurve"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<param name="listInstrumentIdAndQuotes"></param>
        ///<param name="listPerturbations"></param>
        ///<returns></returns>
        public static List<DoubleRangeItem> CalculateFixedSideSensitivity2(DateTime valuationDate, double floatMargin, double fixedRate,
                                                                           BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
                                                                           BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
                                                                           RateCurve originalCurve, DateTime bulletPaymentDate, double bulletPaymentValue,
                                                                           List<InstrumentIdAndQuoteRangeItem> listInstrumentIdAndQuotes, List<DoubleRangeItem> listPerturbations)
        {
            var result = new List<DoubleRangeItem>();
            if (null == listPerturbations)
            {
                listPerturbations = new List<DoubleRangeItem>();
                foreach (InstrumentIdAndQuoteRangeItem item in listInstrumentIdAndQuotes)
                {
                    item.InstrumentId = RemoveExtraInformationFromInstrumentId(item.InstrumentId);
                    var defaultPerturbationAmount = new DoubleRangeItem
                                                        {
                                                            Value = GetDefaultPerturbationAmount(item.InstrumentId)
                                                        };
                    listPerturbations.Add(defaultPerturbationAmount);
                }
            }
            for (int i = 0; i < listInstrumentIdAndQuotes.Count; i++)
            {
                InstrumentIdAndQuoteRangeItem item = listInstrumentIdAndQuotes[i];
                item.InstrumentId = RemoveExtraInformationFromInstrumentId(item.InstrumentId);
                DoubleRangeItem perturbItem = listPerturbations[i];
                //  pay == fixed.
                //
                IDayCounter dayCounter = DayCounterHelper.Parse(payTerms.DayCountConvention);
                var perturbationArray = new List<Pair<string, decimal>> { new Pair<string, decimal>(item.InstrumentId, (decimal)perturbItem.Value) };
                //var perturbedCurveId = ObjectCacheHelper.PerturbRateCurve(curveId, perturbationArray);

                //  Perturb the curve
                //
                var perturbedReceiveCurve = (RateCurve)originalCurve.PerturbCurve(perturbationArray);
                double sensitivity = GetFixedSideSensitivity(valuationDate, payRolls, receiveRolls, dayCounter, originalCurve, perturbedReceiveCurve, floatMargin, fixedRate, bulletPaymentDate, bulletPaymentValue);
                var bucketSensitivityItem = new DoubleRangeItem {Value = sensitivity};
                result.Add(bucketSensitivityItem);
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="originalReceiveCurve"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<param name="listInstrumentIdAndQuotes"></param>
        ///<param name="listPerturbations"></param>
        ///<param name="filterByInstruments"></param>
        ///<returns></returns>
        public static double CalculateFixedSideDelta(DateTime valuationDate, double floatMargin, double fixedRate,
                                                     BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
                                                     BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
                                                     RateCurve originalReceiveCurve, DateTime bulletPaymentDate, double bulletPaymentValue,
                                                     List<InstrumentIdAndQuoteRangeItem> listInstrumentIdAndQuotes, List<DoubleRangeItem> listPerturbations, string filterByInstruments)
        {
            if (null == listPerturbations)
            {
                listPerturbations = new List<DoubleRangeItem>();
                foreach (InstrumentIdAndQuoteRangeItem item in listInstrumentIdAndQuotes)
                {
                    item.InstrumentId = RemoveExtraInformationFromInstrumentId(item.InstrumentId);
                    var defaultPerturbationAmount = new DoubleRangeItem
                                                        {
                                                            Value = GetDefaultPerturbationAmount(item.InstrumentId)
                                                        };
                    listPerturbations.Add(defaultPerturbationAmount);
                }
            }
            var perturbationArray = new List<Pair<string, decimal>>();
            for (int i = 0; i < listInstrumentIdAndQuotes.Count; i++)
            {
                InstrumentIdAndQuoteRangeItem item = listInstrumentIdAndQuotes[i];
                item.InstrumentId = RemoveExtraInformationFromInstrumentId(item.InstrumentId);
                DoubleRangeItem perturbItem = listPerturbations[i];
                if (!String.IsNullOrEmpty(filterByInstruments))
                {
                    if (item.InstrumentId.StartsWith(filterByInstruments, true, null))
                    {
                        perturbationArray.Add(new Pair<string, decimal>(item.InstrumentId, (decimal)perturbItem.Value));
                    }
                }
                else
                {
                    perturbationArray.Add(new Pair<string, decimal>(item.InstrumentId, (decimal)perturbItem.Value));
                }
            }
            //var perturbedCurveId = originalReceiveCurve.PerturbCurve(perturbationArray);
            //  Perturb the curve
            //
            //Curves.RateCurve perturbedReceiveCurve = RateCurveInMemoryCollection.Instance.Get(perturbedCurveId);
            var perturbedReceiveCurve = (RateCurve)originalReceiveCurve.PerturbCurve(perturbationArray); //ObjectCacheHelper.GetPricingStructureFromSerialisable(perturbedCurveId);
            //  pay == fixed.
            //
            IDayCounter dayCounter = DayCounterHelper.Parse(payTerms.DayCountConvention);
            double sensitivity = GetFixedSideSensitivity(valuationDate, payRolls, receiveRolls, dayCounter, originalReceiveCurve, perturbedReceiveCurve, floatMargin, fixedRate, bulletPaymentDate, bulletPaymentValue);
            return sensitivity;
        }

        ///<summary>
        ///</summary>
        ///<param name="rateCurve"></param>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<returns></returns>
        public static double CalculateFixedRate(DateTime valuationDate, double floatMargin,
                                                BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
                                                BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
                                                RateCurve rateCurve, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            //  pay == fixed.
            //
            IDayCounter dayCounter = DayCounterHelper.Parse(payTerms.DayCountConvention);
            double fixedRate = GetFixedRate(valuationDate, payRolls, receiveRolls, dayCounter, rateCurve, floatMargin, bulletPaymentDate, bulletPaymentValue);
            return fixedRate;
        }

        public static double CalculateFixedSideSensitivity(DateTime valuationDate, double floatMargin, double fixedRate,
                                                           BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
                                                           BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
                                                           RateCurve rateCurve, DateTime bulletPaymentDate, double bulletPaymentValue,
                                                           string curveInstrumentId, double perturbationAmount)
        {
            //  pay == fixed.
            var perturbationArray = new List<Pair<string, decimal>>
                                        {new Pair<string, decimal>(curveInstrumentId, (decimal) perturbationAmount)};
            IDayCounter dayCounter = DayCounterHelper.Parse(payTerms.DayCountConvention);
            //var originalCurve = (RateCurve)cache.LoadObject(curveId);
            var perturbedCurve = rateCurve.PerturbCurve(perturbationArray) as RateCurve;
            //  Perturb the curve
            //
            //var perturbedReceiveCurve = (RateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(perturbedCurveId);
            double sensitivity = GetFixedSideSensitivity(valuationDate, payRolls, receiveRolls, dayCounter, rateCurve, perturbedCurve, 
                floatMargin, fixedRate, bulletPaymentDate, bulletPaymentValue);
            return sensitivity;
        }

        ///<summary>
        ///</summary>
        ///<param name="cfItems"></param>
        ///<param name="amortisingSchedule"></param>
        ///<returns></returns>
        public static List<AmortisingResultItem> GenerateAmortisationSchedule(List<AmortisingResultItem> cfItems, List<AmortisingScheduleItem> amortisingSchedule)
        {
            var dateTimeList = cfItems.Select(item => new Pair<DateTime, string>(item.RollDate, item.WasRolled)).ToList();
            List<AmortisingResultItem> amortizationSchedule = GetAmortizationSchedule(dateTimeList, cfItems[0].OutstandingValue, amortisingSchedule);
            return amortizationSchedule;
        }


        ///<summary>
        ///</summary>
        ///<param name="termsRange"></param>
        ///<param name="metaScheduleDefinitionRange"></param>
        ///<param name="paymentCalendar"></param>
        ///<returns></returns>
        public static List<AmortisingResultItem> GenerateCashflowSchedule(BillsSwapPricer2TermsRange termsRange, 
            List<MetaScheduleRangeItem> metaScheduleDefinitionRange, IBusinessCalendar paymentCalendar)
        {

            RollConventionEnum rollConventionEnum = RollConventionEnumHelper.Parse(termsRange.RollDay);
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(termsRange.DateRollConvention, termsRange.Calendar);
            bool backwardGeneration = (termsRange.BuildDates.ToLower() == "backward");
            List<DateTime> adjustedDatesResult;
            List<DateTime> unadjustedDatesResult;
            if (termsRange.RollFrequency.ToLower().Contains("custom"))
            {
                var rollsMetaSchedule = metaScheduleDefinitionRange.Select(item => new MetaScheduleItem
                                                                                       {
                                                                                           RollFrequency = PeriodHelper.Parse(item.RollFrequency), Period = PeriodHelper.Parse(item.Period), RollConvention = RollConventionEnumHelper.Parse(item.RollConvention)
                                                                                       }).ToList();
                unadjustedDatesResult = DatesMetaSchedule.GetUnadjustedDates3(termsRange.StartDate, termsRange.EndDate,
                                                                              rollsMetaSchedule, backwardGeneration);
                adjustedDatesResult = AdjustedDatesMetaSchedule.GetAdjustedDates3(termsRange.StartDate, termsRange.EndDate,
                                                                         rollsMetaSchedule, backwardGeneration, businessDayAdjustments, paymentCalendar);
            }
            else
            {
                Period interval = PeriodHelper.Parse(termsRange.RollFrequency);
                unadjustedDatesResult = DatesMetaSchedule.GetUnadjustedDates2(termsRange.StartDate, termsRange.EndDate,
                                                                              interval, rollConventionEnum, backwardGeneration);
                adjustedDatesResult = AdjustedDatesMetaSchedule.GetAdjustedDates2(termsRange.StartDate, termsRange.EndDate,
                                                                          interval, rollConventionEnum, backwardGeneration, businessDayAdjustments, paymentCalendar);
            }
            var result = new List<AmortisingResultItem>();
            for (int i = 0; i < adjustedDatesResult.Count; i++)
            {
                DateTime adjustedTime = adjustedDatesResult[i];
                DateTime unadjustedTime = unadjustedDatesResult[i];
                var amortisingResultItem = new AmortisingResultItem
                                          {
                                              WasRolled = (adjustedTime == unadjustedTime) ? "No" : "Yes",
                                              RollDate = adjustedTime,
                                              AmortisingAmount = 0,
                                              OutstandingValue = termsRange.FaceValue
                                          };
                result.Add(amortisingResultItem);
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="floatMargin"></param>
        ///<param name="fixedRate"></param>
        ///<param name="payTerms"></param>
        ///<param name="payRolls"></param>
        ///<param name="receiveTerms"></param>
        ///<param name="receiveRolls"></param>
        ///<param name="rateCurve"></param>
        ///<param name="bulletPaymentDate"></param>
        ///<param name="bulletPaymentValue"></param>
        ///<returns></returns>
        public static double CalculateFixedSidePV(DateTime valuationDate, double floatMargin, double fixedRate,
                                                  BillsSwapPricer2TermsRange payTerms, List<AmortisingResultItem> payRolls,
                                                  BillsSwapPricer2TermsRange receiveTerms, List<AmortisingResultItem> receiveRolls,
                                                  RateCurve rateCurve, DateTime bulletPaymentDate, double bulletPaymentValue)
        {
            //  pay == fixed.
            //
            IDayCounter dayCounter = DayCounterHelper.Parse(payTerms.DayCountConvention);
            double fixedSidePV = GetFixedSidePV(valuationDate, payRolls, receiveRolls, dayCounter, rateCurve, floatMargin, fixedRate, bulletPaymentDate, bulletPaymentValue);
            return fixedSidePV;
        }

        public class BillSwapPricer2SwapParRateObjectiveFunction : IObjectiveFunction
        {
            private readonly List<AmortisingResultItem> _fixedCFs;
            private readonly List<AmortisingResultItem> _floatCFs;
            private readonly RateCurve _curve;
            private readonly IDayCounter _dayCounter;
            private readonly double _floatRateMargin;
            private readonly DateTime _bulletPaymentDate;
            private readonly double _bulletPaymentValue;          
            private readonly DateTime _valuationDate;
          
            public BillSwapPricer2SwapParRateObjectiveFunction(DateTime valuationDate, List<AmortisingResultItem> fixedCFs, List<AmortisingResultItem> floatCFs, RateCurve curve, IDayCounter dayCounter, double floatRateMargin, DateTime bulletPaymentDate, double bulletPaymentValue)
            {
                _fixedCFs = fixedCFs;
                _floatCFs = floatCFs;
                _curve = curve;
                _dayCounter = dayCounter;
                _floatRateMargin = floatRateMargin;
                _valuationDate = valuationDate;
                _bulletPaymentDate = bulletPaymentDate;
                _bulletPaymentValue = bulletPaymentValue;
            }


            public double Value(double fixedRate)
            {
                List<BillSwapPricer2CashflowItem> floatCFs = GenerateFloatingCashflowsFromAmortisingResultItems(_floatCFs, _dayCounter, _curve,
                                                                                                          _floatRateMargin);
                List<BillSwapPricer2CashflowItem> fixedCFs = GenerateFixedCashflowsFromAmortisingResultItems(_fixedCFs, _dayCounter, fixedRate);
                //  Float side PV
                //
                double floatPV = (from cf in floatCFs
                                  let floatInterest = cf.Notional - cf.DiscountedValue
                                  select floatInterest*_curve.GetDiscountFactor(_valuationDate, cf.StartDate)).Sum();
                //  Fixed side PV
                //
                double fixedPV = (from cf in fixedCFs
                                  let fixedInterest = cf.Notional - cf.DiscountedValue
                                  select fixedInterest*_curve.GetDiscountFactor(_valuationDate, cf.StartDate)).Sum();
                // we pay to client 
                //
                double bulletPaymentPresentValue = _bulletPaymentValue * _curve.GetDiscountFactor(_valuationDate, _bulletPaymentDate);
                return floatPV + bulletPaymentPresentValue - fixedPV;
            }

            /// <summary>
            /// Derivative of the objective function.
            /// </summary>
            /// <param name="x">Argument to the objective function.</param>
            /// <returns>The value of the derivative, <i>f'(x)</i>.</returns>
            /// <exception cref="NotImplementedException">
            /// Thrown when the function's derivative has not been implemented.
            /// </exception>
            public double Derivative(double x)
            {
                throw new NotImplementedException();
            }
        }

        internal static List<BillSwapPricer2CashflowItem> GenerateFloatingCashflowsFromAmortisingResultItems(List<AmortisingResultItem> amortisingResultItems, IDayCounter dayCounter, RateCurve curve, double floatRateMargin)
        {
            var result = new List<BillSwapPricer2CashflowItem>();
            for(int i = 0; i < amortisingResultItems.Count - 1; ++i)
            {
                var billSwapPricer2CashflowItem = new BillSwapPricer2CashflowItem
                                                      {
                                                          StartDate = amortisingResultItems[i].RollDate,
                                                          EndDate = amortisingResultItems[i + 1].RollDate,
                                                          Notional = amortisingResultItems[i].OutstandingValue
                                                      };
                billSwapPricer2CashflowItem.AccrualPeriod = dayCounter.YearFraction(billSwapPricer2CashflowItem.StartDate, billSwapPricer2CashflowItem.EndDate);
                double startOfPeriodDiscount = curve.GetDiscountFactor(billSwapPricer2CashflowItem.StartDate);
                double endOfPeriodDiscount = curve.GetDiscountFactor(billSwapPricer2CashflowItem.EndDate);
                double forecastContinuouslyCompoundingRate = floatRateMargin + ((startOfPeriodDiscount / endOfPeriodDiscount - 1.0) / billSwapPricer2CashflowItem.AccrualPeriod);
                billSwapPricer2CashflowItem.ImpliedForwardRate = forecastContinuouslyCompoundingRate;
                billSwapPricer2CashflowItem.DiscountedValue = billSwapPricer2CashflowItem.Notional /
                                                              (1 + billSwapPricer2CashflowItem.ImpliedForwardRate * billSwapPricer2CashflowItem.AccrualPeriod);
                result.Add(billSwapPricer2CashflowItem);
            }
            return result;
        }


        private static double GetDefaultPerturbationAmount(string instrumentId)
        {
            if (instrumentId.ToLower().Contains("future"))
            {
                return -0.01;
            }
            return 0.0001;
        }

        private static string RemoveExtraInformationFromInstrumentId(string instrumentId)
        {
            if (-1 != instrumentId.IndexOf('('))
            {
                string cleanedInstrumentId = instrumentId.Substring(0, instrumentId.IndexOf('(') - 1);
                return cleanedInstrumentId.Trim();
            }
            return instrumentId;
        }
        
        internal static List<BillSwapPricer2CashflowItem>    GenerateFixedCashflowsFromAmortisingResultItems(List<AmortisingResultItem> amortisingResultItems, IDayCounter dayCounter, double fixedRate)
        {
            var result = new List<BillSwapPricer2CashflowItem>();
            for (int i = 0; i < amortisingResultItems.Count - 1; ++i)
            {
                var billSwapPricer2CashflowItem = new BillSwapPricer2CashflowItem
                                                      {
                                                          StartDate = amortisingResultItems[i].RollDate,
                                                          EndDate = amortisingResultItems[i + 1].RollDate,
                                                          Notional = amortisingResultItems[i].OutstandingValue
                                                      };
                billSwapPricer2CashflowItem.AccrualPeriod = dayCounter.YearFraction(billSwapPricer2CashflowItem.StartDate, billSwapPricer2CashflowItem.EndDate);
                billSwapPricer2CashflowItem.ImpliedForwardRate = fixedRate;
                billSwapPricer2CashflowItem.DiscountedValue = billSwapPricer2CashflowItem.Notional /
                                                              (1 + billSwapPricer2CashflowItem.ImpliedForwardRate * billSwapPricer2CashflowItem.AccrualPeriod);

                result.Add(billSwapPricer2CashflowItem);
            }
            return result;
        }
    }

    #endregion
}