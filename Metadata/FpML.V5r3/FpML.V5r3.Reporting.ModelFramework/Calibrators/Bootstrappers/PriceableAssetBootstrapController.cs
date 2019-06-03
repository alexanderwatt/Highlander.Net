
using System.Reflection;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Calibrators.Bootstrappers;
using Orion.Util.Helpers;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base Bootstrap Controller class from which all bootstrapping controllers should be extended
    /// 
    /// </summary>
    abstract public class PriceableAssetBootstrapController: IBootstrapPriceableAssetController<IBootstrapControllerData, IPricingStructure>
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
        abstract public IPricingStructure Calculate(IBootstrapControllerData bootstrapData, IPriceableAssetController[] priceableAssets);


        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bootstrapResults">The bootstrap results.</param>
        /// <returns></returns>
        protected static IPricingStructure GetValue<T>(T bootstrapResults)
        {
            return (IPricingStructure)ObjectLookupHelper.GetPropertyValue(bootstrapResults, "PricingStructure");
        }

        #endregion

        ///<summary>
        ///</summary>
        protected PriceableAssetBootstrapController()
        {
            Id = string.Empty;
        }
    }
}