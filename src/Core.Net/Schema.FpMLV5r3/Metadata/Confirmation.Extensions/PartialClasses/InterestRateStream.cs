using System;
using System.Collections.Generic;
using System.Linq;

namespace nab.QDS.FpML.V47
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
                var currency = XsdClassesFieldResolver.Calculation_GetNotionalSchedule(amount);
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
                foreach(var stubIndex in stubCalculationPeriodAmount.Items)
                {
                    result.AddRange(from value in stubIndex.Items
                                    where value as Money != null
                                    select Helpers.GetDiscountCurveName(((Money) value).currency));
                }
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
            var item = XsdClassesFieldResolver.Calculation_GetNotionalSchedule((Calculation)calculationPeriodAmount.Item);

            if (item != null && item.notionalStepSchedule != null && item.notionalStepSchedule.currency != null)
            {
                result.Add(item.notionalStepSchedule.currency.Value);
            }
            return result;
        }
    }
}
