using System;
using System.Collections.Generic;
using System.Linq;
using nab.QDS.FpML.V47.Extensions;

namespace nab.QDS.FpML.V47
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
            return result.Distinct().ToList<string>();
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredVolatilitySurfaces()
        {
            var result = new List<String>();
            result.Add(Helpers.GetRateVolatilityMatrixName(this));
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
