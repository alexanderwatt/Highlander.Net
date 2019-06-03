#region Using Directives

using System;
using System.Collections.Generic;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;
using Orion.Numerics.LinearAlgebra;

#endregion

namespace Orion.Analytics.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class DiscreteCube : DiscreteSpace
    {
        /// <summary>
        /// x coordinate array.
        /// </summary>
        private double[] _xArray;

        /// <summary>
        /// x coordinate array.
        /// </summary>
        private double[] _yArray;

        /// <summary>
        /// x coordinate array.
        /// </summary>
        private double[] _zArray;

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="points"></param>
        public DiscreteCube(IList<IPoint> points)
            : base(3, points)
        {
            Map(points);
        }

        /// <summary>
        /// Useful ctor
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="vMatrix"></param>
        public DiscreteCube(double[] rows, double[] columns, Matrix vMatrix)
            : base(2, PointHelpers.Point2D(rows, columns, vMatrix))
        {
            Map(PointHelpers.Point2D(rows, columns, vMatrix));
        }

        /// <summary>
        /// The coordinate array data used for interpolation.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public override double[] GetCoordinateArray(int dimension)
        {
            if (dimension > GetNumDimensions())
                throw new ArgumentException(
                    "TODO: Dimension is not of this rank");
            return _xArray;
        }

        /// <summary>
        /// Used in interpolations.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override List<IPoint> GetClosestValues(IPoint pt)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Converts to a matrix for mathematical manipulation.
        /// </summary>
        /// <returns></returns>
        protected void Map(IList<IPoint> points)
        {
            _xArray = new double[points.Count];
            _yArray = new double[points.Count];
            _zArray = new double[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                _xArray[i] = (double) points[i].Coords[1];
                _yArray[i] = points[i].FunctionValue;
            }
        }

    }
}