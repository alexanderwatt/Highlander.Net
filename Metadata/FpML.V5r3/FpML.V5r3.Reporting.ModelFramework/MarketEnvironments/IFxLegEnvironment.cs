

namespace Orion.ModelFramework.MarketEnvironments
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface IFxLegEnvironment : IMarketEnvironment
    {
        ///<summary>
        /// Gets the relevant environment.
        ///</summary>
        ///<returns></returns>
        ISwapLegEnvironment GetExchangeCurrencyPaymentEnvironment1();

        ///<summary>
        /// Gets the relevant environment.
        ///</summary>
        ///<returns></returns>
        ISwapLegEnvironment GetExchangeCurrencyPaymentEnvironment2();
    }
}