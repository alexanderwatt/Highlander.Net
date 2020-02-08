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
using Highlander.Reporting.ModelFramework.V5r3;

#endregion


namespace Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public abstract class DiscreteSpace: IDiscreteSpace
    {
        /// <summary>
        /// The list of points.
        /// </summary>
        protected List<IPoint> Points = new List<IPoint>();

        /// <summary>
        /// The number of dimensions.
        /// </summary>
        private readonly int _numDimensions;

        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="points"></param>
        protected DiscreteSpace(IList<IPoint> points)
        {
            _numDimensions = points[0].Coords.Count - 1;
            CleanAndSortPoints(points);
        }

        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="numDimensions"></param>
        /// <param name="points"></param>
        protected DiscreteSpace(int numDimensions, IEnumerable<IPoint> points)
        {
            _numDimensions = numDimensions;
            CleanAndSortPoints(points);
        }

        //TODO add sorting and enumeration.

        /// <summary>
        /// Adds points of the correct dimension to the sparse space.
        /// </summary>
        /// <param name="points"></param>
        private void CleanAndSortPoints(IEnumerable<IPoint> points)
        {
            foreach (var point in points)
            {
                if (point.Coords.Count-1 != _numDimensions)
                {
                    Points.Remove(point);
                }
                else
                {
                    Points.Add(point);
                }
            }
            Points.Sort();
        }

        /// <summary>
        /// This returns the number of dimensions of the point.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> dimensions.</returns>
        public int GetNumDimensions()
        {
            return _numDimensions;
        }

        /// <summary>
        /// This returns the number of points.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> points.</returns>
        public int GetNumPoints()
        {
            return Points.Count;

        }

        /// <summary>
        /// The coordinate array data used for interpolation.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public abstract double[] GetCoordinateArray(int dimension);

        /// <summary>
        /// The coordinate array data used for interpolation.
        /// </summary>
        /// <returns></returns>
        public double[] GetFunctionValueArray()
        {
            var values = new double[Points.Count];
            for (var i = 0; i < Points.Count; i++)
            {
                values[i] = Points[i].FunctionValue;
            }
            return values;
        }

        /// <summary>
        /// The set of points in the discrete space.
        /// </summary>
        /// <returns><c>IPoint</c> The list  of the specified <c>IPoint</c> values.</returns>
        public List<IPoint> GetPointList()
        {
            return Points.ToList();
        }

        /// <summary>
        /// Sets the points in the discrete space.
        /// </summary>
        public void SetPointList( List<IPoint> points)
        {
            Points = points;
        }

        /// <summary>
        /// Converts to a matrix for mathematical manipulation.
        /// </summary>
        /// <returns></returns>
        protected void SetFunctionValues(double[] points)
        {
            if (points.Length != Points.Count)
                throw new ArgumentException(
                    "TODO: unequal number of elements for applying the helper");
            for (var i = 0; i < points.Length; i++)
            {
                Points[i].FunctionValue = points[i];
            }
        }

        /// <summary>
        /// Used in interpolations.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract List<IPoint> GetClosestValues(IPoint pt);
    }
}
