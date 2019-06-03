#region Using directives

using System;
using System.Collections.Generic;
using Orion.Analytics.Interpolations.Points;
using Orion.ModelFramework;

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
                throw new ArgumentNullException("bounds", "Bounding points list cannot be null.");
            if (point == null)
                throw new ArgumentNullException("point", "Interpolating point cannot be null.");
            if (bounds.Count < 2)
                return point.FunctionValue;

            double x = (double)point.Coords[0];

            double x0 = (double)bounds[0].Coords[0];
            double fx0 = (double)bounds[0].Coords[1];
            double x1 = (double)bounds[1].Coords[0];
            double fx1 = (double)bounds[1].Coords[1];

            //double fx = fx0 + (x - x0) * ((fx1 - fx0) / (x1 - x0));
            double fx;

            // if outside, or at, boundry, and point 0 is nearest, then use point 0
            if ((x1 >= x0 && x0 >= x) || (x1 <= x0 && x0 <= x))
            {
                fx = fx0;
            }
            // if outside, or at, boundry, and point 1 is nearest, then use point 1
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
      

    }
}