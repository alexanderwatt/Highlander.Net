using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableRateAssetController : IPriceableAssetController
    {

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }

        /// <summary>
        /// Calculates the DF at maturity using a simple discount curve.
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns>The value.</returns>
        Decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace);
    }
}