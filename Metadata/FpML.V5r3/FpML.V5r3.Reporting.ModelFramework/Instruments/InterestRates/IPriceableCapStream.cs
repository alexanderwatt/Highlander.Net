
namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Cap option stream
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableCapStream<AMP, AMR> : IPriceableRateOptionStream<AMP, AMR>
    {
    }
}