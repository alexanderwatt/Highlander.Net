/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

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