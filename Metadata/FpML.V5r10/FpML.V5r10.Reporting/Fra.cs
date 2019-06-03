#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting
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
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(notional.currency, true);
                result.Add(discountCurve);
            }           
            if (floatingRateIndex != null && indexTenor!=null)
                {
                    result.Add(CurveNameHelpers.GetForecastCurveName(floatingRateIndex, indexTenor[0]));
                }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string> {notional.currency.Value};
            return result;
        }
    }
}
