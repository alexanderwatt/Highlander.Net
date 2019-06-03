/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public abstract class InterpolatedSpace: IInterpolatedSpace
    {
        /// <summary>
        /// The discrete space.
        /// </summary>
        private readonly IDiscreteSpace _discreteSpace;

        /// <summary>
        /// The interpolation type for the x axis.
        /// </summary>
        protected readonly IInterpolation Interpolation;

        /// <summary>
        /// The discrete space.
        /// </summary>
        private readonly bool _allowExtrapolation;

        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="discreteSpace">The discrete space upon which to apply the interpolating function.</param>
        /// <param name="interpolation">Interpolation type for the x axis.</param>
        /// <param name="allowExtrapolation">Boolean flag, currently not implemented.</param>
        protected InterpolatedSpace(IDiscreteSpace discreteSpace, IInterpolation interpolation, bool allowExtrapolation)
        {
            _discreteSpace = discreteSpace;
            Interpolation = interpolation;
            _allowExtrapolation = allowExtrapolation;
        }

        /// <summary>
        /// This returns the point function used to add continuity to the discrete space.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> The number of <c>int</c> points.</returns>
        public IInterpolation GetInterpolatingFunction()
        {
            return Interpolation;
        }

        /// <summary>
        /// Gets the underlying discrete space that is used for interpolation.
        /// </summary>
        /// <returns></returns>
        public IDiscreteSpace GetDiscreteSpace()
        {
            return _discreteSpace;
        }

        /// <summary>
        /// Is extrapolation allowed?
        /// </summary>
        /// <returns></returns>
        public bool AllowExtrapolation()
        {
            return _allowExtrapolation;
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public abstract double Value(IPoint point);

        /// <summary>
        /// Not yet implemented
        /// </summary>
        /// <returns></returns>
        public virtual Market GetMarket()
        {
            throw new NotImplementedException();
        }
    }
}
