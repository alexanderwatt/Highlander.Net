using Orion.Models.Rates.Options;

namespace Orion.Models.ForeignExchange
{
    public class FxOptionAssetParameters : ISimpleOptionAssetParameters
    {
        #region IRateOptionAssetParameters Members

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the base NPV.
        /// </summary>
        /// <value>The notional.</value>
        public decimal BaseNPV { get; set; }

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
        /// Gets or sets the isput flag.
        /// </summary>
        /// <value>The isput flag.</value>
        public bool IsPut { get; set; }

        /// <summary>
        /// Gets or sets the isVolatiltiyQuote flag.
        /// </summary>
        /// <value>The isVolatiltiyQuote flag.</value>
        public bool IsVolatiltiyQuote { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flag.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        public bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal Volatility { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The start premium.</value>
        public decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public decimal Strike { get; set; }

        /// <summary>
        /// Gets or sets the time to expiry.
        /// </summary>
        /// <value>The time to expiry.</value>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal PremiumPaymentDiscountFactor { get; set; }
        
        /// <summary>
        /// The payment discountfactor
        /// </summary>
        public decimal PaymentDiscountFactor { get; set; }

        #endregion
    }
}