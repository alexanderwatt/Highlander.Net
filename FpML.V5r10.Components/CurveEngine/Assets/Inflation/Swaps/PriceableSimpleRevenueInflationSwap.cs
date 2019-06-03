using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Schedulers;
// Orion API Asset extensions
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;


namespace Orion.CurveEngine.Assets.Inflation.Swaps
{
    /// <summary>
    /// Base class for inflation indexes.
    /// </summary>
    public class PriceableSimpleRevenueInflationSwap : PriceableInflationSwapAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset SpotDateOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Calculation Calculation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDayCounter DayCounter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SimpleIRSwap SimpleInflationSwap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => AnalyticResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleFra"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableSimpleRevenueInflationSwap(DateTime baseDate, SimpleIRSwapNodeStruct nodeStruct,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct.DateAdjustments, fixedRate)
        {
            Id = nodeStruct.SimpleIRSwap.id;
            SimpleInflationSwap = nodeStruct.SimpleIRSwap;
            SpotDateOffset = nodeStruct.SpotDate;
            Calculation = nodeStruct.Calculation;
            UnderlyingRateIndex = nodeStruct.UnderlyingRateIndex;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, SpotDateOffset);
            DayCounter = DayCounterHelper.Parse(Calculation.dayCountFraction.Value);
            List<DateTime> unadjustedDateSchedule = DateScheduler.GetUnadjustedDateSchedule(AdjustedStartDate, SimpleInflationSwap.term, SimpleInflationSwap.paymentFrequency);
            AdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDateSchedule, nodeStruct.DateAdjustments.businessDayConvention, paymentCalendar);
        }

        /// <summary>
        /// Gets the forward spot date.
        /// </summary>
        /// <returns></returns>
        protected DateTime GetSpotDate()
        {
            return AdjustedPeriodDates[0];
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return AdjustedPeriodDates[AdjustedPeriodDates.Count - 1];
        }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public override decimal[] GetYearFractions()
        {
            return GetYearFractionsForDates(AdjustedPeriodDates, SimpleInflationSwap.dayCountFraction);
        }

        /// <summary>
        /// Gets the year fractions for dates.
        /// </summary>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        /// <returns></returns>
        private static decimal[] GetYearFractionsForDates(IList<DateTime> periodDates, DayCountFraction dayCountFraction)
        {
            var yearFractions = new List<decimal>();
            int index = 0;
            int periodDatesLastIndex = periodDates.Count - 1;
            foreach (DateTime periodDate in periodDates)
            {
                if (index == periodDatesLastIndex)
                    break;
                var yearFraction = (decimal)DayCounterHelper.Parse(dayCountFraction.Value).YearFraction(periodDate, periodDates[index + 1]);
                yearFractions.Add(yearFraction);
                index++;
            }
            return yearFractions.ToArray();
        }        
    }
}