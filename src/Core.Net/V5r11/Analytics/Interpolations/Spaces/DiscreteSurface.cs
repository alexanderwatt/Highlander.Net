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
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class DiscreteSurface : DiscreteSpace
    {
        /// <summary>
        /// The value matrix.
        /// </summary>
        private Matrix _vMatrix;

        /// <summary>
        /// x coordinate array.
        /// </summary>
        public double[] XArray { get; private set; }

        /// <summary>
        /// x coordinate array.
        /// </summary>
        public double[] YArray { get; private set; }

        /// <summary>
        /// Main ctor. It is assumed that the basic structure of 2 dimensional points can be represented as a Matrix.
        /// </summary>
        /// <param name="points"></param>
        public DiscreteSurface(IList<IPoint> points)
            : base(2, points)
        {
            Map(points);
        }

        /// <summary>
        /// Useful ctor
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="vMatrix"></param>
        public DiscreteSurface(double[] rows, double[] columns, Matrix vMatrix)
            : base(2, PointHelpers.Point2D(rows, columns, vMatrix))
        {
            XArray = rows;
            YArray = columns;
            _vMatrix = vMatrix;
        }

        /// <summary>
        /// Gets the value matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix GetMatrixOfValues()
        {
            return _vMatrix;
        }

        /// <summary>
        /// The coordinate array data used for interpolation.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public override double[] GetCoordinateArray(int dimension)
        {
            if (dimension == 1)
            {
                return XArray;
            }
            if (dimension == 2)
            {
                return YArray;
            }
            throw new ArgumentException(
                "TODO: Dimension is not of this rank");
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
            if(pt.Coords.Count!=3) throw new NotImplementedException();

            var xtimes = GetCoordinateArray(1);
            //var xlength = xtimes.Length - 1;
            var ytimes = GetCoordinateArray(2);
            //var ylength = ytimes.Length - 1;
            int[] xIndexes = CalculateIndexes((double)pt.Coords[0], xtimes);
            int[] yIndexes = CalculateIndexes((double)pt.Coords[1], ytimes);
            var values = GetMatrixOfValues();
            //var xindex = Array.BinarySearch(xtimes, pt.Coords[0]);
            //var yindex = Array.BinarySearch(ytimes, pt.Coords[1]);
            //int nextxIndex;
            //int prevxIndex;
            //if (xindex >= 0)
            //{
            //    if (xindex < xlength)
            //    {
            //        prevxIndex = xindex;
            //        nextxIndex = xindex + 1;
            //    }
            //    else
            //    {
            //        prevxIndex = xindex - 1;
            //        nextxIndex = xindex;
            //    }
            //}
            //else
            //{
            //    nextxIndex = ~xindex;
            //    prevxIndex = nextxIndex - 1;
            //}
            //int nextyIndex;
            //int prevyIndex;
            //if (yindex >= 0)
            //{
            //    if (yindex < ylength)
            //    {
            //        prevyIndex = yindex;
            //        nextyIndex = yindex + 1;
            //    }
            //    else
            //    {
            //        prevyIndex = yindex - 1;
            //        nextyIndex = yindex;
            //    }
            //}
            //else
            //{
            //    nextyIndex = ~yindex;
            //    prevyIndex = nextyIndex - 1;
            //}
            //var result = new List<IPoint> { 
            //    new Point2D(xtimes[prevxIndex], ytimes[prevyIndex], values[prevxIndex, prevyIndex]), 
            //    new Point2D(xtimes[prevxIndex], ytimes[nextyIndex], values[prevxIndex, nextyIndex]),
            //    new Point2D(xtimes[nextxIndex], ytimes[prevyIndex], values[nextxIndex, prevyIndex]),
            //    new Point2D(xtimes[nextxIndex], ytimes[nextyIndex], values[nextxIndex, nextyIndex]),
            //};
            var result = new List<IPoint> { 
                new Point2D(xtimes[xIndexes[0]], ytimes[yIndexes[0]], values[xIndexes[0], yIndexes[0]]), 
                new Point2D(xtimes[xIndexes[0]], ytimes[yIndexes[1]], values[xIndexes[0], yIndexes[1]]),
                new Point2D(xtimes[xIndexes[1]], ytimes[yIndexes[0]], values[xIndexes[1], yIndexes[0]]),
                new Point2D(xtimes[xIndexes[1]], ytimes[yIndexes[1]], values[xIndexes[1], yIndexes[1]]),
            };
            return result;
        }

        private static int[] CalculateIndexes(double searchCoordinate, double[] items)
        {
            int index = Array.BinarySearch(items, searchCoordinate);
            int[] indexes = new int[2];
            if (index >= 0)
            {
                // Exact match
                indexes[0] = index;
                indexes[1] = index;
            }
            else if (~index > items.GetUpperBound(0))
            {
                // Past the top boundary, set to the last point
                indexes[0] = items.GetUpperBound(0);
                indexes[1] = items.GetUpperBound(0);
            }
            else if (~index == 0)
            {
                // Below the bottom boundary, set to the first point
                indexes[0] = 0;
                indexes[1] = 0;
            }
            else
            {
                // Within the boundaries
                indexes[0] = ~index - 1;
                indexes[1] = ~index;
            }
            return indexes;
        }

        /// <summary>
        /// Converts to a matrix for mathematical manipulation.
        /// </summary>
        /// <returns></returns>
        protected void Map(IList<IPoint> points)//Points must be sorted by dimension1 first and then dimension2.
        {
            var xArray = new List<double>();
            var yArray = new List<double>();
            foreach (var element in points)
            {
                if (element.GetNumDimensions()!=2) throw new Exception("The dimension is incorrect.");
                var xval = (double) element.Coords[0];
                if (!xArray.Contains(xval))
                {
                    xArray.Add(xval);
                }
                var yval = (double)element.Coords[1];
                if (!yArray.Contains(yval))
                {
                    yArray.Add(yval);
                }
            }
            xArray.Sort();
            yArray.Sort();
            XArray = xArray.ToArray();
            YArray = yArray.ToArray();
            _vMatrix = new Matrix(XArray.Length, YArray.Length);
            for (var i = 0; i < XArray.Length; i++)
            {
                for (var j = 0; j < YArray.Length; j++)
                {
                
                    _vMatrix[i,j] = points[i * YArray.Length + j].FunctionValue;
                }
            }
        }
    }
}
