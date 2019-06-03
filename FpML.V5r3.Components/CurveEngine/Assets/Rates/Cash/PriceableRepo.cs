#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableRepo : PriceableSimpleRateAsset
    {
        ///<summary>
        ///</summary>
        public RelativeDateOffset SpotDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public Deposit Deposit { get; set; }

        ///<summary>
        ///</summary>
        public Asset UnderlyingAsset { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRepo"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">Notional Amount.</param>
        /// <param name="nodeStruct">The deposit nodeStruct.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableRepo(DateTime baseDate, Decimal amount, RepoNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, 
                                IBusinessCalendar paymentCalendar, BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
            : base(nodeStruct.Deposit.id, baseDate, amount, businessDayAdjustments, fixedRate)
        {
            SpotDateOffset = nodeStruct.SpotDate;
            Deposit = nodeStruct.Deposit;
            UnderlyingAsset = nodeStruct.UnderlyingAsset;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.Deposit.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(Deposit.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }
    }
}