using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Util.Helpers;


namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base Bootstrap Controller class from which all bootstrapping controllers should be extended
    /// 
    /// </summary>
    public abstract class PriceableRepoAssetBootstrapController : IBootstrapPriceableRepoAssetController<IBootstrapControllerData, IPricingStructure>
    {
        ///<summary>
        ///</summary>
        protected PriceableRepoAssetBootstrapController()
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
        public abstract IPricingStructure Calculate(IBootstrapControllerData bootstrapData, IPriceableRepoAssetController[] priceableAssets);


        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bootstrapResults">The bootstrap results.</param>
        /// <returns></returns>
        protected static IRepoCurve GetValue<T>(T bootstrapResults)
        {
            return (IRepoCurve)ObjectLookupHelper.GetPropertyValue(bootstrapResults, "PricingStructure");
        }

        #endregion
    }
}