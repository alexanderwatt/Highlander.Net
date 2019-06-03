using System;
using System.Collections.Generic;
using System.Linq;

namespace FpML.V5r10.Reporting
{
    public partial class CapFloor
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()
        {
            var result = new List<String>();
            if (capFloorStream != null)
            {
                result.AddRange(capFloorStream.GetRequiredPricingStructures());
                result.AddRange(capFloorStream.GetRequiredVolatilitySurfaces());
            }
            foreach (var payment in (additionalPayment ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredPricingStructures());
            foreach (var prem in (premium ?? new Payment[] { }))
                result.AddRange(prem.GetRequiredPricingStructures());
            return result.Distinct().ToList();
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String>();
            if (capFloorStream != null)
            {
                result.AddRange(capFloorStream.GetRequiredCurrencies());
            }
            foreach (var payment in (additionalPayment ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredCurrencies());
            foreach (var prem in (premium ?? new Payment[] { }))
                result.AddRange(prem.GetRequiredCurrencies());
            return result.Distinct().ToList();
        }
    }
}
