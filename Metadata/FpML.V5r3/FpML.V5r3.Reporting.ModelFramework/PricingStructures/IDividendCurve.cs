using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.PricingStructures
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