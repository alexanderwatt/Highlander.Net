using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableFuturesAssetController : IPriceableAssetController
    {
        /// <summary>
        /// Gets the exchange initial margin.
        /// </summary>
        /// <value>The exchange initial margin.</value>
        Decimal InitialMargin { get; }

        /// <summary>
        /// Gets the last settled value.
        /// </summary>
        /// <value>The last settled value.</value>
        Decimal LastSettledValue { get; }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>The current valuee.</value>
        Decimal IndexAtMaturity { get; }

        /// <summary>
        /// Gets the last trading date.
        /// </summary>
        /// <value>The last trading dat.</value>
        DateTime LastTradeDate { get; }

        /// <summary>
        /// Returns the futures expiry or the option expiry date.
        /// </summary>
        /// <returns></returns>
        DateTime GetBootstrapDate();
    }
}