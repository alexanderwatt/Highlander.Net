namespace FpML.V5r10.Reporting.Models.Futures
{
    public class FuturesAssetResults : FuturesAssetParameters, IFuturesAssetResults
    {
        #region Implementation of IEquityAssetResults

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal NPV{ get; set; }

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
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public decimal PandL { get; set; }

        /// <summary>
        /// The inital margin
        /// </summary>
        public decimal InitialMargin { get; set; }

        /// <summary>
        /// THe variation margin
        /// </summary>
        public decimal VariationMargin { get; set; }

        /// <summary>
        /// Gets the Index At Maturity.
        /// </summary>
        public decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        public decimal Strike { get; set; }

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        public decimal OptionVolatility { get; set; }

        #endregion
    }
}