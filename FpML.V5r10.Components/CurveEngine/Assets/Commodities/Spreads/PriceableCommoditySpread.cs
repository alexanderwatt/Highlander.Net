#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using DayCounterHelper=Orion.Analytics.DayCounters.DayCounterHelper;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableCommoditySpread : PriceableSimpleCommoditySpreadAsset
    {
        /// <summary>
        /// The time to expisry
        /// </summary>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// The spot date
        /// </summary>
        public DateTime SpotDate { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Period Tenor { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommoditySpread"/> class.
        /// </summary>
        /// <param name="identifier">The asset identifier</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="tenor">The tenor.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="spread">The spread.</param>
        /// <param name="fixingAndPaymentCalendar">A fixing and payment Calendar.</param>
        public PriceableCommoditySpread(String identifier, DateTime baseDate, string tenor, CommoditySpotNodeStruct nodeStruct,
            IBusinessCalendar fixingAndPaymentCalendar, BasicQuotation spread)
            : base(baseDate, spread)
        {
            Id = identifier;
            SpotDate = GetSpotDate(baseDate, fixingAndPaymentCalendar, nodeStruct.SpotDate);
            CommodityAsset = nodeStruct.Commodity;
            Tenor = PeriodHelper.Parse(tenor);
            RiskMaturityDate = GetEffectiveDate(SpotDate, fixingAndPaymentCalendar, Tenor, nodeStruct.SpotDate.businessDayConvention);
            TimeToExpiry = GetTimeToMaturity(baseDate, RiskMaturityDate);
            SetRate(spread);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCommoditySpread"/> class.
        /// </summary>
        /// <param name="baseDate">The base date</param>
        /// <param name="identifier">The asset identifier</param>
        /// <param name="maturityDate">The matrutiy date.</param>
        /// <param name="spread">The spread.</param>
        public PriceableCommoditySpread(String identifier, DateTime baseDate, DateTime maturityDate, BasicQuotation spread)
            : base(baseDate, spread)
        {
            Id = identifier;
            RiskMaturityDate = maturityDate;
            TimeToExpiry = GetTimeToMaturity(baseDate, RiskMaturityDate);
            SetRate(spread);
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
        /// Gets the value at maturity.
        /// </summary>
        /// <value>The values.</value>
        public override decimal ValueAtMaturity
        {
            get
            {
                var values = Values;
                return values[values.Count - 1];
            }
            set
            {
                var values = Values;
                values[values.Count] = value;
                Values = values;
            }
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override IList<DateTime> GetRiskDates()
        {
            return new[] { RiskMaturityDate };
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns>The spread calculated from the curve provided and the marketquote of the asset.</returns>
        public override decimal CalculateSpreadQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Spread.value;
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