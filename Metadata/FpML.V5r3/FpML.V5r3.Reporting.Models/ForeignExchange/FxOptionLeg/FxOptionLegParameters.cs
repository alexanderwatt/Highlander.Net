#region Usings

using Orion.Models.ForeignExchange.FxLeg;

#endregion


namespace Orion.Models.ForeignExchange.FxOptionLeg
{
    public class FxOptionLegParameters : FxLegParameters, IFxOptionLegParameters
    {
        #region Implementation of IFxOptionLegParameters

        ///// <summary>
        ///// Gets or sets the metrics.
        ///// </summary>
        ///// <value>The metrics.</value>
        //public string[] Metrics { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency1.
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

        /// <summary>
        /// The is call flag;
        /// </summary>
        public bool IsCall { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public decimal? Premium { get; set; }

        #endregion
    }
}