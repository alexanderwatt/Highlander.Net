/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.Analytics.Interpolations.Points;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Interpolators
{
    ///<summary>
    ///</summary>
    public class TrilinearInterpolator : BilinearInterpolator
    {
        /// <summary>
        /// Perform a tri-linear interpolation on the bounds to return a value
        /// For tri-linear interpolation of surfaces we must assume
        ///     strike = x, tenor = y and expiry = z for the interpolator to return comprehensible results
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
            if (point == null)
                throw new ArgumentNullException(nameof(point), "Interpolating point cannot be null.");
            if (bounds.Count < 8)
                return base.Value(point, bounds);
            var x = (double)point.Coords[0];
            var y = (double)point.Coords[1];
            var z = (double)point.Coords[2];
            var x0 = (double)bounds[0].Coords[0];
            var y0 = (double)bounds[0].Coords[1];
            var z0 = (double)bounds[0].Coords[2];
            var fx0y0z0 = (double)bounds[0].Coords[3];
            var x1 = (double)bounds[1].Coords[0];
            var fx1y0z0 = (double)bounds[1].Coords[3];
            var y1 = (double)bounds[2].Coords[1];
            var fx0y1z0 = (double)bounds[2].Coords[3];
            var fx1y1z0 = (double)bounds[3].Coords[3];
            var z1 = (double)bounds[4].Coords[2];
            var fx0y0z1 = (double)bounds[4].Coords[3];
            var fx1y0z1 = (double)bounds[5].Coords[3];
            var fx0y1z1 = (double)bounds[6].Coords[3];
            var fx1y1z1 = (double)bounds[7].Coords[3];
            // perform bilinear interpolations on the z(min) and z(max) surfaces
            var pt = new Point2D(x, y, 0);
            var XYbounds = new List<IPoint>
                               {
                                   new Point2D(x0, y0, fx0y0z0),
                                   new Point2D(x1, y0, fx1y0z0),
                                   new Point2D(x0, y1, fx0y1z0),
                                   new Point2D(x1, y1, fx1y1z0)
                               };
            var fxyz1 = base.Value(pt, XYbounds);
            XYbounds = new List<IPoint>
                           {
                               new Point2D(x0, y0, fx0y0z1),
                               new Point2D(x1, y0, fx1y0z1),
                               new Point2D(x0, y1, fx0y1z1),
                               new Point2D(x1, y1, fx1y1z1)
                           };
            var fxyz2 = base.Value(pt, XYbounds);
            // perform linear interpolation on the z axis
            // Now interpolate in the y direction
            Point ptZ = new Point1D(z);
            var boundsZ1 = new List<IPoint> {new Point1D(z0, fxyz1), new Point1D(z1, fxyz2)};
            var fxyz = base.Value(ptZ, boundsZ1);
            return fxyz;
        }
    }
}