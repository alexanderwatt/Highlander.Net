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

using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolators
{
    /// <summary>
    /// This class will not implement any of IInterpolation but needs it's signature
    /// It is the abstract base for n-dimensional linear interpolators
    /// </summary>
    public abstract class Interpolator : IInterpolation
    {
        /// <summary>
        /// A method to interpolate boundary points to generate a value for an enclosed point
        /// </summary>
        /// <param name="point">The point to value</param>
        /// <param name="bounds">The bounding points to interpolate on</param>
        /// <returns></returns>
        public abstract double Value(IPoint point, List<IPoint> bounds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public abstract double ValueAt(double point, bool bounds);

        #region IInterpolation Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void Initialize(double[] x, double[] y);

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public abstract IInterpolation Clone();

        #endregion
    }
}
