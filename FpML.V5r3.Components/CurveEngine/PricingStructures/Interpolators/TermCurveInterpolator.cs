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
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    /// <summary>
    /// The key rate curve interpolator.
    /// </summary>
    public class TermCurveInterpolator : InterpolatedCurve
    {
        /// <summary>
        /// The term curve.
        /// </summary>
        public TermCurve TermCurve { get; protected set; }

        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        public TermCurveInterpolator(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
            :base(Converter(termCurve, baseDate, dayCounter), InterpolationHelper(termCurve), IsExtrapolationPermitted(termCurve))
        {
            TermCurve = termCurve;
        }

        private enum InterpolationType
        {
            ClampedCubicSplineInterpolation,
            CubicHermiteSplineInterpolation,
            CubicSplineInterpolation,
            FlatInterpolation,
            LinearInterpolation,
            LinearRateInterpolation,
            LogLinearInterpolation,
            LogRateClampedCubicSplineInterpolation,
            LogRateCubicSplineInterpolation,
            PiecewiseConstantInterpolation,
            PiecewiseConstantRateInterpolation,
            PiecewiseConstantZeroRateInterpolation,
            SabrModelInterpolation,
            WingModelInterpolation
        }

        /// <summary>
        /// Used to create the interpolator.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <returns></returns>
        public static IInterpolation InterpolationHelper(TermCurve termCurve)
        {
            InterpolationType interpolation = termCurve.interpolationMethod == null 
                ? InterpolationType.LogLinearInterpolation
                : EnumHelper.Parse<InterpolationType>(termCurve.interpolationMethod.Value, true);
            switch (interpolation)
            {
                case InterpolationType.ClampedCubicSplineInterpolation:
                    return new CubicSplineInterpolation();
                case InterpolationType.CubicHermiteSplineInterpolation:
                    return new CubicHermiteSplineInterpolation();
                case InterpolationType.CubicSplineInterpolation:
                    return new CubicSplineInterpolation();
                case InterpolationType.FlatInterpolation:
                    return new FlatInterpolation();
                case InterpolationType.LinearInterpolation:
                    return new LinearInterpolation();
                case InterpolationType.LinearRateInterpolation:
                    return new LinearRateInterpolation();
                case InterpolationType.LogLinearInterpolation:
                    return new LogLinearInterpolation();
                case InterpolationType.LogRateClampedCubicSplineInterpolation:
                    return new LogRateCubicSplineInterpolation();
                case InterpolationType.LogRateCubicSplineInterpolation:
                    return new LogRateCubicSplineInterpolation();
                case InterpolationType.PiecewiseConstantInterpolation:
                    return new PiecewiseConstantInterpolation();
                case InterpolationType.PiecewiseConstantRateInterpolation:
                    return new PiecewiseConstantRateInterpolation();
                case InterpolationType.PiecewiseConstantZeroRateInterpolation:
                    return new PiecewiseConstantZeroRateInterpolation();
                case InterpolationType.SabrModelInterpolation:
                    return new SABRModelInterpolation();
                case InterpolationType.WingModelInterpolation:
                    return new WingModelInterpolation();
                default:
                    throw new ArgumentException($"Unknown Interpolation Method '{termCurve.interpolationMethod?.Value}'");
            }
        }
    }
}