/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;

#endregion

namespace Orion.Analytics.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class DiscreteCurve : DiscreteSpace
    {
        /// <summary>
        /// x coordinate array.
        /// </summary>
        protected readonly IList<double> XArray;

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="points"></param>
        public DiscreteCurve(IList<IPoint> points)
            : base(1, points)
        {
            XArray = Map(points);
        }

        /// <summary>
        /// Useful ctor
        /// </summary>
        /// <param name="xArray"></param>
        /// <param name="vArray"></param>
        public DiscreteCurve(IList<double> xArray, IList<double> vArray)
            : base(1, PointHelpers.Point1D(xArray, vArray))
        {
            XArray = xArray;
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
          return XArray.ToArray();
      }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override List<IPoint> GetClosestValues(IPoint pt)
        {
            if (pt.Coords.Count != 2) throw new NotImplementedException();
            var times = GetCoordinateArray(1);
            var values = GetFunctionValueArray();
            var index = Array.BinarySearch(times, pt.Coords[0]);
            int nextIndex;
            int prevIndex;
            if (index >= 0)
            {
                if (index < times.Length-1)
                {
                    prevIndex = index;
                    nextIndex = index + 1;
                }
                else
                {
                    prevIndex = index - 1;
                    nextIndex = index;
                }
            }
            else
            {
                nextIndex = ~index;
                prevIndex = nextIndex - 1;
            }
            var result = new List<IPoint> { new Point1D(times[prevIndex], values[prevIndex]), new Point1D(times[nextIndex], values[nextIndex]) };
            return result;
        }

        /// <summary>
      /// Converts to a matrix for mathematical manipulation.
      /// </summary>
      /// <returns></returns>
      private static double[] Map(IList<IPoint> points)
      {
          var xArray = new double[points.Count];
          for (var i = 0; i < points.Count; i++)
          {
              xArray[i] = (double)points[i].Coords[0];
          }
          return xArray;
      }
    }
}
