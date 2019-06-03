#region Usings

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Instruments.Commodities
{
    /// <summary>
    /// IPriceableFxLeg
    /// </summary>
    public interface IPriceableCommoditySwapLeg<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets or sets the payment in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        InstrumentControllerBase PriceablePayment { get; }

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketting.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}