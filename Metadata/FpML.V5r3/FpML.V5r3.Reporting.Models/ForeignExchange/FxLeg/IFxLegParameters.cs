using System;

namespace Orion.Models.ForeignExchange.FxLeg
{
    public interface IFxLegParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        /// <value>The multiplier.</value>
        Decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency1.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //Decimal ExchangedCurrency1 { get; set; }

        ///// <summary>
        ///// Gets or sets the amount of exchange currency2.
        ///// </summary>
        ///// <value>The amount of exchange currency1.</value>
        //Decimal ExchangedCurrency2 { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency1 discount factor.</value>
        //Decimal Currency1DiscountFactor { get; set; }

        ///// <summary>
        ///// Gets or sets the currency discount factor.
        ///// </summary>
        ///// <value>The currency2 discount factor.</value>
        //Decimal Currency2DiscountFactor { get; set; }
    }
}