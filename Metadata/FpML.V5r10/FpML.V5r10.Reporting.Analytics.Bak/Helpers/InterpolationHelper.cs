#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;

#endregion

namespace Orion.Analytics.Helpers
{
    /// <summary>
    /// Creates an interpolation class.
    /// </summary>
    public class InterpolationHelper //TODO convert this to an analytical model.
    {
        /// <summary>
        /// Used to create the interpolation.
        /// </summary>
        /// <param name="interpolationMethod"></param>
        /// <returns></returns>
        public static IInterpolation Create(InterpolationMethod interpolationMethod)
        {
            var interpolationName = "LogLinearInterpolation";
            if (null != interpolationMethod)
            {
                interpolationName = interpolationMethod.Value;
            }
            //WTF??? - hardcoded string literal for the assebmly name ????
            //
            return ClassFactory<IInterpolation>.Create("Analytics", interpolationName);
        }

        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="dayCounter"></param>
        ///<returns></returns>
        public static DiscreteCurve Converter(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
        {
            var curve = new DiscreteCurve(PointCurveHelper(termCurve, baseDate, dayCounter));
            return curve;
        }

        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="dayCounter"></param>
        ///<returns></returns>
        public static List<IPoint> PointCurveHelper(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
        {
            var points = new List<IPoint>();
            foreach (var termPoint in termCurve.point)
            {
                var dayCount = dayCounter.YearFraction(baseDate, (DateTime)termPoint.term.Items[0]);
                //TODO extend to chask if term or dates.
                //
                var value = (double)termPoint.mid;
                IPoint point = new Point1D(dayCount, value); //TODO check terms and dates.
                points.Add(point);
            }
            return points;
        }

        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<returns></returns>
        public static IInterpolation Parse(TermCurve termCurve)
        {
            string interpolationName = "LinearInterpolation";
            if (null != termCurve.interpolationMethod)
            {
                interpolationName = termCurve.interpolationMethod.Value;
            }
            //WTF??? - hardcoded string literal for the assebmly name ????
            //
            return ClassFactory<IInterpolation>.Create("Analytics", interpolationName);
        }

        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<returns></returns>
        public static bool IsExtrapolationPermitted(TermCurve termCurve)
        {
            var isExtrapolationPermitted = termCurve.extrapolationPermittedSpecified && termCurve.extrapolationPermitted;
            return isExtrapolationPermitted;
        }
    }
}