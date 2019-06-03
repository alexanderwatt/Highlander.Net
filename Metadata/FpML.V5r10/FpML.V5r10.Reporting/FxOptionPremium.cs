#region usings

using System;

#endregion

namespace FpML.V5r10.Reporting
{
    public partial class FxOptionPremium
    {
        ///// <summary>
        ///// Gets and sets the required pricing structures to value this leg.
        ///// </summary>
        //public List<String> GetRequiredPricingStructures() 
        //{
        //    var result = new List<String>();
        //    if (paymentAmount != null)
        //    {
        //        var discountCurve = Helpers.GetDiscountCurveName(paymentAmount.currency);
        //        result.Add(discountCurve);
        //    }
        //    return result;
        //}

        //public List<String> GetRequiredCurrencies()
        //{
        //    var result = new List<String> { paymentAmount.currency.Value };
        //    return result;
        //}

        public static FxOptionPremium Create(string payer, string receiver, string currency, decimal amount, DateTime settlementDate)
        {
            var fxOptionPremium = new FxOptionPremium();
            return fxOptionPremium;
        }
    }
}
