#region using Directives

using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class Point2D : Point
    {

        #region Point Members

        #endregion

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 2d point value - x coordinate.</param>
        /// <param name="y"><c>double</c> The 2d point value - y coordinate.</param>
        public Point2D(double x, double y)
            : base(new[] {x, y , 0.0})
        {}

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 2d point value - x coordinate.</param>
        /// <param name="y"><c>double</c> The 2d point value - y coordinate.</param>
        /// <param name="value"></param>
        public Point2D(double x, double y, double value)
            : base(new[] { x, y, value })
        {}

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        public Point2D(double[] x)
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

        #endregion

    }
}
