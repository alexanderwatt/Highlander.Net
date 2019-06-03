using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public interface ISwapDualAssetParameters
    {
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal StartDiscountFactor { get; set; }

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
    }
}