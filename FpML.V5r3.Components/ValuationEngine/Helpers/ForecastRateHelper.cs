#region Using directives

using System;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public static class ForecastRateHelper
    {
        public static FloatingRateDefinition CreateFloatingRateDefinition(FloatingRate floatingRate,
                                                                          DayCountFraction dayCountFraction,
                                                                          CalculationPeriod calculationPeriod,
                                                                          IRateCurve forecastCurve)
        {
            //throw new NotImplementedException();
            // assign floating rate def with adjusted fixing date
            //
            var floatingRateDefinition = new FloatingRateDefinition {spread = 0.000m, spreadSpecified = true};
            // Spread = 0.0
            //
            var rateObservation = new RateObservation();
            floatingRateDefinition.rateObservation = new[] { rateObservation };
            rateObservation.forecastRate = GetForecastRate(calculationPeriod, forecastCurve, dayCountFraction);
            rateObservation.forecastRateSpecified = true;
            Decimal finalRate = rateObservation.forecastRate;
            // If spread specified - add it to final rate.
            //
            if (floatingRateDefinition.spreadSpecified)
            {
                finalRate += floatingRateDefinition.spread;
            }
            floatingRateDefinition.calculatedRate = finalRate;
            floatingRateDefinition.calculatedRateSpecified = true;
            return floatingRateDefinition;
        }

        public static FloatingRateDefinition CreateFloatingRateDefinition(CalculationPeriod calculationPeriod)
        {
            var floatingRateDefinition = new FloatingRateDefinition();
            //floatingRateDefinition.spread = 0.0m;
            //floatingRateDefinition.spreadSpecified = true;
            return floatingRateDefinition;
        }

        public static void UpdateFloatingRateDefinition(FloatingRateDefinition floatingRateDefinition,
                                                        FloatingRateCalculation floatingRateCalculation,
                                                        DayCountFraction dayCountFraction,
                                                        CalculationPeriod calculationPeriod,
                                                        IRateCurve forecastCurve)
        {
            var rateObservation = new RateObservation();
            if (floatingRateDefinition.rateObservation!=null)
            {
                if (floatingRateDefinition.rateObservation[0].adjustedFixingDateSpecified)
                {
                    rateObservation.adjustedFixingDate = floatingRateDefinition.rateObservation[0].adjustedFixingDate;
                    rateObservation.adjustedFixingDateSpecified = true;
                }
            }
            floatingRateDefinition.rateObservation = new[] { rateObservation };
            rateObservation.forecastRate = GetForecastRate(calculationPeriod, forecastCurve, dayCountFraction);
            rateObservation.forecastRateSpecified = true;
            Decimal finalRate = rateObservation.forecastRate;
            // If spread specified - add it to the final rate.
            //
            if (floatingRateDefinition.spreadSpecified)
            {
                finalRate += floatingRateDefinition.spread;
            }
            // Apply rounding (if it's been specified)
            //
            if (null != floatingRateCalculation.finalRateRounding)
            {
                Rounding finalRateRounding = floatingRateCalculation.finalRateRounding;
                floatingRateDefinition.calculatedRate = RoundingHelper.Round(finalRate, finalRateRounding);
            }
            else
            {
                floatingRateDefinition.calculatedRate = finalRate;
            }
            floatingRateDefinition.calculatedRateSpecified = true;
        }



        private static decimal GetForecastRate(CalculationPeriod calculationPeriod, IRateCurve forecastCurve, DayCountFraction dayCountFraction)
        {
            double startOfPeriodDiscount = forecastCurve.GetDiscountFactor(calculationPeriod.adjustedStartDate);
            double endOfPeriodDiscount = forecastCurve.GetDiscountFactor(calculationPeriod.adjustedEndDate);
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountFraction.Value);
            double accrualPeriod = dayCounter.YearFraction(calculationPeriod.adjustedStartDate, calculationPeriod.adjustedEndDate);
            if (0 == accrualPeriod)
            {
                string message = String.Format("Accrual period is 0 days. calculationPeriod.adjustedStartDate = '{0}', calculationPeriod.adjustedEndDate = '{1}'", calculationPeriod.adjustedStartDate, calculationPeriod.adjustedEndDate);
                throw new System.Exception(message);
            }
            double forecastContinouslyCompoundingRate = (startOfPeriodDiscount / endOfPeriodDiscount - 1.0) / accrualPeriod;
            return (decimal)forecastContinouslyCompoundingRate;
        }
    }
}