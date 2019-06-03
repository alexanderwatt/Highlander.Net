using System;

namespace Orion.Models.Assets
{
    public interface ISimpleDualAssetParameters
    {
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the base npv.
        /// </summary>
        /// <value>The base npv.</value>
        Decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the start discount factor.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the end discount factor.
        /// </summary>
        /// <value>The end discount factor.</value>
        Decimal EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the payment discount factor.
        /// </summary>
        /// <value>The payment discount factor.</value>
        Decimal PaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        Decimal YearFraction { get; set; }
    }
}