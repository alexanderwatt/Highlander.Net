#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Interpolations;
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
                var yearFractionsbeginPeriod = yearFractions[index];
                var endOfPeriodDateTime = forwardRateTenor.Add(startOfPeriodDateTime);
                var yearFractionAtEndOfPeriod = (endOfPeriodDateTime - baseDate).TotalDays / 365.0;
                //get df corresponding to end of period
                //
                IInterpolation interpolation = new LinearRateInterpolation();
                interpolation.Initialize(yearFractions.ToArray(), discountFactors.ToArray());
                var dfAtEndOfPeriod = interpolation.ValueAt(yearFractionAtEndOfPeriod, true);
                var dfAtTheBeginingOfPeriod = discountFactors[index];
                var forwardRate = (dfAtTheBeginingOfPeriod / dfAtEndOfPeriod - 1) /
                                  (yearFractionAtEndOfPeriod - yearFractionsbeginPeriod);
                var zeroPoint = TermPointFactory.Create(Convert.ToDecimal(forwardRate), startOfPeriodDateTime);
                forwardTermCurve.Add(zeroPoint);
                ++index;
            }
            return forwardTermCurve;
        }
    }
}