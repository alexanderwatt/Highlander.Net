#region Using directives

using System;

using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
	/// <summary>
	/// Interface to the popular X-ibor index classes.
	/// </summary>
	/// <remarks>
	/// This interface is mainly for COM interop.
	/// </remarks>
	public interface IRateIndex : IIndex
    {
	    /// <summary>
	    /// Index family name.
	    /// </summary>
	    RelativeDateOffset GetResetDateConvention();

	    /// <summary>
	    /// The rate index.
	    /// </summary>
	    RateIndex GetRateIndex();

        /// <summary>
        /// The date adjustemnt rules of the index.
        /// </summary>
        BusinessDayAdjustments GetBusinessDayAdjustments();

        /// <summary>
        /// The maturity date of a specific rate index.
        /// </summary>
        /// <returns>The maturity date of the rate index from the base date supplied.</returns>
        DateTime GetRiskMaturityDate();

        /// <summary>
        /// Intialises the start date of a specific rate index.
        /// </summary>
        /// <param name="startDate"></param>
	    void SetIndexStartDate(DateTime startDate);

	    /// <summary>
	    /// Gets the initialised start date. Otherwise returns null.
	    /// </summary>
	    DateTime GetStartDate();

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="pricingStructure">The pricing structure.</param>
        /// <returns></returns>
        double GetValue(DateTime valuationDate, IRateCurve pricingStructure);
	}
}
