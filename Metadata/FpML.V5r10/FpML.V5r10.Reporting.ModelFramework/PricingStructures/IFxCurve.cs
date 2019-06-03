using System;

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IFxCurve : ICurve, ICloneable
    {
        /// <summary>
        /// Gets the fx forward.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        double GetForward(DateTime baseDate, DateTime targetDate);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        decimal GetSpotRate();
    }
}