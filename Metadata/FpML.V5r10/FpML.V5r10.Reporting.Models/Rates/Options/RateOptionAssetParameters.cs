using System;
using System.Collections.Generic;

namespace FpML.V5r10.Reporting.Models.Rates.Options
{
    public class RateOptionAssetParameters : IRateOptionAssetParameters
    {
        public string[] Metrics { get; set; }

        #region IRateOptionAssetParameters Members

        /// <summary>
        /// Gets or sets the isDiscounted flags.
        /// </summary>
        /// <value>The isDiscounted flag.</value>
        public bool IsDiscounted { get; set; }

        /// <summary>
        /// Gets or sets the flat volatility.
        /// </summary>
        /// <value>The flat volatility.</value>
        public Decimal FlatVolatility { get; set; }

        /// <summary>
        /// Gets or sets the notionals.
        /// </summary>
        /// <value>The notional.</value>
        public List<double> Notionals { get; set; }

        /// <summary>
        /// Gets or sets the spot rate, ot the ATM rate.
        /// </summary>
        /// <value>The spot rate or ATM rate.</value>
        public double Rate { get; set; }

        /// <summary>
        /// Gets or sets the isput flags.
        /// </summary>
        /// <value>The isput flag.</value>
        public bool IsPut { get; set; }

        /// <summary>
        /// Gets or sets the ATM flag.
        /// </summary>
        /// <value>The ATM flag.</value>
        public bool IsATMForward { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The start premium.</value>
        public double Premium { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        public double PremiumPaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        public List<double> PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the forecast factors.
        /// </summary>
        /// <value>The forecast factors.</value>
        public List<double> ForecastDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the volatilities.
        /// </summary>
        /// <value>The volatilities.</value>
        public List<double> Volatilities { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        public List<double> ForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the times to expiry.
        /// </summary>
        /// <value>The times to expiry.</value>
        public List<double> TimesToExpiry { get; set; }

        /// <summary>
        /// Gets or sets the strike rates.
        /// </summary>
        /// <value>The strike rates.</value>
        public List<double> Strikes { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        public List<double> YearFractions { get; set; }

        #endregion
    }
}