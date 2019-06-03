#region Using directives

using System;
using System.Collections.Generic;
using National.QRSC.FpML.V47;
using National.QRSC.ModelFramework.Instruments;
using National.QRSC.ModelFramework.Trades;

#endregion

namespace National.QRSC.ModelFramework
{
    ///<summary>
    ///</summary>
    public interface IProductPricer
    {
        ///<summary>
        /// Returns the relevant Pricing Structures.
        ///</summary>
        ///<returns></returns>
        List<String> RequiredPricingStructures { get; }
    }

    ///<summary>
    ///</summary>
    public abstract class ProductPricerBase : IProductPricer
    {
        ///<summary>
        /// Returns the relevant Pricing Structures.
        ///</summary>
        ///<returns></returns>
        public List<String> RequiredPricingStructures { get; protected set; }
    }
}