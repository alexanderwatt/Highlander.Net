namespace FpML.V5r10.Reporting.Models.Rates.FuturesOptions
{
    public class RateFuturesOptionAssetResults : IRateFuturesOptionAssetResults
    {
        #region IRateFuturesOptionAssetResults Members

        /// <summary>
        /// Gets the forward delta.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal NPV { get; set; }

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public decimal NPVChange { get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets the Index At Maturity.
        /// </summary>
        public decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the PandL.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal PandL { get; set; }

        /// <summary>
        /// Gets the intial margin.
        /// </summary>
        /// <value>The inital margin.</value>
        public decimal InitialMargin { get; set; }

        /// <summary>
        /// Gets the variation margin.
        /// </summary>
        /// <value>The variation margin.</value>
        public decimal VariationMargin { get; set; }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        public decimal ImpliedStrike { get; set; }

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        public decimal OptionVolatility { get; set; }

        /// <summary>
        /// Gets the spot delta.
        /// </summary>
        /// <value>The spot delta.</value>
        public decimal SpotDelta { get; set; }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal AccrualFactor { get; set; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public decimal ExpectedValue { get; set; }

        /// <summary>
        /// Gets the forward rate.
        /// </summary>
        /// <value>The forward rate.</value>
        public decimal ForwardRate { get; set; }

        /// <summary>
        /// Gets the $ derivative with respect to the Rate.
        /// </summary>
        /// <value>The $ delta wrt the fixed rate.</value>
        public decimal DeltaR { get; set; }

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
        /// Gets the first derivative with respect to the Vol.
        /// </summary>
        /// <value>The vega wrt the forward rate.</value>
        public decimal Vega0 { get; set; }

        /// <summary>
        /// Gets the second derivative with respect to the Time.
        /// </summary>
        /// <value>The theta wrt the forward rate.</value>
        public decimal Theta0 { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal VolatilityAtExpiry { get; set; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public decimal ConvexityAdjustment { get; set; }

        #endregion
    }
}