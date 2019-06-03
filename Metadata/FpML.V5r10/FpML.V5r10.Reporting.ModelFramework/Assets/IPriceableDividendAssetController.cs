using System;
using FpML.V5r10.Reporting.ModelFramework.Assets;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableDividendAssetController : IPriceableAssetController
    {

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DividendFactorAtMaturity { get; }
    }
}