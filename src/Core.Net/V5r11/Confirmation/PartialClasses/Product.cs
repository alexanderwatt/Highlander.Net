using System;
using System.Collections.Generic;

namespace FpML.V5r3.Confirmation
{
    public partial class Product
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public virtual List<String> GetRequiredPricingStructures() 
        {
            // this class is abstract
            // - derived classes must implement this method
            return new List<string>();
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public virtual List<String> GetRequiredCurrencies()
        {
            // this class is abstract
            // - derived classes must implement this method
            return new List<string>();
        }
    }
}
