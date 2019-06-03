using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableClearedRateAssetController : IPriceableRateAssetController
    {
        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <param name="discountedSpace">The OIS Space.</param>
        /// <returns></returns>
        Decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace, IInterpolatedSpace discountedSpace);
    }
}