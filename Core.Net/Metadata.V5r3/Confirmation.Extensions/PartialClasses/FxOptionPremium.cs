#region usings

using System;
using System.Collections.Generic;

#endregion

namespace nab.QDS.FpML.V47
{
    public partial class FxOptionPremium
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (premiumAmount != null)
            {
                var discountCurve = Helpers.GetDiscountCurveName(premiumAmount.currency);
                result.Add(discountCurve);
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<String> {premiumAmount.currency.Value};
            return result;
        }

        public static FxOptionPremium Create(string payer, string receiver, string currency, decimal amount, DateTime settlementDate)
        {
            var fxOptionPremium = new FxOptionPremium();
            return fxOptionPremium;
        }
    }
}
