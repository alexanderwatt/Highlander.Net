#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting
{
    public partial class Payment
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures()
        {
            var result = new List<String>();
            if (paymentAmount.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(paymentAmount.currency, true);
                result.Add(discountCurve);
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<String> { paymentAmount.currency.Value };
            return result;
        }
    }

    public partial class NonNegativePayment
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (paymentAmount.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(paymentAmount.currency, true);
                result.Add(discountCurve);
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<String> {paymentAmount.currency.Value};
            return result;
        }
    }

    public partial class PositivePayment
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            if (paymentAmount.currency != null)
            {
                var discountCurve = CurveNameHelpers.GetDiscountCurveName(paymentAmount.currency, true);
                result.Add(discountCurve);
            }
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<String> {paymentAmount.currency.Value};
            return result;
        }
    }
}
