
using System.Reflection;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Util.Helpers;


namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base Bootstrap Controller class from which all bootstrapping controllers should be extended
    /// 
    /// </summary>
    public abstract class PriceableCommodityAssetBootstrapController : IBootstrapPriceableCommodityAssetController<IBootstrapControllerData, IPricingStructure>
    {
        protected PriceableCommodityAssetBootstrapController()
        {
            Id = string.Empty;
        }

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
        public abstract IPricingStructure Calculate(IBootstrapControllerData bootstrapData);

        /// <summary>
        /// Calculates the specified bootstrap data.
        /// </summary>
        /// <param name="bootstrapData">The bootstrap data.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        public abstract IPricingStructure Calculate(IBootstrapControllerData bootstrapData, IPriceableCommodityAssetController[] priceableAssets);


        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bootstrapResults">The bootstrap results.</param>
        /// <returns></returns>
        protected static ICommodityCurve GetValue<T>(T bootstrapResults)
        {
            return (ICommodityCurve)ObjectLookupHelper.GetPropertyValue(bootstrapResults, "PricingStructure");
        }

        #endregion
    }
}