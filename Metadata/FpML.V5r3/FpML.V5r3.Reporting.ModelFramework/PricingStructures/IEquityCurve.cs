using System;

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IEquityCurve : ICurve
    {
        /// <summary>
        /// Gets the equity factor.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetEquityFactor(DateTime baseDate, DateTime targetDate);
    }
}