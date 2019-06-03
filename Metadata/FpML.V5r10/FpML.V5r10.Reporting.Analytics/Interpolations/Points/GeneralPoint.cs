#region Using Directives

using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// In financial points we are interested mostly in curves and surface used for financial valuation.
    /// Unlike mathematical points, financial points at a minimum contain an x-coordinate and a function value.
    /// For ease of use, this is labelled as a one-dimensional point.
    /// </summary>
    public class GeneralPoint : Point
    {
        /// <summary>
        /// The ctor for the two dimensional point.
        /// </summary>
        /// <param name="array"><c>double</c> The array.</param>
        public GeneralPoint(double[] array)
            : base(array)
        {}
    }
}
