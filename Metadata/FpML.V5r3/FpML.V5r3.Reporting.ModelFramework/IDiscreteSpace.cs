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

using System.Collections.Generic;

#endregion

namespace Orion.ModelFramework
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public interface IDiscreteSpace
    {
        /// <summary>
        /// This returns the number of dimensions of the point.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> dimensions.</returns>
        int GetNumDimensions();

        /// <summary>
        /// This returns the number of points.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> points.</returns>
        int GetNumPoints();

        /// <summary>
        /// The useable arrays for interpolation.
        /// </summary>
        /// <returns><c>double</c> The array values of the specified <c>int</c> dimension.</returns>
        double[] GetCoordinateArray(int dimension);

        /// <summary>
        /// The set of points in the discrete space.
        /// </summary>
        /// <returns><c>IPoint</c> The list  of the specified <c>IPoint</c> values.</returns>
        List<IPoint> GetPointList();

        /// <summary>
        /// The coordinate array data used for interpolation.
        /// </summary>
        /// <returns></returns>
        double[] GetFunctionValueArray();

        /// <summary>
        /// Sets the points in the discrete space.
        /// </summary>
        void SetPointList(List<IPoint> points);

        /// <summary>
        /// Used in interpolations.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        List<IPoint> GetClosestValues(IPoint pt);
    }
}
