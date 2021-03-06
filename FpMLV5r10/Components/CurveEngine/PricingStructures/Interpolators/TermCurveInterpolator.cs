#region Using Directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Util.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    /// <summary>
    /// The key rate curve interpolator.
    /// </summary>
    public class TermCurveInterpolator : InterpolatedCurve
    {
        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        public TermCurveInterpolator(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
            :base(Converter(termCurve, baseDate, dayCounter), InterpolationHelper(termCurve), IsExtrapolationPermitted(termCurve))
        {
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