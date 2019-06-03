#region Usings

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.Instruments.ForeignExchange
{
    /// <summary>
    /// IPriceableFxLeg
    /// </summary>
    public interface IPriceableFxLeg<TAmp, TAmr> : IMetricsCalculation<TAmp, TAmr>
    {
        /// <summary>
        /// Gets or sets the payment in currency1.
        /// </summary>
        /// <value>The payment in currency1.</value>
        InstrumentControllerBase Currency1Payment { get; }

        /// <summary>
        /// Gets or sets the payment in currency2.
        /// </summary>
        /// <value>The payment in currency2.</value>
        InstrumentControllerBase Currency2Payment { get; }

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