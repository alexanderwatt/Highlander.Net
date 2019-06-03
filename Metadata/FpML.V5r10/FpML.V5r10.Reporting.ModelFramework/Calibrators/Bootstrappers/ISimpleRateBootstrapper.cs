using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Bootstrapping interface for simple rate assets
    /// </summary>
    public interface ISimpleRateBootstrapper : IBootstrapController<IBootstrapControllerData, IPricingStructure>
    {
        /// <summary>
        /// Gets the priceable rate assets.
        /// </summary>
        /// <value>The priceable assets.</value>
        IList<IPriceableRateAssetController> PriceableAssets { get; }
    }
}