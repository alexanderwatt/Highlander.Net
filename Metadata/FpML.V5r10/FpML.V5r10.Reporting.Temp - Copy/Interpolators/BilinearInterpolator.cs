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
    /// <summary>
    /// Bilinear Interpolation
    /// </summary>
    public class BilinearInterpolator : LinearInterpolator
    {
        /// <summary>
        /// Perform a bilinear interpolation on the bounds to return a value.
        /// For bilinear interpolation of surfaces we must assume
        ///     strike = x and expiry = y for the interpolator to return comprehensible results
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
            var fX0Y0 = (double)bounds[0].Coords[2];
            var x1 = (double)bounds[1].Coords[0];
            var fX1Y0 = (double)bounds[1].Coords[2];
            var y1 = (double)bounds[2].Coords[1];
            var fX0Y1 = (double)bounds[2].Coords[2];
            var fX1Y1 = (double)bounds[3].Coords[2];
            // Interpolate in the x direction for fx()
            Point ptX = new Point1D(x);
            var boundsX1 = new List<IPoint> {new Point1D(x0, fX0Y0), new Point1D(x1, fX1Y0)};
            var boundsX2 = new List<IPoint> {new Point1D(x0, fX0Y1), new Point1D(x1, fX1Y1)};
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