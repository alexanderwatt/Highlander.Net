using System;
using FpML.V5r10.Reporting.Models.Futures;

namespace FpML.V5r10.Reporting.Models.Rates.Futures
{
    public interface IRateFuturesAssetParameters : IFuturesAssetParameters
    {
        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        Decimal YearFraction { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal Volatility { get; set; }

        /// <summary>
        /// The time to exiry of the future.
        /// </summary>
        /// <value>The time to expiry of the future.</value>
        Decimal TimeToExpiry { get; set; }
    }
}