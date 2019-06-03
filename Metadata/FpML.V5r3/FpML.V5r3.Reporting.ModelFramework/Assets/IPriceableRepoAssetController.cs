using System;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableRepoAssetController : IPriceableAssetController
    {

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }
    }
}