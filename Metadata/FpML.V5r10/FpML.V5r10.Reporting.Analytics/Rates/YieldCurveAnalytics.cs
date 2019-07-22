/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Rates
{
    ///<summary>
    ///</summary>
    public static class YieldCurveAnalytics //TODO convert this to an analytical model.
    {
        //  need a based date? 
        //
        ///<summary>
        ///</summary>
        ///<param name="discountCurve"></param>
        ///<param name="baseDate"></param>
        ///<param name="frequency"></param>
        ///<param name="dayCounter"></param>
        ///<returns></returns>
        ///<exception cref="System.Exception"></exception>
        public static TermCurve ToZeroCurve(TermCurve discountCurve, DateTime baseDate, 
            CompoundingFrequencyEnum frequency, IDayCounter dayCounter)
        {
            TermCurve result = TermCurve.Create(new List<TermPoint>());
            foreach (TermPoint point in discountCurve.point)
            {
                DateTime pointDate = XsdClassesFieldResolver.TimeDimensionGetDate(point.term);
                double zeroRateDouble;
                if (baseDate != pointDate)
                {
                    double time = dayCounter.YearFraction(baseDate, pointDate);
                    zeroRateDouble = RateAnalytics.DiscountFactorToZeroRate((double)point.mid, time, frequency);
                }
                else
                {
                    // set after the loop
                    zeroRateDouble = 0;
                }
                TermPoint zeroPoint = TermPointFactory.Create(Convert.ToDecimal(zeroRateDouble), pointDate);
                zeroPoint.id = point.id;
                result.Add(zeroPoint);
            }
            if (result.point[0].mid == 0)
            {
                result.point[0].mid = result.point[1].mid;
            }
            return result;
        }
    }
}