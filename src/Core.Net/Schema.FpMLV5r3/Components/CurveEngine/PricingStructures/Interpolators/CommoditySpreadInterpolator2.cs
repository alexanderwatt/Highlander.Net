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
using System.Collections.Generic;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Interpolators
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