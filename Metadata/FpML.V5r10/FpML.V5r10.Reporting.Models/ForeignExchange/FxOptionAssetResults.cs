using System;

namespace FpML.V5r10.Reporting.Models.ForeignExchange
{
    public class FxOptionAssetResults : IFxOptionAssetResults
    {
        /// <summary>
        /// Gets the forward delta the fixed rate.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        #region IRateOptionAssetResults Members

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal NPV { get; set; }

        /// <summary>
        /// Gets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public Decimal Premium { get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta0 { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta1 { get; set; }

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        public decimal Gamma0 { get; set; }

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        public decimal Gamma1 { get; set; }

        /// <summary>
        /// Gets the accrual factor
        /// </summary>
        public decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal VolatilityAtExpiry { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote { get; set; }

        #endregion
    }
}