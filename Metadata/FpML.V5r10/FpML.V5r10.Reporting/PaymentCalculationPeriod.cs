using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r10.Reporting
{
    public partial class PaymentCalculationPeriod
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();

            if (forecastPaymentAmount != null)
            {
                var currency = forecastPaymentAmount.currency;
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(currency, true);
                result.Add(discountCurve);
            }
            if(Items != null)
            {
                result.AddRange(Items.Select(calculationPeriod => ((CalculationPeriod)calculationPeriod).forecastAmount).Select(forecastAmount => CurveNameHelpers.GetDiscountCurveName(forecastAmount.currency, true)));
            }

            return result;
        }

    }
}
