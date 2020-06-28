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
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Interpolators
{
    ///<summary>
    ///</summary>
    public class FxCurveInterpolator : InterpolatedCurve
    {
        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="dayCounter"></param>
        public FxCurveInterpolator(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter)
            : base(CurveHelper.Converter(termCurve, baseDate, dayCounter), InterpolationFactory.Create(termCurve), CurveHelper.IsExtrapolationPermitted(termCurve))
        {
        }
    }
}