using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// The Priceable base asset controller
    /// </summary>
    public interface IPriceableAssetController : IModelController<IAssetControllerData, BasicAssetValuation>
    {
        /// <summary>
        /// Gets and sets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        BasicQuotation MarketQuote { get; set; }

        /// <summary>
        /// Calculates the implied quote. For use with the fast bootstrapper.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace);

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        DateTime GetRiskMaturityDate();

        /// <summary>
        /// Store the original valuations
        /// </summary>
        BasicAssetValuation BasicAssetValuation { get; set; }
    }
}