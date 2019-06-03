#region Using Directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    /// <summary>
    /// The key rate curve interpolator.
    /// </summary>
    public class RateSpreadInterpolator : InterpolatedCurve
    {
        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="zeroSpreadCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        public RateSpreadInterpolator(IRateCurve referenceCurve, TermCurve zeroSpreadCurve, DateTime baseDate, IDayCounter dayCounter)
            : base(Converter(zeroSpreadCurve, baseDate, dayCounter), new RateBasisSpreadInterpolation(referenceCurve), IsExtrapolationPermitted(zeroSpreadCurve))
        {
        }

    }
}