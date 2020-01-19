using System;
using System.Collections.Generic;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Calibrators.Bootstrappers;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
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