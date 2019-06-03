using System;

namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxSwap
{
    public interface IFxSwapParameters //: IRateInstrumentParameters
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
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedStartCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency1.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedEndCurrency1 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedStartCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the amount of exchange currency2.
        /// </summary>
        /// <value>The amount of exchange currency1.</value>
        Decimal ExchangedEndCurrency2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        Decimal Currency1DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency1 discount factor.</value>
        Decimal Currency1DiscountFactor2 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        Decimal Currency2DiscountFactor1 { get; set; }

        /// <summary>
        /// Gets or sets the currency discount factor.
        /// </summary>
        /// <value>The currency2 discount factor.</value>
        Decimal Currency2DiscountFactor2 { get; set; }
    }
}