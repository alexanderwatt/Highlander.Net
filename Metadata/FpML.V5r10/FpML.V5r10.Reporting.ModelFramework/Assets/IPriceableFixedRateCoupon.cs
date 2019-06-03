using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    ///<summary>
    ///</summary>
    ///<typeparam name="AMP"></typeparam>
    ///<typeparam name="AMR"></typeparam>
    public interface IPriceableFixedRateCoupon<AMP, AMR> : IPriceableRateCoupon<AMP, AMR>
    {
        /// <summary>
        /// Gets the fixed rate.
        /// </summary>
        /// <value>The margin.</value>
        Decimal? Rate { get; }     
    }
}