using System;

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IDividendCurve : IPricingStructure
    {
        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetDiscountFactor(DateTime baseDate, DateTime targetDate);
    }
}