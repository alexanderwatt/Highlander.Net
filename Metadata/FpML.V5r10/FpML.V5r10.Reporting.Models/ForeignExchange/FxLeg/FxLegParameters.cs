
namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxLeg
{
    public class FxLegParameters : IFxLegParameters
    {
        #region Implementation of IFxLegParameters

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
        /// Gets or sets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exc { get; set; }hange currency1.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //public decimal ExchangedCurrency1 { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency2.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //public decimal ExchangedCurrency2 { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency1 discount factor.</value>
        //public decimal Currency1DiscountFactor { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency2 discount factor.</value>
        //public decimal Currency2DiscountFactor { get; set; }

        #endregion
    }
}