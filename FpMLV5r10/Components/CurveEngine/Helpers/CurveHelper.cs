#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// Creates an interpolation class.
    /// </summary>
    public class CurveHelper //TODO convert this to an analytical model.
    {
        //public static NamedValueSet CombinePropertySets(NamedValueSet baseProperties, NamedValueSet additionalProperties)
        //{
        //    var properties = additionalProperties.ToDictionary();
        //    foreach (var nvs in properties.Keys)
        //    {
        //        var value = properties[nvs];
        //        baseProperties.Set(nvs, value);
        //    }
        //    return baseProperties;
        //}

        public static NamedValueSet CombinePropertySetsClone(NamedValueSet baseProperties, NamedValueSet additionalProperties)
        {
            var properties = additionalProperties.ToDictionary();
            var cloneProperties = baseProperties.Clone();
            foreach (var nvs in properties.Keys)
            {
                var value = properties[nvs];
                cloneProperties.Set(nvs, value);
            }
            return cloneProperties;
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
                //TODO extend to check if term or dates.
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
        public static bool IsExtrapolationPermitted(TermCurve termCurve)
        {
            var isExtrapolationPermitted = termCurve.extrapolationPermittedSpecified && termCurve.extrapolationPermitted;
            return isExtrapolationPermitted;
        }
    }
}