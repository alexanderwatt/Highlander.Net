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
using Highlander.Reporting.V5r3;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;

#endregion

namespace Highlander.ValuationEngine.V5r3.Helpers
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
                string message =
                    $"Accrual period is 0 days. calculationPeriod.adjustedStartDate = '{calculationPeriod.adjustedStartDate}', calculationPeriod.adjustedEndDate = '{calculationPeriod.adjustedEndDate}'";
                throw new System.Exception(message);
            }
            double forecastContinuouslyCompoundingRate = (startOfPeriodDiscount / endOfPeriodDiscount - 1.0) / accrualPeriod;
            return (decimal)forecastContinuouslyCompoundingRate;
        }
    }
}