#region Using Directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.CurveEngine.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    ///<summary>
    ///</summary>
    public class InflationCurveInterpolator : InterpolatedCurve
    {
        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="dayCounter"></param>
        public InflationCurveInterpolator(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
            : base(CurveHelper.Converter(termCurve, baseDate, dayCounter), InterpolationFactory.Create(termCurve), CurveHelper.IsExtrapolationPermitted(termCurve))
        {
        }
    }
}