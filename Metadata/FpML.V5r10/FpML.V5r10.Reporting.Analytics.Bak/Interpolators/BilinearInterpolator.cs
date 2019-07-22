#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Interpolations.Points;

#endregion

namespace Orion.Analytics.Interpolators
{
    /// <summary>
    /// Bilinear Interpolation
    /// </summary>
    public class BilinearInterpolator : LinearInterpolator
    {
        /// <summary>
        /// Perform a bilinear interpolation on the bounds to return a value.
        /// For bilinear interpolation of surfaces we must assume
        ///     strike = x and expiry = y for the interpolator to return comprehensible reults
        /// We must assume the bounds list is sorted into [x0y0], [x1y0], [x0,y1], [x1,y1]
        /// A Point is assumed to be a <see cref="Point2D"/>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public override double Value(IPoint point, List<IPoint> bounds)
        {
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds), "Bounding points list cannot be null.");
            if (point == null)
                throw new ArgumentNullException(nameof(point), "Interpolating point cannot be null.");
            if (bounds.Count < 4)
                return base.Value(point, bounds);

            var x = (double)point.Coords[0];
            var y = (double)point.Coords[1];

            var x0 = (double)bounds[0].Coords[0];
            var y0 = (double)bounds[0].Coords[1];
            var fx0y0 = (double)bounds[0].Coords[2];

            var x1 = (double)bounds[1].Coords[0];
            var fx1y0 = (double)bounds[1].Coords[2];

            var y1 = (double)bounds[2].Coords[1];
            var fx0y1 = (double)bounds[2].Coords[2];

            var fx1y1 = (double)bounds[3].Coords[2];

            // Interpolate in the x direction for fx()
            Point ptX = new Point1D(x);

            var boundsX1 = new List<IPoint> {new Point1D(x0, fx0y0), new Point1D(x1, fx1y0)};

            var boundsX2 = new List<IPoint> {new Point1D(x0, fx0y1), new Point1D(x1, fx1y1)};

            var fxy1 = base.Value(ptX, boundsX1);
            var fxy2 = base.Value(ptX, boundsX2);

            // Now interpolate in the y direction
            Point ptY = new Point1D(y);
            var boundsY1 = new List<IPoint> {new Point1D(y0, fxy1), new Point1D(y1, fxy2)};

            var fxy = base.Value(ptY, boundsY1);
            return fxy;
        }
    }
}