#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting
{
    public partial class MoneyBase
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(currency, true);
                result.Add(discountCurve);
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<String> {currency.Value};
            return result;
        }
    }
}
