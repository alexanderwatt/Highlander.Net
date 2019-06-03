#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r10.Reporting
{
    public partial class Swaption
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures()
        {
            var result = new List<String>();
            if (swap != null)
            {
                result.AddRange(swap.GetRequiredPricingStructures());
                result.AddRange(GetRequiredVolatilitySurfaces());
            }
            foreach (var payment in (premium ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredPricingStructures());
            foreach (var prem in (premium ?? new Payment[] { }))
                result.AddRange(prem.GetRequiredPricingStructures());
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredVolatilitySurfaces()
        {
            var result = new List<String>();
            var exercise = Item as EuropeanExercise;
            if (swap != null && exercise != null)
            {
                result.AddRange(swap.GetRequiredVolatilitySurfaces());
            }
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<String>();
            if (swap != null)
            {
                result.AddRange(swap.GetRequiredCurrencies());
            }
            foreach (var payment in (premium ?? new Payment[] { }))
                result.AddRange(payment.GetRequiredCurrencies());
            foreach (var prem in (premium ?? new Payment[] { }))
                result.AddRange(prem.GetRequiredCurrencies());
            return result.Distinct().ToList();
        }
    }
}
