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

#region Using Direcctives

using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class Point3D : Point
    {
        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        /// <param name="y"><c>double</c> The 3d point value - y coordinate.</param>
        /// <param name="z"><c>double</c> The 3d point value - z coordinate.</param>
        public Point3D(double x, double y, double z)
            : base(new[] { x, y, z, 0.0 })
        {}

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        /// <param name="y"><c>double</c> The 3d point value - y coordinate.</param>
        /// <param name="z"><c>double</c> The 3d point value - z coordinate.</param>
        /// <param name="value"></param>
        public Point3D(double x, double y, double z, double value)
            : base(new[] { x, y, z, value })
        {}

        /// <summary>
        /// The ctor for the one dimensional point.
        /// </summary>
        /// <param name="x"><c>double</c> The 3d point value - x coordinate.</param>
        public Point3D(double[] x)
            : base(x)
        {}

        #region Point Methods

        /// <summary>
        /// Gets the y coordinate.
        /// </summary>
        /// <returns></returns>
        public double GetY()
        {
            return PointArray[1];
        }

        /// <summary>
        /// Gets the z coordinate
        /// </summary>
        /// <returns></returns>
        public double GetZ()
        {
            return PointArray[2];//TODO return 0 if index past end of array.
        }

        #endregion

    }
}
