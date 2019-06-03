#region Usings

using System;
using FpML.V5r10.Reporting.Models.ForeignExchange.FxLeg;

#endregion

namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxOptionLeg
{
    public interface IFxOptionLegParameters : IFxLegParameters
    {
        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal? Premium { get; set; }

        /// <summary>
        /// The is call flag;
        /// </summary>
        bool IsCall { get; set; }

        ///// <summary>
        ///// Gets or sets the metrics.
        ///// </summary>
        ///// <value>The metrics.</value>
        //string[] Metrics { get; set; }

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