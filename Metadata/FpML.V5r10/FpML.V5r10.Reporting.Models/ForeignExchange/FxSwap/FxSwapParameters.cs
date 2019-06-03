
namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxSwap
{
    public class FxSwapParameters : IFxSwapParameters
    {
        #region Implementation of IFxSwapParameters

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        /// <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedStartCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedEndCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedStartCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        public decimal ExchangedEndCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        public decimal Currency1DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        public decimal Currency1DiscountFactor2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        public decimal Currency2DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        public decimal Currency2DiscountFactor2 { get; set; }

        #endregion
    }
}