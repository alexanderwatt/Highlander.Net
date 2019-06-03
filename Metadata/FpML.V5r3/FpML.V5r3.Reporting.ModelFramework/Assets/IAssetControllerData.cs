using System;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base interface defines the data required by all asset controllers (i.e. Type A Models)
    /// </summary>
    public interface IAssetControllerData
    {
        /// <summary>
        /// Gets the basic asset valuation.
        /// </summary>
        /// <value>The basic asset valuation.</value>
        BasicAssetValuation BasicAssetValuation { get; }

        /// <summary>
        /// Gets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        DateTime ValuationDate { get; }

        /// <summary>
        /// Gets the market environment.
        /// </summary>
        /// <value>The market environment.</value>
        IMarketEnvironment MarketEnvironment { get; }

        /// <summary>
        /// Creates the asset controller data.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        IAssetControllerData CreateAssetControllerData(string[] metrics, DateTime baseDate, IMarketEnvironment market);
    }
}