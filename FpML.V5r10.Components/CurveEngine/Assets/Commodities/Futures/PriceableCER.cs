#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableCER : PriceableCommodityFuturesAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCER"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="rollCalendar">THe rollCalendar.</param>
        /// <param name="fixedRate"></param>
        public PriceableCER(DateTime baseDate, CommodityFutureNodeStruct nodeStruct,
                                    IBusinessCalendar rollCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct.Future, nodeStruct.BusinessDayAdjustments, fixedRate, null)
        {
            Id = nodeStruct.Future.id;
            Future = nodeStruct.Future;
            PriceQuoteUnits = nodeStruct.PriceQuoteUnits; 
            ModelIdentifier = "CommoditiesFuturesAsset";
            SettlementBasis = "The business day prior to the 15th calendar day of the contract month";
            ContractMonthPeriod = nodeStruct.ContractMonthPeriod;
            ContractSeries = "March (H), May (K), July (N), September (U) & December (Z)";
            var idParts = Id.Split('-');
            var exchangeCommodityNames = idParts[2].Split('.');
            var commodityCode = exchangeCommodityNames[0];
            if (exchangeCommodityNames.Length > 1)
            {
                commodityCode = exchangeCommodityNames[1];
            }
            var immCode = idParts[3];
            //Catch the relative rolls.
            if (int.TryParse(immCode, out var intResult))
            {
                var tempTradingDate = LastTradingDayHelper.ParseCode(commodityCode);
                immCode = tempTradingDate.GetNthMainCycleCode(baseDate, intResult);
            }
            var lastTradingDay = LastTradingDayHelper.Parse(commodityCode, immCode);
            LastTradeDate = lastTradingDay.GetLastTradingDay(baseDate);
            RiskMaturityDate = LastTradeDate;
            TimeToExpiry = (decimal)DayCounter.YearFraction(BaseDate, RiskMaturityDate);
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