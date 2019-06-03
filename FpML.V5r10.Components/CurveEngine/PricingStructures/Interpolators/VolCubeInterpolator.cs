#region Using directives

using System;
using System.Collections.Generic;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolators;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    public class TrilinearInterpolator : BilinearInterpolator
    {
        /// <summary>
        /// Perform a trilinear interpolation on the bounds to return a value
        /// For trilinear interpolation of surfaces we must assume
        ///     strike = x, tenor = y and expiry = z for the interpolator to return comprehensible reults
        /// The points must be arranged in the form [x0y0z0],[x1y0z0],[x0y1z0],[x1y1z0],[x0y0z1],[x1y0z1],[x0y1z1],[x1y1z1]
        /// A Point is assumed to be a <see cref="Point3D"/>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public override double Value(IPoint point, List<IPoint> bounds)
        {
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds), "Bounding points list cannot be null.");
            else if (point == null)
                throw new ArgumentNullException(nameof(point), "Interpolating point cannot be null.");
            else if (bounds.Count < 8)
                return base.Value(point, bounds);

            double x = (double)point.Coords[0];
            double y = (double)point.Coords[1];
            double z = (double)point.Coords[2];

            double x0 = (double)bounds[0].Coords[0];
            double y0 = (double)bounds[0].Coords[1];
            double z0 = (double)bounds[0].Coords[2];
            double fx0y0z0 = (double)bounds[0].Coords[3];

            double x1 = (double)bounds[1].Coords[0];
            double fx1y0z0 = (double)bounds[1].Coords[3];

            double y1 = (double)bounds[2].Coords[1];
            double fx0y1z0 = (double)bounds[2].Coords[3];

            double fx1y1z0 = (double)bounds[3].Coords[3];

            double z1 = (double)bounds[4].Coords[2];
            double fx0y0z1 = (double)bounds[4].Coords[3];

            double fx1y0z1 = (double)bounds[5].Coords[3];

            double fx0y1z1 = (double)bounds[6].Coords[3];

            double fx1y1z1 = (double)bounds[7].Coords[3];

            // perform bilinear interpolations on the z(min) and z(max) surfaces
            Point2D pt = new Point2D(x, y, 0);

            List<IPoint> XYbounds = new List<IPoint>();
            XYbounds.Add(new Point2D(x0, y0, fx0y0z0));
            XYbounds.Add(new Point2D(x1, y0, fx1y0z0));
            XYbounds.Add(new Point2D(x0, y1, fx0y1z0));
            XYbounds.Add(new Point2D(x1, y1, fx1y1z0));

            double fxyz1 = base.Value(pt, XYbounds);

            XYbounds = new List<IPoint>();
            XYbounds.Add(new Point2D(x0, y0, fx0y0z1));
            XYbounds.Add(new Point2D(x1, y0, fx1y0z1));
            XYbounds.Add(new Point2D(x0, y1, fx0y1z1));
            XYbounds.Add(new Point2D(x1, y1, fx1y1z1));

            double fxyz2 = base.Value(pt, XYbounds);

            // perform linear interpolation on the z axis
            // Now interpolate in the y direction
            Point ptZ = new Point(z);
            List<IPoint> boundsZ1 = new List<IPoint>();
            boundsZ1.Add(new Points.Point(z0, fxyz1));
            boundsZ1.Add(new Points.Point(z1, fxyz2));

            double fxyz = base.Value(ptZ, boundsZ1);
            return fxyz;
        }
    }
}