using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments.ForeignExchange
{
    /// <summary>
    /// IPriceableFxLeg
    /// </summary>
    public interface IPriceableFxSwap<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets the payments in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        IList<InstrumentControllerBase> Currency1Payments { get; }

        /// <summary>
        /// Gets the payment in currency2.
        /// </summary>
        /// <value>The payment in currency2.</value>
        IList<InstrumentControllerBase> Currency2Payments { get; }

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="currency1CurveName">New name of the currecny discount curve.</param>
        /// <param name="currency2CurveName">New name of the currecny discount curve.</param>
        void UpdateDiscountCurveNames(string currency1CurveName, string currency2CurveName);

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketting.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        void UpdateBucketingInterval(DateTime baseDate, Period bucketingInterval);
    }
}