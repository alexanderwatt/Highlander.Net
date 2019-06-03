#region Using Directives

using System;
using System.Collections.Generic;
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
    public class CommoditySpreadInterpolator2 : InterpolatedCurve
    {
        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="xArray"></param>
        /// <param name="yArray"></param>
        /// <param name="extrapolate"></param>
        public CommoditySpreadInterpolator2(ICommodityCurve referenceCurve, IList<double> xArray, IList<double> yArray, bool extrapolate)
            : base(new DiscreteCurve(xArray, yArray), new CommodityBasisSpreadInterpolation2(referenceCurve), extrapolate)
        {
        }

        /// <summary>
        /// The other ctor.
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="zeroSpreadCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        public CommoditySpreadInterpolator2(ICommodityCurve referenceCurve, TermCurve zeroSpreadCurve, DateTime baseDate, IDayCounter dayCounter)
            : base(Converter(zeroSpreadCurve, baseDate, dayCounter), new CommodityBasisSpreadInterpolation(referenceCurve), IsExtrapolationPermitted(zeroSpreadCurve))
        {
        }
    }
}