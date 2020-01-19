using System;
using System.Collections.Generic;

namespace nab.QDS.FpML.V47
{
    public partial class FxSwap
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            foreach (var leg in fxSingleLeg)
            {
                result.AddRange(leg.GetRequiredPricingStructures());
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string>();
            foreach (FxLeg fxLeg in fxSingleLeg)
            {
                result.AddRange(fxLeg.GetRequiredCurrencies());
            }
            return result;
        }

    }
}
