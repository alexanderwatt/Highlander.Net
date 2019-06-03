
namespace FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Rate Option stream
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableRateOptionStream<AMP, AMR> : IPriceableFloatingInterestRateStream<AMP, AMR>
    {
        ///// <summary>
        ///// Gets the strike price.
        ///// </summary>
        ///// <value>The strike price.</value>
        //Decimal StrikePrice { get; }

        /// <summary>
        /// Updates the name of the volatility curve.
        /// </summary>
        /// <param name="newVolatilityCurveName">New name of the volatility curve.</param>
        void UpdateVolatilityCurveName(string newVolatilityCurveName);
    }
}