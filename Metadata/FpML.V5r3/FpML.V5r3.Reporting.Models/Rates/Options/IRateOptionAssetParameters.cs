using System;
using System.Collections.Generic;

namespace Orion.Models.Rates.Options
{
    public interface IRateOptionAssetParameters
    {
        /// <summary>
        /// Gets or sets the isput flags.
        /// </summary>
        /// <value>The isput flag.</value>
        Boolean IsPut { get; set; }

        /// <summary>
        /// Gets or sets the isDiscounted flags.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        Boolean IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        Decimal FlatVolatility { get; set; }

        /// <summary>
        /// Gets or sets the notionals.
        /// </summary>
        /// <value>The notional.</value>
        List<double> Notionals { get; set; }

        /// <summary>
        /// Gets or sets the spot rate, ot the ATM rate.
        /// </summary>
        /// <value>The spot rate or ATM rate.</value>
        double Rate { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The start premium.</value>
        double Premium { get; set; }

        /// <summary>
        /// Gets or sets the ATM flag.
        /// </summary>
        /// <value>The ATM flag.</value>
        bool IsATMForward { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        double PremiumPaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        List<double> PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the forecast factors.
        /// </summary>
        /// <value>The forecast factors.</value>
        List<double> ForecastDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the volatilities.
        /// </summary>
        /// <value>The volatilities.</value>
        List<double> Volatilities { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        List<double> ForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the times to expiry.
        /// </summary>
        /// <value>The times to expiry.</value>
        List<double> TimesToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the strike rates.
        /// </summary>
        /// <value>The strike rates.</value>
        List<double> Strikes { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        List<double> YearFractions { get; set; }
    }
}