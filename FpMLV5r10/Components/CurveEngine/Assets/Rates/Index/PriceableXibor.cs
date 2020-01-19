#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Xibor Rates
    /// </summary>
    public class PriceableXibor : PriceableRateIndex
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
        public PriceableXibor(DateTime baseDate, Decimal amount, XiborNodeStruct nodeStruct,
                              IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
                              BasicQuotation fixedRate)
            : base(baseDate, amount, nodeStruct.RateIndex, nodeStruct.BusinessDayAdjustments, nodeStruct.SpotDate, fixedRate)
        {
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.RateIndex.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(nodeStruct.RateIndex.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="riskMaturityDate">The risk maturity date.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        public PriceableXibor(string id, Decimal amount, DateTime effectiveDate, DateTime riskMaturityDate, DayCountFraction dayCountFraction)
            : base(id, amount, effectiveDate, riskMaturityDate)
        {
            YearFraction = GetYearFraction(dayCountFraction.Value, effectiveDate, riskMaturityDate);
        }
    }
}