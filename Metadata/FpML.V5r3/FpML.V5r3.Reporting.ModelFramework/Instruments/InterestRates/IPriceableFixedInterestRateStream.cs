
namespace Orion.ModelFramework.Instruments.InterestRates
{
    ///<summary>
    ///</summary>
    ///<typeparam name="AMP"></typeparam>
    ///<typeparam name="AMR"></typeparam>
    public interface IPriceableFixedInterestRateStream<AMP, AMR>: IPriceableInterestRateStream<AMP, AMR>
    {
        /// <summary>
        /// Updates the rate.
        /// </summary>
        /// <param name="newRate">The new rate.</param>
        void UpdateRate(decimal newRate);
    }
}