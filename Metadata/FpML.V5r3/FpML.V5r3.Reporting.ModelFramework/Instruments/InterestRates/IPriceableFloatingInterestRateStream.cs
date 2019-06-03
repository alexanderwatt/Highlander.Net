using System;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Floating Interest rate streem
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableFloatingInterestRateStream<AMP, AMR>: IPriceableInterestRateStream<AMP, AMR>
    {
        /// <summary>
        /// Gets the margin.
        /// </summary>
        /// <value>The margin.</value>
        Decimal Margin {get; }

        /// <summary>
        /// Gets the index of the floating rate.
        /// </summary>
        /// <value>The index of the floating rate.</value>
        FloatingRateIndex FloatingRateIndex {get; }

        /// <summary>
        /// Gets the index tenor.
        /// </summary>
        /// <value>The index tenor.</value>
        Period IndexTenor {get; }

        /// <summary>
        /// Gets the floating rate calculation.
        /// </summary>
        /// <value>The floating rate calculation.</value>
        FloatingRateCalculation FloatingRateCalculation {get; }

        /// <summary>
        /// Forecasts the index of the rate.
        /// </summary>
        /// <returns></returns>
        ForecastRateIndex ForecastRateIndex { get; }

        /// <summary>
        /// Gets the fixing date reset frequency.
        /// </summary>
        /// <value>The fixing date reset frequency.</value>
        ResetFrequency FixingDateResetFrequency {get; }

        /// <summary>
        /// Gets the reset relative to.
        /// </summary>
        /// <value>The reset relative to.</value>
        ResetRelativeToEnum ResetRelativeTo {get; }

        /// <summary>
        /// Updates the name of the forward curve.
        /// </summary>
        /// <param name="newForwardCurveName">New name of the forward curve.</param>
        void UpdateForwardCurveName(string newForwardCurveName);
    }
}