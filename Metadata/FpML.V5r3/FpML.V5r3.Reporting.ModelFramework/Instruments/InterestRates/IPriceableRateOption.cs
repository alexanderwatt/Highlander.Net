using System;

namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Rate Option
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableRateOption<AMP, AMR> : IPriceableFloatingRateCoupon<AMP, AMR>
    {
        /// <summary>
        /// Gets the name of the volatility curve.
        /// </summary>
        /// <value>The name of the volatility curve.</value>
        string VolatilityCurveName { get; }

        /// <summary>
        /// Gets the strike price.
        /// </summary>
        /// <value>The strike price.</value>
        Decimal StrikePrice { get; }

        /// <summary>
        /// Updates the name of the volatility curve.
        /// </summary>
        /// <param name="newVolatilityCurveName">New name of the volatility curve.</param>
        void UpdateVolatilityCurveName(string newVolatilityCurveName);
    }
}