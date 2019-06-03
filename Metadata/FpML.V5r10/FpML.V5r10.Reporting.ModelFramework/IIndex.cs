#region Using directives

using System;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// Base Index interface
    /// </summary>
	public interface IIndex
	{
		/// <summary>
		/// A stringified representation of the Index.
		/// </summary>
		/// <returns>A String representing the object.</returns>
		String ToString();

        /// <summary>
        /// Index fixing at the given date.
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="fixingDate">The fixing date as a value date.</param>
        /// <param name="floatingCurve">The floating curve.</param>
        /// <returns>The fixing at the given date.</returns>
        /// <remarks>
        /// Any date passed as arguments must be a value date,
        /// i.e., the real calendar date advanced by a number of
        /// settlement days.
        /// </remarks>
        double GetFixing(DateTime valuationDate, DateTime fixingDate, IPricingStructure floatingCurve);

        /// <summary>
        /// Index fixing at the given date.
        /// </summary>
        /// <returns>The fixing date.</returns>
        DateTime GetFixingDate(DateTime baseDate);
	}
}
