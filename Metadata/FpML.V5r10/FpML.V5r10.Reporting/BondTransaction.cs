using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting
{
    public partial class BondTransaction
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()//TODO Need to add the case of floaters where there is a forecast curve.
        {
            var result = new List<String>();
            if (bond?.Item is string && bond.currency != null)
            {
                var tempId = bond.id.Split('-');
                var bondId = tempId[0];
                if (tempId.Length > 2)
                {
                    bondId = tempId[2];
                }
                var bondCurve = CurveNameHelpers.GetBondCurveName(bond.currency.Value, bondId);
                result.Add(bondCurve);
            }
            if (bond?.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(bond.currency.Value, true);
                result.Add(discountCurve);
                if (notionalAmount.currency != null && notionalAmount.currency.Value != bond.currency.Value)
                {
                    var discountCurve2 = CurveNameHelpers.GetDiscountCurveName(notionalAmount.currency, true);
                    result.Add(discountCurve2);
                }
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String> { notionalAmount.currency.Value };
            if (notionalAmount.currency.Value != bond.currency.Value)
            {
                result.Add(bond.currency.Value);
            }
            return result;
        }
    }
}