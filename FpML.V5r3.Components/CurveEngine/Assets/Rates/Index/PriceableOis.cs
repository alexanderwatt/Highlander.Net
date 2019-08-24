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
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.RateIndex.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(nodeStruct.RateIndex.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }
    }
}