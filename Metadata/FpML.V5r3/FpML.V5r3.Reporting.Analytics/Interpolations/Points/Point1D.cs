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

using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// In financial points we are interested mostly in curves and surface used for financial valuation.
    /// Unlike mathematical points, financial points at a minimum contain an x-coordinate and a function value.
    /// For ease of use, this is labeled as a one-dimensional point.
    /// </summary>
    public class Point1D : Point
    {
        /// <summary>
        /// The ctor for the two dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The one dimensional x value.</param>
        public Point1D(double x) : base(new[] {x, 0.0})
        {}

        /// <summary>
        /// The ctor for the two dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The one dimensional x value.</param>
        /// <param name="value"><c>double</c> The one dimensional function value.</param>
        public Point1D(double x, double value)
            : base(new[] { x, value })
        {}

        /// <summary>
        /// The ctor for the two dimensional point.
        /// </summary>
        /// <param name="array"><c>double</c> The array.</param>
        public Point1D(double[] array) : base(array)
        {}
    }
}
