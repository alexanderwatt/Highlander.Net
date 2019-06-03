
namespace Orion.ModelFramework.Instruments.InterestRates
{
    /// <summary>
    /// Base Interface for Caplet coupons
    /// </summary>
    /// <typeparam name="AMP">The type of the MP.</typeparam>
    /// <typeparam name="AMR">The type of the MR.</typeparam>
    public interface IPriceableCaplet<AMP, AMR> : IPriceableRateOption<AMP, AMR>
    {
    }
}