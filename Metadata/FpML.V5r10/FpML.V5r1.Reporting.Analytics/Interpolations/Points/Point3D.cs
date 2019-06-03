#region Using Direcctives

using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class Point3D : Point
    {
        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        /// <param name="y"><c>double</c> The 3d point value - y coordinate.</param>
        /// <param name="z"><c>double</c> The 3d point value - z coordinate.</param>
        public Point3D(double x, double y, double z)
            : base(new[] { x, y, z, 0.0 })
        {}

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        /// <param name="y"><c>double</c> The 3d point value - y coordinate.</param>
        /// <param name="z"><c>double</c> The 3d point value - z coordinate.</param>
        /// <param name="value"></param>
        public Point3D(double x, double y, double z, double value)
            : base(new[] { x, y, z, value })
        {}

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        public Point3D(double[] x)
            : base(x)
        {}

        #region Point Methods

        /// <summary>
        /// Gets the y coord.
        /// </summary>
        /// <returns></returns>
        public double GetY()
        {
            return Pointarray[1];
        }

        /// <summary>
        /// Gets the z coord.
        /// </summary>
        /// <returns></returns>
        public double GetZ()
        {
            return Pointarray[2];//TODO return 0 if index past end of array.
        }

        #endregion

    }
}
