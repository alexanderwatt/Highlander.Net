﻿
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Calibrators.Bootstrappers;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base Bootstrap Controller class from which all bootstrapping controllers should be extended
    /// 
    /// </summary>
    abstract public class PriceableInflationAssetBootstrapController : IBootstrapPriceableInflationAssetController<IBootstrapControllerData, IPricingStructure>
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }


        /// <summary>
        /// Gets the model data.
        /// </summary>
        /// <value>The model data.</value>
        public IBootstrapControllerData ModelData { get; protected set; }

        #region IBootstrapController<IBootstrapControllerData> Members

        /// <summary>
        /// Calculates the specified bootstrap data.
        /// </summary>
        /// <param name="bootstrapData">The bootstrap data.</param>
        /// <returns></returns>
        abstract public IPricingStructure Calculate(IBootstrapControllerData bootstrapData);

        /// <summary>
        /// Calculates the specified bootstrap data.
        /// </summary>
        /// <param name="bootstrapData">The bootstrap data.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        /// 
        //ISTODO: does it have to be a part of interface???
        abstract public IPricingStructure Calculate(IBootstrapControllerData bootstrapData, IPriceableInflationAssetController[] priceableAssets);

        #endregion

        protected PriceableInflationAssetBootstrapController()
        {
            Id = string.Empty;
        }
    }
}