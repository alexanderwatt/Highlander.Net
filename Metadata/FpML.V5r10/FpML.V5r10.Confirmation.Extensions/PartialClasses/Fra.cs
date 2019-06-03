#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace nab.QDS.FpML.V47
{
    public partial class Fra
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (notional.currency != null)
            {
                var discountCurve = Helpers.GetDiscountCurveName(notional.currency);
                result.Add(discountCurve);
            }           
            if (floatingRateIndex != null && indexTenor!=null)
                {
                    result.Add(Helpers.GetForecastCurveName(floatingRateIndex, indexTenor[0]));
                }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string>();
            result.Add(notional.currency.Value);
            return result;
        }
    }
}
