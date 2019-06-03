using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// The Priceable base commodity asset controller
    /// </summary>
    public interface IPriceableCommodityAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the commodity asset forward value.
        /// </summary>
        /// <returns></returns>
        Decimal IndexAtMaturity { get; }
    }
}