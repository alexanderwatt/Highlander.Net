#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.DayCounters;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public abstract class PriceableCash : PriceableSimpleRateAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public Period Term { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCash"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="cash">The deposit.</param>
        /// <param name="amount">The cah amount.</param>
        /// <param name="term">The term of the cash flow.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        protected PriceableCash(DateTime baseDate, Asset cash, Decimal amount, Period term, IBusinessCalendar paymentCalendar,
                                BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
            : base(cash.id, baseDate, amount, businessDayAdjustments, fixedRate)
        {
            Term = term;      
            RiskMaturityDate =
                paymentCalendar.Advance(BaseDate, OffsetHelper.FromInterval(Term, DayTypeEnum.Calendar),
                                  BusinessDayAdjustments.businessDayConvention);
            YearFraction = (decimal)Actual365.Instance.YearFraction(BaseDate, RiskMaturityDate);
        }
    }
}