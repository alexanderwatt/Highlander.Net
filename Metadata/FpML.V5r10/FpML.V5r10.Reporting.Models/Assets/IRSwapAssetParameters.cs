using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public class IRSwapAssetParameters : ISwapAssetParameters
    {
        /// <summary>
        /// The ctor
        /// </summary>
        public IRSwapAssetParameters()
        {
            NotionalAmount = 1.0m;
        }

        public string[] Metrics { get; set; }

        #region ISwapAssetParameters Members

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the base NPV.
        /// </summary>
        /// <value>The notional.</value>
        public decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        public Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        public Decimal[] FloatingLegForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        public Decimal[] DiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        public Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public Decimal[] YearFractions { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        public decimal[] FloatingLegWeightings { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        public decimal[] FloatingLegDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal FloatingLegSpread { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        public decimal[] FloatingLegYearFractions { get; set; }

        /// <summary>
        /// Gets or sets the forecast discount factors.
        /// </summary>
        /// <value>The forecast discount factors.</value>
        public decimal[] FloatingLegForecastDiscountFactors { get; set; }

        #endregion
    }
}