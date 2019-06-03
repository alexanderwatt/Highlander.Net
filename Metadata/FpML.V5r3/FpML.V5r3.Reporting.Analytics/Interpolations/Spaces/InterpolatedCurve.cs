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

#region

using System;
using System.Collections.Generic;
using Orion.Analytics.Interpolations.Points;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

//using Orion.Analytics.Interpolations;

#endregion

namespace Orion.Analytics.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class InterpolatedCurve : InterpolatedSpace
    {
        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="discreteCurve"></param>
        /// <param name="interpolation"></param>
        /// <param name="allowExtrapolation"></param>
        public InterpolatedCurve(IDiscreteSpace discreteCurve, IInterpolation interpolation, bool allowExtrapolation)
            : base(discreteCurve, interpolation, allowExtrapolation)
        {
            var xArray = GetDiscreteSpace().GetCoordinateArray(1);
            var yArray = GetDiscreteSpace().GetFunctionValueArray();
            Interpolation.Initialize(xArray, yArray);
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public override double Value(IPoint point)
        {
            return Interpolation.ValueAt((double) point.Coords[0], true);
        }

        /// <summary>
        /// Creates the discrete curve.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        /// <returns></returns>
        public static DiscreteCurve Converter(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
        {
            var curve = new DiscreteCurve(PointCurveHelper(termCurve, baseDate, dayCounter));
            return curve;
        }

        /// <summary>
        /// Maps the term points.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        /// <returns></returns>
        public static List<IPoint> PointCurveHelper(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
        {
            var points = new List<IPoint>();
            foreach (var termPoint in termCurve.point)
            {
                var dayCount = dayCounter.YearFraction(baseDate, (DateTime)termPoint.term.Items[0]);
                var value = (double)termPoint.mid;
                IPoint point = new Point1D(dayCount, value);
                points.Add(point);
            }
            return points;
        }

        /// <summary>
        /// Determines whether extrapolation is permitted.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <returns></returns>
        public static bool IsExtrapolationPermitted(TermCurve termCurve)
        {
            bool isExtrapolationPermitted = termCurve.extrapolationPermittedSpecified && termCurve.extrapolationPermitted;
            return isExtrapolationPermitted;
        }
    }
}
