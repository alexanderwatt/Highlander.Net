#region Using Directives

using System;
using Orion.CurveEngine.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    ///<summary>
    ///</summary>
    public class CommodityCurveInterpolator : InterpolatedCurve
    {
        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="dayCounter"></param>
        public CommodityCurveInterpolator(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
            : base(CurveHelper.Converter(termCurve, baseDate, dayCounter), InterpolationFactory.Create(termCurve), CurveHelper.IsExtrapolationPermitted(termCurve))
        {
        }
    }
}