using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting
{
    public partial class EquityTransaction
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()//TODO Need to add the case of floaters where there is a forecast curve.
        {
            var result = new List<String>();
            if (equity.currency != null)
            {
                var bondCurve = CurveNameHelpers.GetEquityCurveName(equity.currency.Value,
                                                                      equity.id);
                result.Add(bondCurve);
            }
            if (equity.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(equity.currency.Value, true);
                result.Add(discountCurve);
                if (unitPrice.currency != null && unitPrice.currency.Value != equity.currency.Value)
                {
                    var discountCurve2 = CurveNameHelpers.GetDiscountCurveName(unitPrice.currency, true);
                    result.Add(discountCurve2);
                }
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String> { equity.currency.Value };
            if (unitPrice.currency.Value != equity.currency.Value)
            {
                result.Add(unitPrice.currency.Value);
            }
            return result;
        }
    }
}