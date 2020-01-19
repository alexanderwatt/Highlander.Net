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


#region UsingDirectives

using System;
using System.Collections;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public interface IPoint : IComparable
    {
        /// <summary>
        /// This returns the dimension value for a point.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// A function is applied to a collection of points.
        /// </summary>
        /// <returns><c>double[]</c> The point coordinate values.</returns>       
        /// <remarks>The interface method <c>point.GetDimensionValues()</c>
        /// can handle a multi-dimensional point, but with a minimum of one dimension.</remarks>
        IList Coords{get;}

        /// <summary>
        /// Gets or sets the function value.
        /// </summary>
        /// <value>The function value.</value>
        double FunctionValue { get; set;}

        /// <summary>
        /// Gets the x co-ordinate of the 1D point.
        /// </summary>
        /// <returns></returns>
        double GetX();

        /// <summary>
        /// this returns the number of dimensions of the point.
        /// <seealso cref="IPoint"/>the interface <c>iPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> the number of <c>int</c> dimensions.</returns>
        int GetNumDimensions();
    }
}
