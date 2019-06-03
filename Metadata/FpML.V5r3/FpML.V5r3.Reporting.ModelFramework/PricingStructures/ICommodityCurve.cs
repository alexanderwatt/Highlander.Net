using System;
using System.Collections.Generic;
using Orion.ModelFramework.Assets;

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ICommodityCurve : ICurve
    {
        /// <summary>
        /// Gets the commodity forward.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetForward(DateTime baseDate, DateTime targetDate);

        /// <summary>
        /// Gets the commodity forward.
        /// </summary>
        /// <param name="targetTime">The target time interval.</param>
        /// <returns></returns>
        double GetForward(Double targetTime);

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        List<IPriceableCommodityAssetController> PriceableCommodityAssets { get; }
    }
}