using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// The Priceable base commodity asset controller
    /// </summary>
    public interface IPriceableEquityAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the commodity asset forward value.
        /// </summary>
        /// <returns></returns>
        Decimal Price { get; }

        /// <summary>
        /// Gets the index at matiurity.
        /// </summary>
        /// <returns></returns>
        Decimal IndexAtMaturity { get; }

        ///<summary>
        /// Gets the date on which the eqity settles.
        ///</summary>
        DateTime SettlementDate { get; set; }

        /// <summary>
        /// THe equity valuation curve.
        /// </summary>
        string EquityCurveName { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }
    }
}