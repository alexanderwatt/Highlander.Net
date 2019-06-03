using Orion.Models.Futures;

namespace Orion.Models.Rates.FuturesOptions
{
    public class RateFuturesOptionAssetParameters : FuturesOptionAssetParameters, IRateFuturesOptionAssetParameters
    {
        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        public decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal Volatility { get; set; }

        /// <summary>
        /// The time to exiry of the future.
        /// </summary>
        /// <value>The time to expiry of the future.</value>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Is the option a put or a call?
        /// </summary>
        public bool IsPut { get; set; }

        /// <summary>
        /// Is the quote of volatility type?
        /// </summary>
        public bool IsVolatilityQuote { get; set; }

        /// <summary>
        /// Gets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public decimal Premium { get; set; }
    }
}