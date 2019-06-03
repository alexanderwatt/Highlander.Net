
namespace FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Floor option stream
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableFloorStream<AMP, AMR> : IPriceableRateOptionStream<AMP, AMR>
    {
    }
}