using System;
using System.Collections.Generic;

namespace FpML.V5r3.Confirmation
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
            result.AddRange(farLeg.GetRequiredPricingStructures());
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string>();
            result.AddRange(nearLeg.GetRequiredPricingStructures());
            result.AddRange(farLeg.GetRequiredPricingStructures());
            return result;
        }
    }
}
