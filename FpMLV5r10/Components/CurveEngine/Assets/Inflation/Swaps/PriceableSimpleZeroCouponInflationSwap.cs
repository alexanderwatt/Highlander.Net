using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.DayCounters;

namespace Orion.CurveEngine.Assets.Inflation.Swaps
{
    /// <summary>
    /// Base class for inflation indexes.
    /// </summary>
    public class PriceableSimpleZeroCouponInflationSwap : PriceableSimpleInflationAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset SpotDateOffset { get; }
        /// <summary>
        /// 
        /// </summary>
        public Calculation Calculation { get; }

        /// <summary>
        /// 
        /// </summary>
        public SimpleIRSwap SimpleInflationSwap { get; }

        /// <summary>
        /// Gets the index of the underlying rate.
        /// </summary>
        /// <value>The index of the underlying rate.</value>
        public RateIndex UnderlyingRateIndex { get; }

        ///<summary>
        ///</summary>
        public IDayCounter DayCounter { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => AnalyticResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleInflationAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleZeroCouponInflationSwap(DateTime baseDate, SimpleIRSwapNodeStruct nodeStruct,
                                                      IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate,
                   XsdClassesFieldResolver.CalculationGetNotionalSchedule(nodeStruct.Calculation).notionalStepSchedule.initialValue, nodeStruct.DateAdjustments, fixedRate)
        {
            Id = nodeStruct.SimpleIRSwap.id;
            SimpleInflationSwap = nodeStruct.SimpleIRSwap;
            SpotDateOffset = nodeStruct.SpotDate;
            Calculation = nodeStruct.Calculation;
            UnderlyingRateIndex = nodeStruct.UnderlyingRateIndex;
            DayCounter = DayCounterHelper.Parse(Calculation.dayCountFraction.Value);
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, SpotDateOffset);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, SimpleInflationSwap.term, nodeStruct.DateAdjustments.businessDayConvention);
            YearFraction = GetYearFractions()[0];
        }

        /// <summary>
        /// Gets the forward spot date.
        /// </summary>
        /// <returns></returns>
        protected DateTime GetSpotDate()
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

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public sealed override decimal[] GetYearFractions()//TODO 1/1 daycount needs implementing. Temporarily use number of years.
        {
            return new[] { Convert.ToDecimal(SimpleInflationSwap.term.periodMultiplier) };
        }       
    }
}