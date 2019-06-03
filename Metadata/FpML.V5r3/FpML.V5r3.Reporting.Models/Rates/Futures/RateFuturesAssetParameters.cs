using System;
using Orion.Models.Futures;

namespace Orion.Models.Rates.Futures
{
    public class RateFuturesAssetParameters : FuturesAssetParameters, IRateFuturesAssetParameters
    {
        /// <summary>
        /// Gets or position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get; set; } = 1;

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public Decimal YearFraction { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public Decimal Volatility { get; set; }

        /// <summary>
        /// The time to exiry of the future.
        /// </summary>
        /// <value>The time to expiry of the future.</value>
        public Decimal TimeToExpiry { get; set; }
    }
}