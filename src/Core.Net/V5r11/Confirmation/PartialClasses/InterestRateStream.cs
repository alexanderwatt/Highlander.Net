using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r3.Confirmation
{
    public partial class InterestRateStream
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            var amount = calculationPeriodAmount.Item as Calculation;
            if (amount != null)
            {
                var currency = XsdClassesFieldResolver.CalculationGetNotionalSchedule(amount);
                if (currency != null && currency.notionalStepSchedule != null)
                {
                    var discountCurve = Helpers.GetDiscountCurveName(currency.notionalStepSchedule.currency);
                    result.Add(discountCurve);
                }
                var floatingRateCalculation = amount.Items[0] as FloatingRateCalculation;
                if (floatingRateCalculation != null)
                {
                    result.Add(Helpers.GetForecastCurveName(floatingRateCalculation.floatingRateIndex, floatingRateCalculation.indexTenor));
                }
            }
            //TODO
            if (stubCalculationPeriodAmount != null)
            {
                if (stubCalculationPeriodAmount.Items != null)
                {
                    foreach (var item in stubCalculationPeriodAmount.Items)
                    {
                        if (item.Items != null)
                        {
                            result.AddRange(from value in item.Items
                                            where value as Money != null
                                            select Helpers.GetDiscountCurveName(((Money)value).currency));
                        }
                    }                   
                }
            //if (stubCalculationPeriodAmount != null)
            //{
            //    if (stubCalculationPeriodAmount.initialStub != null && stubCalculationPeriodAmount.initialStub.Items != null)
            //    {
            //        result.AddRange(from value in stubCalculationPeriodAmount.initialStub.Items
            //                            where value as Money != null
            //                            select Helpers.GetDiscountCurveName(((Money)value).currency));
            //    }
            //    if (stubCalculationPeriodAmount.finalStub != null && stubCalculationPeriodAmount.finalStub.Items != null)
            //    {
            //        result.AddRange(from value in stubCalculationPeriodAmount.finalStub.Items
            //                            where value as Money != null
            //                            select Helpers.GetDiscountCurveName(((Money)value).currency));
            //    }
            }
            return result;
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredVolatilitySurfaces()
        {
            var result = new List<String>();
            var amount = calculationPeriodAmount.Item as Calculation;
            if (amount != null)
            {
                var floatingRateCalculation = amount.Items[0] as FloatingRateCalculation;
                if (floatingRateCalculation != null)
                {
                    result.Add(Helpers.GetRateVolatilityMatrixName(floatingRateCalculation.floatingRateIndex, floatingRateCalculation.indexTenor));
                }
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<string>();
            var item = XsdClassesFieldResolver.CalculationGetNotionalSchedule((Calculation)calculationPeriodAmount.Item);

            if (item != null && item.notionalStepSchedule != null && item.notionalStepSchedule.currency != null)
            {
                result.Add(item.notionalStepSchedule.currency.Value);
            }
            return result;
        }
    }
}
