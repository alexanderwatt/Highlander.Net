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

#region Using directives

using System;
using System.Linq;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Interpolators
{
    /// <summary>
    /// The vol surface interpolator
    /// </summary>
    public class ExtendedVolatilitySurfaceInterpolator : ExtendedInterpolatedSurface
    {
        /// <summary>
        /// Builds a raw volatility surface from basic data types.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forwards"></param>
        /// <param name="xInterpolation"></param>
        /// <param name="yInterpolation"></param>
        public ExtendedVolatilitySurfaceInterpolator(MultiDimensionalPricingData data, ParametricAdjustmentPoint[] forwards, string xInterpolation, string yInterpolation)
            : base(ProcessMultiDimensionalPricingData(data), forwards, InterpolationFactory.Create(xInterpolation), InterpolationFactory.Create(yInterpolation), true)
        {
        }

        /// <summary>
        /// Builds a raw volatility surface from basic data types.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="forwards"></param>
        /// <param name="volData"></param>
        /// <param name="xInterpolation"></param>
        /// <param name="yInterpolation"></param>
        public ExtendedVolatilitySurfaceInterpolator(double[] rows, double[] columns, double[] forwards, Matrix volData, string xInterpolation, string yInterpolation)
            : base(rows, columns, forwards, volData, InterpolationFactory.Create(xInterpolation), InterpolationFactory.Create(yInterpolation))
        {
        }

        internal static DiscreteSurface ProcessMultiDimensionalPricingData(MultiDimensionalPricingData data)
        {
            var points = (from point in data.point let baseDate = data.valuationDateSpecified ? (DateTime?) data.valuationDate : null select new PricingDataPoint2D(point, baseDate)).Cast<IPoint>().ToList();
            var result = new DiscreteSurface(points);//todo THERE REMAINS A PROBLEM WITH THIS CONSTRUCTOR.
            return result;
        }
 
    }
}