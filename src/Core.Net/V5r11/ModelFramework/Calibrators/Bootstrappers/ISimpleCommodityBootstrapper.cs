using System;
using System.Collections.Generic;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Calibrators.Bootstrappers;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Bootstrapping interface for simple commodity assets
    /// </summary>
    public interface ISimpleCommodityBootstrapper : IBootstrapController<IBootstrapControllerData, IPricingStructure>
    {
        ///<summary>
        /// Returns the commodity priceables.
        ///</summary>
        IList<IPriceableCommodityAssetController> PriceableAssets { get; }
    }
}