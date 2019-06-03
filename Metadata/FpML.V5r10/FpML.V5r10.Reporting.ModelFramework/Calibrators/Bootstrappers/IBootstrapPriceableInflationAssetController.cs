
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// A bootstrap controller interface. 
    /// D - Denotes a generic type for bootstrap data
    /// R - Denotes a generic type for the results
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="R"></typeparam>
    public interface IBootstrapPriceableInflationAssetController<D, R> : IBootstrapController<D, R>
    {
        /// <summary>
        /// Calculates the specified bootstrap data.
        /// </summary>
        /// <param name="bootstrapData">The bootstrap data.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <returns></returns>
        R Calculate(D bootstrapData, IPriceableInflationAssetController[] priceableAssets);
    }
}