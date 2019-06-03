#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.CalendarEngine.Dates;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for priceable brent oil futures.
    /// </summary>
    public class PriceableIceBrentFuture : PriceableCommodityFuturesAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIceBrentFuture"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="rollCalendar">THe rollCalendar.</param>
        /// <param name="fixedRate"></param>
        public PriceableIceBrentFuture(DateTime baseDate, CommodityFutureNodeStruct nodeStruct,
                                    IBusinessCalendar rollCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct.Future, nodeStruct.BusinessDayAdjustments, fixedRate, new Offset
                {dayType = DayTypeEnum.Business,dayTypeSpecified = true, period = PeriodEnum.D, periodMultiplier = "0",
                    periodSpecified = true
                })
        {
            Id = nodeStruct.Future.id;
            Future = nodeStruct.Future;
            PriceQuoteUnits = nodeStruct.PriceQuoteUnits; 
            ModelIdentifier = "CommoditiesFuturesAsset";
            SettlementBasis = "EFP Delivery with an option to cash settle.";
            ContractSeries = "Monthly";
            var idParts = Id.Split('-');
            //var exchangeCommodityName = idParts[2];
            var immCode = idParts[3];
            //Only the relative rolls.
            if (int.TryParse(immCode, out var intResult))
            {
                var lastTradingDay = new FirstDayLessFifteen().GetNthLastTradingDay(baseDate, intResult);
                //Do the date adjustment logic.
                LastTradeDate = rollCalendar.Advance(lastTradingDay, RollOffset,
                                                  BusinessDayConventionEnum.PRECEDING);
                RiskMaturityDate = LastTradeDate;
                TimeToExpiry = (decimal) DayCounter.YearFraction(BaseDate, RiskMaturityDate);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return IndexAtMaturity;
        }
    }
}