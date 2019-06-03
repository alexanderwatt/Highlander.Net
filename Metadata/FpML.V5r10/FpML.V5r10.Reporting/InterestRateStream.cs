using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r10.Reporting
{
    public partial class InterestRateStream
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (calculationPeriodAmount.Item is Calculation amount)
            {
                var currency = XsdClassesFieldResolver.CalculationGetNotionalSchedule(amount);
                if (currency?.notionalStepSchedule != null)
                {
                    var discountCurve = CurveNameHelpers.GetDiscountCurveName(currency.notionalStepSchedule.currency, true);
                    result.Add(discountCurve);
                }

                if (amount.Items[0] is FloatingRateCalculation floatingRateCalculation)
                {
                    result.Add(CurveNameHelpers.GetForecastCurveName(floatingRateCalculation.floatingRateIndex, floatingRateCalculation.indexTenor));
                }
            }
            //TODO
            if (stubCalculationPeriodAmount != null)
            {
                if (stubCalculationPeriodAmount.initialStub?.Items != null)
                {
                    result.AddRange(from value in stubCalculationPeriodAmount.initialStub.Items
                                        where value is Money
                                    select CurveNameHelpers.GetDiscountCurveName(((Money)value).currency, true));
                }
                if (stubCalculationPeriodAmount.finalStub?.Items != null)
                {
                    result.AddRange(from value in stubCalculationPeriodAmount.finalStub.Items
                                        where value is Money
                                    select CurveNameHelpers.GetDiscountCurveName(((Money)value).currency, true));
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
            if (calculationPeriodAmount.Item is Calculation amount)
            {
                if (amount.Items[0] is FloatingRateCalculation floatingRateCalculation)
                {
                    result.Add(CurveNameHelpers.GetRateVolatilityMatrixName(floatingRateCalculation.floatingRateIndex, floatingRateCalculation.indexTenor));
                }
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<string>();
            var item = XsdClassesFieldResolver.CalculationGetNotionalSchedule((Calculation)calculationPeriodAmount.Item);

            if (item?.notionalStepSchedule?.currency != null)
            {
                result.Add(item.notionalStepSchedule.currency.Value);
            }
            return result;
        }
    }
}
