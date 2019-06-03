#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.Analytics.Rates
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