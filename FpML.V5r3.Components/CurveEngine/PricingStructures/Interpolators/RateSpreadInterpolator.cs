#region Using Directives

using System;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using FpML.V5r3.Reporting;

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