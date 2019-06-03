#region Using directives

using System;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleFra : PriceableSimpleRateAsset
    {
        /// <summary>
        /// FixingDateOffset
        /// </summary>
        public RelativeDateOffset FixingDateOffset { get; set; }

        /// <summary>
        /// SimpleFra
        /// </summary>
        public SimpleFra SimpleFra { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SpotDate { get; set; }

        ///<summary>
        ///</summary>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => CalculationResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleFra"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.></param>
        /// <param name="notional">The notional. The default value is 1.00m.</param>
        /// <param name="normalisedRate">Thhhe fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableSimpleFra(DateTime baseDate, SimpleFraNodeStruct nodeStruct, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicQuotation normalisedRate)
            : base(nodeStruct.SimpleFra.id, baseDate, notional, nodeStruct.BusinessDayAdjustments, normalisedRate)
        {
            SimpleFra = nodeStruct.SimpleFra;
            FixingDateOffset = nodeStruct.SpotDate;
            UnderlyingRateIndex = nodeStruct.RateIndex;
            SpotDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            AdjustedStartDate = GetEffectiveDate(SpotDate, paymentCalendar, nodeStruct.SimpleFra.startTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);//GetSpotDate();
            RiskMaturityDate = GetEffectiveDate(SpotDate, paymentCalendar, nodeStruct.SimpleFra.endTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(SimpleFra.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
            TimeToExpiry = GetTimeToMaturity(baseDate, RiskMaturityDate);
        }

        /// <summary>
        /// Returns the time to expiry.
        /// </summary>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return  RiskMaturityDate;
        }

        /// <summary>
        /// Gets the year fraction to maturity.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToMaturity(DateTime baseDate, DateTime maturityDate)
        {
            return (decimal)DayCounterHelper.ToDayCounter(DayCountFractionEnum.ACT_365_FIXED).YearFraction(baseDate, maturityDate);
        }
    }
}