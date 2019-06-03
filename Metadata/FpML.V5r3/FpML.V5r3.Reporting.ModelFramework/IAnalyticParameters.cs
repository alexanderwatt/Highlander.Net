using System;

namespace Orion.ModelFramework
{
    /// <summary>
    /// Default base analytic parameters
    /// </summary>
    public interface IAnalyticParameters
    {
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        Decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the discount factor.
        /// </summary>
        /// <value>The discount factor.</value>
        Decimal DiscountFactor { get; set; }
    }
}
