using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public interface IZeroRateAssetParameters : ISimpleRateAssetParameters
    {
        /// <summary>
        /// Gets or sets the compounding frequency.
        /// </summary>
        ///  <value>The frequency.</value>
        Decimal PeriodAsTimesPerYear { get; set; }
    }
}