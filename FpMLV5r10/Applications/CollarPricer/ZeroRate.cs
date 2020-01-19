using System;
using Orion.EquityCollarPricer.Helpers;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// A Zero Rate
    /// </summary>
    public class ZeroRate
    {
        /// <summary>
        /// Gets the tenor date.
        /// </summary>
        /// <value>The tenor date.</value>
        public DateTime TenorDate { get; private set; }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public double Rate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroRate"/> class.
        /// </summary>
        /// <param name="tenorDate">The tenor date.</param>
        /// <param name="rate">The rate.</param>
        public ZeroRate(DateTime tenorDate, Double rate)
        {
            InputValidator.NotNull("Tenor Date", rate, true);
            InputValidator.NotZero("Rate", rate, true);
            TenorDate = tenorDate;
            Rate = rate;
        }
    }
}
