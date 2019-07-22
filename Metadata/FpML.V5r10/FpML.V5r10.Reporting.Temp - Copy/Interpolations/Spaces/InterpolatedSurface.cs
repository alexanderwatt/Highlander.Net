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

#region Using Directives

using System;
using Highlander.Numerics.LinearAlgebra;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class InterpolatedSurface : InterpolatedSpace
    {
        /// <summary>
        /// The interpolation type.
        /// </summary>
        protected readonly IInterpolation YInterpolation;
        protected readonly IInterpolation XInterpolation;

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="discreteSurface">The discrete surface ie 2 dimensional.</param>
        /// <param name="interpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="yInterpolation">The interpolation type for the y axis</param>
        /// <param name="allowExtrapolation">Not implemented in EO.</param>
        public InterpolatedSurface(IDiscreteSpace discreteSurface, IInterpolation interpolation,
             IInterpolation yInterpolation, bool allowExtrapolation)
            : base(discreteSurface, interpolation, allowExtrapolation)
        {
            if(discreteSurface.GetNumDimensions()!=2) throw new Exception("Wrong number of dimensions.");
            YInterpolation = yInterpolation;
            XInterpolation = interpolation;
        }

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="discreteSurface">The discrete surface ie 2 dimensional.</param>
        /// <param name="interpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="allowExtrapolation">Not implemented in EO.</param>
        public InterpolatedSurface(IDiscreteSpace discreteSurface, IInterpolation interpolation, bool allowExtrapolation)
            : this(discreteSurface, interpolation, interpolation, allowExtrapolation)
        {}

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="columns">The column values.</param>
        /// <param name="values">The discrete surface ie 2 dimensional.</param>
        /// <param name="interpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="allowExtrapolation">Not implemented in EO.</param>
        /// <param name="rows">The row values.</param>
        public InterpolatedSurface(double[] rows, double[] columns, Matrix values, IInterpolation interpolation, bool allowExtrapolation)
            : this(new DiscreteSurface(rows, columns, values), interpolation, interpolation, allowExtrapolation)
        {}

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public override double Value(IPoint point)
        {
            var curve = (DiscreteSpace)GetDiscreteSpace();
            var basePoints = curve.GetClosestValues(point);
            if (basePoints.Count != 4) throw new Exception("Incorrect number of base points.");
            var x0 = basePoints[0].Coords;
            var y0 = basePoints[1].Coords;//yo
            var x1 = basePoints[2].Coords;//x1
            var y1 = basePoints[3].Coords;//y1
            var xArray = new double[2];
            xArray[0] = (double)x0[0];
            xArray[1] = (double)x1[0];
            var fxArray = new double[2];
            fxArray[0] = (double)x0[2];
            fxArray[1] = (double)x1[2];
            var yArray = new double[2];
            yArray[0] = (double)y0[0];
            yArray[1] = (double)y1[0];
            var fyArray = new double[2];
            fyArray[0] = (double)y0[2];
            fyArray[1] = (double)y1[2];
            var xyArray = new double[2];
            xyArray[0] = (double)x0[1];
            xyArray[1] = (double)y0[1];
            //_interpolation.Initialize(xArray, fxArray);
            //var xy = (double) point.Coords[0];
            //var xyValue = _interpolation.ValueAt(xy, true);
            //_interpolation.Initialize(yArray, fyArray);
            //var yy = (double)point.Coords[0];
            //var yyValue = _interpolation.ValueAt(yy, true);
            //_yInterpolation.Initialize(xyArray, new[] { xyValue, yyValue });
            //var result = _yInterpolation.ValueAt((double)point.Coords[1], true);
            Double xyValue;
            Double yyValue;
            if (xArray[0].Equals(xArray[1]))
                xyValue = fxArray[0];
            else
            {
                Interpolation.Initialize(xArray, fxArray);
                var xy = (double)point.Coords[0];
                xyValue = Interpolation.ValueAt(xy, true);
            }

            if (yArray[0].Equals(yArray[1]))
                yyValue = fyArray[0];
            else
            {
                Interpolation.Initialize(yArray, fyArray);
                var yy = (double)point.Coords[0];
                yyValue = Interpolation.ValueAt(yy, true);
            }
            var yxArray = new[] { xyValue, yyValue };
            double result;
            if (xyArray[0] == xyArray[1])
            {
                result = fxArray[0];
            }
            else
            {
                YInterpolation.Initialize(xyArray, yxArray);
                result = YInterpolation.ValueAt((double)point.Coords[1], true);
            }
            return result;//TODO will not work with splines.
        }

        /// <summary>
        /// This returns the point function used to add continuity to the discrete space in the y axis.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> points.</returns>
        public IInterpolation GetYAxisInterpolatingFunction()
        {
            return YInterpolation;
        }
    }
}
