using System;

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ISpreadCurve : ICurve
    {
        /// <summary>
        /// The reference/parent curve id.
        /// </summary>
        IIdentifier ReferenceCurveId { get; }



        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetSpreadAdjustedValue(DateTime baseDate, DateTime targetDate);
    }
}