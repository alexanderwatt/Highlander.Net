using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r3.Confirmation
{
    public partial class Swap
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()
        {
            var result = new List<String>();
            foreach (var leg in swapStream)
                result.AddRange(leg.GetRequiredPricingStructures());
            foreach (var payment in (additionalPayment ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredPricingStructures());
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredVolatilitySurfaces()
        {
            var result = new List<String> {Helpers.GetRateVolatilityMatrixName(this)};
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String>();
            foreach (var leg in swapStream)
                result.AddRange(leg.GetRequiredCurrencies());
            foreach (var payment in (additionalPayment ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredCurrencies());
            return result.Distinct().ToList<string>();
        }
    }
}
