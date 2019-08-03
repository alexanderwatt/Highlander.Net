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

#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.Assets;

#endregion

namespace Orion.CurveEngine.PricingStructures.Helpers
{
    ///<summary>
    ///</summary>
    public class YieldCurveConverter //TODO convert this to an analytical model.
    {
        ///<summary>
        ///</summary>
        ///<param name="discountCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="forwardRateTenor"></param>
        ///<param name="priceableXibor"></param>
        ///<returns></returns>
        public TermCurve ToPriceableXiborForwardCurve(TermCurve discountCurve, DateTime baseDate,
                                                      Period forwardRateTenor, PriceableXibor priceableXibor) //TODO
        {
            //CompoundingFrequency frequency = CompoundingFrequencyHelper.Create("Annual");
            var dates = discountCurve.GetListTermDates();
            var yearFractions = new List<double>();
            foreach (var date in dates)
            {
                var yearFraction = (date - baseDate).TotalDays / 365.0;//TODO extend this with a general daycountraction.                   
                yearFractions.Add(yearFraction);
            }
            var midValues = discountCurve.GetListMidValues();
            var discountFactors = new List<double>(Array.ConvertAll(midValues.ToArray(), Convert.ToDouble));
            var forwardTermCurve = TermCurve.Create(new List<TermPoint>());
            var index = 0;
            foreach (var startOfPeriodDateTime in dates)
            {
                var yearFractionsBeginPeriod = yearFractions[index];
                var endOfPeriodDateTime = forwardRateTenor.Add(startOfPeriodDateTime);
                var yearFractionAtEndOfPeriod = (endOfPeriodDateTime - baseDate).TotalDays / 365.0;
                //get df corresponding to end of period
                //
                IInterpolation interpolation = new LinearRateInterpolation();
                interpolation.Initialize(yearFractions.ToArray(), discountFactors.ToArray());
                var dfAtEndOfPeriod = interpolation.ValueAt(yearFractionAtEndOfPeriod, true);
                var dfAtTheBeginningOfPeriod = discountFactors[index];
                var forwardRate = (dfAtTheBeginningOfPeriod / dfAtEndOfPeriod - 1) /
                                  (yearFractionAtEndOfPeriod - yearFractionsBeginPeriod);
                var zeroPoint = TermPointFactory.Create(Convert.ToDecimal(forwardRate), startOfPeriodDateTime);
                forwardTermCurve.Add(zeroPoint);
                ++index;
            }
            return forwardTermCurve;
        }
    }
}