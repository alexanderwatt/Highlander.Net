#region Using Directives

#endregion

namespace Orion.ModelFramework
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public interface IInterpolatedSpace : IPointFunction
    {
        /// <summary>
        /// This returns the point function used to add continuity to the discrete space.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> points.</returns>
        IInterpolation GetInterpolatingFunction();

        /// <summary>
        /// Gets the nunderlying discrete space that is used for interpolation.
        /// </summary>
        /// <returns></returns>
        IDiscreteSpace GetDiscreteSpace();

        /// <summary>
        /// Is extrapolartion allowed?
        /// </summary>
        /// <returns></returns>
        bool AllowExtrapolation();
    }
}
