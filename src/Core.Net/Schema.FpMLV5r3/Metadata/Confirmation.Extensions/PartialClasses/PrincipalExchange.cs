using System;
using System.Collections.Generic;

namespace nab.QDS.FpML.V47
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
                var discountCurve = Helpers.GetDiscountCurveName(presentValuePrincipalExchangeAmount.currency);
                result.Add(discountCurve);
            }

            return result;
        }

    }
}
