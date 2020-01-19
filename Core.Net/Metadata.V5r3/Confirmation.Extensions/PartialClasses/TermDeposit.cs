using System;
using System.Collections.Generic;
using nab.QDS.FpML.V47.Extensions;

namespace nab.QDS.FpML.V47
{
    public partial class TermDeposit
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (principal.currency != null)
            {
                var discountCurve = Helpers.GetDiscountCurveName(principal.currency);
                result.Add(discountCurve);
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String>();
            result.Add(this.principal.currency.Value);
            return result;
        }
    }
}
