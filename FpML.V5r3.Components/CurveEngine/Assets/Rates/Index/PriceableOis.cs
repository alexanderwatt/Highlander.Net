#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Overnight Index Swap Rate
    /// </summary>
    public class PriceableOis : PriceableRateIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixedRate">The fixedRate.</param>
        public PriceableOis(DateTime baseDate, Decimal amount, XiborNodeStruct nodeStruct,
                              IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, amount, nodeStruct.RateIndex, nodeStruct.BusinessDayAdjustments, nodeStruct.SpotDate, fixedRate)
        {
            //FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(ResetDateConvention.businessCenters);
            //RollCalendar = BusinessCenterHelper.ToBusinessCalendar(BusinessDayAdjustments.businessCenters);
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.RateIndex.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(nodeStruct.RateIndex.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="amount">The amount.</param>
        ///// <param name="resetDateConvention">The reset date convention.</param>
        ///// <param name="rateIndex">Index of the rate.</param>
        ///// <param name="businessDayAdjustments">The business day adjustments.</param>
        ///// <param name="fixedRate">The fixed rate.</param>
        //public PriceableOis(DateTime baseDate, Decimal amount, RelativeDateOffset resetDateConvention,
        //                    RateIndex rateIndex, BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
        //    : base(baseDate, amount, rateIndex, businessDayAdjustments, resetDateConvention, fixedRate)
        //{
        //    FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(ResetDateConvention.businessCenters);
        //    RollCalendar = BusinessCenterHelper.ToBusinessCalendar(BusinessDayAdjustments.businessCenters);
        //    AdjustedStartDate = GetEffectiveDate();
        //    RiskMaturityDate = RollCalendar.Advance(
        //            AdjustedStartDate,
        //            OffsetHelper.FromInterval(UnderlyingRateIndex.term, DayTypeEnum.Calendar),
        //            BusinessDayAdjustments.businessDayConvention);
        //    YearFraction = GetYearFraction(UnderlyingRateIndex.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        //}

        ///// <summary>
        ///// Gets the start date.
        ///// </summary>
        ///// <returns></returns>
        //protected override DateTime GetEffectiveDate()
        //{
        //    return FixingCalendar.Advance(BaseDate, ResetDateConvention, ResetDateConvention.businessDayConvention);
        //}

        ///// <summary>
        ///// Gets the year fraction.
        ///// </summary>
        ///// <returns></returns>
        //public override decimal GetYearFraction()
        //{
        //    return
        //        (decimal)
        //                DayCounterHelper.Parse(UnderlyingRateIndex.dayCountFraction.Value).YearFraction(
        //                    AdjustedStartDate, GetRiskMaturityDate());
        //}
    }
}