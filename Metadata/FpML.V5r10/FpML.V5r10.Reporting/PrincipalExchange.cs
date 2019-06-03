using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting
{
    public partial class PrincipalExchange
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (presentValuePrincipalExchangeAmount.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(presentValuePrincipalExchangeAmount.currency, true);
                result.Add(discountCurve);
            }

            return result;
        }

    }
}
