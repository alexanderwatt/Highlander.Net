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
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Interpolations.Points;

#endregion

namespace Orion.Analytics.Interpolators
{
    /// <summary>
    /// Linear Interpolator base class
    /// </summary>
   
    public class LinearInterpolator : Interpolator 
    {    
        /// <summary>
        /// Perform a linear interpolation on the bounds to return a value
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work
        /// A Point is assumed to be a <see cref="Point"/>
        /// </summary>
        /// <param name="point">This is a 1d point</param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public override double Value(IPoint point, List<IPoint> bounds)
        {
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds), "Bounding points list cannot be null.");
            if (point == null)
                throw new ArgumentNullException(nameof(point), "Interpolating point cannot be null.");
            if (bounds.Count < 2)
                return point.FunctionValue;
            var x = (double)point.Coords[0];
            var x0 = (double)bounds[0].Coords[0];
            var fx0 = (double)bounds[0].Coords[1];
            var x1 = (double)bounds[1].Coords[0];
            var fx1 = (double)bounds[1].Coords[1];
            //double fx = fx0 + (x - x0) * ((fx1 - fx0) / (x1 - x0));
            double fx;
            // if outside, or at, boundary, and point 0 is nearest, then use point 0
            if ((x1 >= x0 && x0 >= x) || (x1 <= x0 && x0 <= x))
            {
                fx = fx0;
            }
            // if outside, or at, boundary, and point 1 is nearest, then use point 1
            else if ((x0 >= x1 && x1 >= x) || (x0 <= x1 && x1 <= x))
            {
                fx = fx1;
            }
            // otherwise interpolate
            else
            {
                double gradient = (x1 == x0) ? 0 : ((fx1 - fx0) / (x1 - x0));
                fx = fx0 + (x - x0) * gradient;
            }
            return fx;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public override double ValueAt(double point, bool bounds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void Initialize(double[] x, double[] y)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            throw new NotImplementedException();
        }
    }
}