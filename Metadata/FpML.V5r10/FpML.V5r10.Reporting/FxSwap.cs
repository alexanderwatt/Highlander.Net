using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting
{
    public partial class FxSwap
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            result.AddRange(nearLeg.GetRequiredPricingStructures());
            var curves = farLeg.GetRequiredPricingStructures();
            foreach (var curve in curves)
            {
                if (!result.Contains(curve))
                {
                    result.Add(curve);
                }
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string>();
            result.AddRange(nearLeg.GetRequiredCurrencies());
            var currencies = farLeg.GetRequiredCurrencies();
            foreach (var currency in currencies)
            {
                if (!result.Contains(currency))
                {
                    result.Add(currency);
                }
            }
            return result;
        }
    }
}
