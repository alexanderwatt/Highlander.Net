using System;
using Orion.Models.Rates.Futures;

namespace Orion.Models.Rates.FuturesOptions
{
    public interface IRateFuturesOptionAssetParameters : IRateFuturesAssetParameters
    {
        /// <summary>
        /// Is the option a put or a call?
        /// </summary>
        Boolean IsPut { get; set; }

        /// <summary>
        /// Is the quote of volatility type?
        /// </summary>
        Boolean IsVolatilityQuote { get; set; }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        /// <value>The strike.</value>
        Decimal Strike { get; set; }

        /// <summary>
        /// Gets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal Premium { get; set; }
    }
}