#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IPropertyRateCurve : IRateCurve
    {
        /// <summary>
        /// Gets the  property return rate.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetPropertyRateValue(DateTime baseDate, DateTime targetDate);
    }
}