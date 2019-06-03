using System;

namespace Orion.Models.Assets
{
    public interface ISwapAssetParameters
    {
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the base NPV.
        /// </summary>
        /// <value>The notional.</value>
        Decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the forward rates.
        /// </summary>
        /// <value>The forward rates.</value>
        Decimal[] FloatingLegForwardRates { get; set; }

        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        Decimal[] DiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        Decimal[] YearFractions { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        Decimal[] FloatingLegWeightings { get; set; }
        
        /// <summary>
        /// Gets or sets the discount factors.
        /// </summary>
        /// <value>The discount factors.</value>
        Decimal[] FloatingLegDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal FloatingLegSpread { get; set; }

        /// <summary>
        /// Gets or sets the year fractions.
        /// </summary>
        /// <value>The year fractions.</value>
        Decimal[] FloatingLegYearFractions { get; set; }

        /// <summary>
        /// Gets or sets the forecast discount factors.
        /// </summary>
        /// <value>The forecast discount factors.</value>
        Decimal[] FloatingLegForecastDiscountFactors { get; set; }
    }
}