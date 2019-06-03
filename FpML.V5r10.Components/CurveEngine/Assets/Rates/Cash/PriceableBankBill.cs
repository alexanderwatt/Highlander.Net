#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public class PriceableBankBill : PriceableSimpleRateAsset
    {
                ///<summary>
        ///</summary>
        public RelativeDateOffset SpotDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public Deposit BankBill { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBankBill"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">Notional Amount.</param>
        /// <param name="nodeStruct">The deposit nodeStruct.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableBankBill(DateTime baseDate, Decimal amount, BankBillNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, 
                                IBusinessCalendar paymentCalendar, BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
            : base(nodeStruct.Deposit.id, baseDate, amount, businessDayAdjustments, fixedRate)
        {
            SpotDateOffset = nodeStruct.SpotDate;
            BankBill = nodeStruct.Deposit;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.Deposit.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(BankBill.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }
    }
}