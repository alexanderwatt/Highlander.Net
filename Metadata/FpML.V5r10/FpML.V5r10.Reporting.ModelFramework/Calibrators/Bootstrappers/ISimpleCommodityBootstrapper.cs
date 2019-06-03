using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
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