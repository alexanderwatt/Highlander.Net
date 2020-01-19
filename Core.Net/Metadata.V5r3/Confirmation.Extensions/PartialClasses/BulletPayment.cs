using System;
using System.Collections.Generic;

namespace nab.QDS.FpML.V47
{
    public partial class BulletPayment
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = payment.GetRequiredPricingStructures();
            return result;
        }

        public override List<String> GetRequiredCurrencies()
        {
            var result = payment.GetRequiredCurrencies();
            return result;
        }
    }
}
