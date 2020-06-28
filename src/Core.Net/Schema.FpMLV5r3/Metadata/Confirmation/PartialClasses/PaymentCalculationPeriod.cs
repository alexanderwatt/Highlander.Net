using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r3.Confirmation
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
                var discountCurve = Helpers.GetDiscountCurveName(currency);
                result.Add(discountCurve);
            }
            if(Items != null)
            {
                result.AddRange(Items.Select(calculationPeriod => ((CalculationPeriod) calculationPeriod).forecastAmount).Select(forecastAmount => Helpers.GetDiscountCurveName(forecastAmount.currency)));
            }

            return result;
        }

    }
}
