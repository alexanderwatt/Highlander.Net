#region Using Directives

using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public interface IDiscreteSpace
    {
        /// <summary>
        /// This returns the numkber of dimensions of the point.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> dimensions.</returns>
        int GetNumDimensions();

        /// <summary>
        /// This returns the numkber of points.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> points.</returns>
        int GetNumPoints();

        /// <summary>
        /// The useable arrays for interpolation.
        /// </summary>
        /// <returns><c>double</c> The array values of the specified <c>int</c> dimension.</returns>
        double[] GetCoordinateArray(int dimension);

        /// <summary>
        /// The set of points in the discrete space.
        /// </summary>
        /// <returns><c>IPoint</c> The list  of the specified <c>IPoint</c> values.</returns>
        List<IPoint> GetPointList();

        /// <summary>
        /// The coordinate array data used for interpolation.
        /// </summary>
        /// <returns></returns>
        double[] GetFunctionValueArray();

        /// <summary>
        /// Sets the points in the discrete space.
        /// </summary>
        void SetPointList(List<IPoint> points);

        /// <summary>
        /// Used in interpolations.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        List<IPoint> GetClosestValues(IPoint pt);
    }
}
