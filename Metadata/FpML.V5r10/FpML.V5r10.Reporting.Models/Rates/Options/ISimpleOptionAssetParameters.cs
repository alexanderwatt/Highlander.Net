using System;
using FpML.V5r10.Reporting.Models.Assets;

namespace FpML.V5r10.Reporting.Models.Rates.Options
{
    public interface ISimpleOptionAssetParameters : ISimpleRateAssetParameters
    {
        /// <summary>
        /// Gets or sets the isput flag.
        /// </summary>
        /// <value>The isput flag.</value>
        Boolean IsPut { get; set; }

        /// <summary>
        /// Gets or sets the isVolatiltiyQuote flag.
        /// </summary>
        /// <value>The isVolatiltiyQuote flag.</value>
        Boolean IsVolatiltiyQuote { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        Boolean IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal Volatility { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        Decimal Strike { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        Decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the premium payment discount factor..
        /// </summary>
        /// <value>The premium payment discount factor.</value>
        Decimal PremiumPaymentDiscountFactor { get; set; }
    }
}