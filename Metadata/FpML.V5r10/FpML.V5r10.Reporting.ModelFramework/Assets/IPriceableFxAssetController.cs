using System;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// The Priceable base commodity asset controller
    /// </summary>
    public interface IPriceableFxAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the commodity asset forward value.
        /// </summary>
        /// <returns></returns>
        Decimal ForwardAtMaturity { get; }
    }
}