using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting
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
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(principal.currency, true);
                result.Add(discountCurve);
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String> {principal.currency.Value};
            return result;
        }
    }
}
