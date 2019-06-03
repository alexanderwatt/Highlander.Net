using System;
using Orion.Constants;

namespace FpML.V5r10.Reporting.ModelFramework.Identifiers
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IAssetIdentifier : IIdentifier
    {
        /// <summary>
        /// BaseDate
        /// </summary>
        DateTime BaseDate { get; set; }

        /// <summary>
        /// MaturityDate
        /// </summary>
        DateTime? MaturityDate { get; set; }

        /// <summary>
        /// ProductType
        /// </summary>
        AssetTypesEnum AssetType { get; set; }

        ///<summary>
        /// The Coupon.
        ///</summary>
        Decimal? Coupon { get; set; }

        /// <summary>
        /// THe market rate
        /// </summary>
        Decimal? MarketQuote { get; set; }

        /// <summary>
        /// THe spread
        /// </summary>
        Decimal? Other { get; set; }

        /// <summary>
        /// THe Strike
        /// </summary>
        Decimal? Strike { get; set; }
    }
}