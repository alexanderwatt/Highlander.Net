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
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.Analytics.Rates
{
    ///<summary>
    ///</summary>
    public static class YieldCurveAnalytics
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

        /// <summary>
        /// </summary>
        /// <param name="discountCurve"></param>
        /// <param name="tenor"></param>
        /// <param name="baseDate"></param>
        /// <param name="interpolatedCurve"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="dayCounter"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static TermCurve ToForwardCurve(TermCurve discountCurve, Period tenor, DateTime baseDate,
            InterpolatedCurve interpolatedCurve, IBusinessCalendar paymentCalendar, IDayCounter dayCounter)
        {
            TermCurve result = TermCurve.Create(new List<TermPoint>());
            var length = discountCurve.point.Length;
            var offset = new Offset
            {
                dayType = DayTypeEnum.Calendar,
                dayTypeSpecified = true,
                period = tenor.period,
                periodMultiplier = tenor.periodMultiplier,
                periodSpecified = true
            };
            if (paymentCalendar == null) return result;
            for (int i = 0; i < length - 1; i++) //This will only go to the penultimate point. Extrapolation required for more.
            {
                var pointStart = discountCurve.point[i];
                DateTime startDate = XsdClassesFieldResolver.TimeDimensionGetDate(pointStart.term);
                var endDate = paymentCalendar.Advance(startDate, offset, BusinessDayConventionEnum.FOLLOWING);
                var endPoint = new DateTimePoint1D(baseDate, endDate);
                var endDF = interpolatedCurve.Value(endPoint);
                double time = dayCounter.YearFraction(startDate, endDate);
                var forwardRateDouble =
                    RateAnalytics.DiscountFactorsToForwardRate((double) pointStart.mid, endDF, time);
                TermPoint forwardPoint = TermPointFactory.Create(Convert.ToDecimal(forwardRateDouble), startDate);
                forwardPoint.id = tenor.id;
                result.Add(forwardPoint);
            }
            return result;
        }
    }
}