#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.Analytics.DayCounters;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets.Inflation.Index
{
    /// <summary>
    /// Xibor Rates
    /// </summary>
    public class PriceableInflationXibor : PriceableInflationIndex
    {
         /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixedRate"></param>
        public PriceableInflationXibor(DateTime baseDate, Decimal amount, XiborNodeStruct nodeStruct,
                              IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
                              BasicQuotation fixedRate)
            : base(baseDate, amount, nodeStruct.RateIndex, nodeStruct.BusinessDayAdjustments, nodeStruct.SpotDate, fixedRate)
        {
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.RateIndex.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(nodeStruct.RateIndex.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public override decimal[] GetYearFractions()
        {
            return
                new[]
                    {
                        (decimal)
                        DayCounterHelper.Parse(UnderlyingRateIndex.dayCountFraction.Value).YearFraction(
                            AdjustedStartDate, GetRiskMaturityDate())
                    };
        }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        /// <returns></returns>
        protected override DateTime GetEffectiveDate()
        {
            return AdjustedStartDate;
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }
    }
}