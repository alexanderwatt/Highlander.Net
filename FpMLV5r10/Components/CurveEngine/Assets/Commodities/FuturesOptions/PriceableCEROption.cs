#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using Orion.CalendarEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableCEROption : PriceableCER, IPriceableCommodityFuturesOptionAssetController
    {
        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        public DateTime OptionsExpiryDate { get; }

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        public DateTime GetExpiryDate() => OptionsExpiryDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCEROption"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="rollCalendar">THe rollCalendar.</param>
        /// <param name="fixedRate"></param>
        public PriceableCEROption(DateTime baseDate, CommodityFutureNodeStruct nodeStruct,
                                    IBusinessCalendar rollCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct, rollCalendar, fixedRate)
        {
            Id = nodeStruct.Future.id;
            Future = nodeStruct.Future;
            ModelIdentifier = "CommoditiesFuturesOptionAsset";
            var idParts = Id.Split('-');
            var exchangeCommodityName = idParts[2];
            var immCode = idParts[3];
            int intResult;
            //Catch the relative rolls.
            if (int.TryParse(immCode, out intResult))
            {
                var tempTradingDate = LastTradingDayHelper.ParseCode(immCode);
                immCode = tempTradingDate.GetNthMainCycleCode(baseDate, intResult);
            }
            var lastTradingDay = LastTradingDayHelper.Parse(exchangeCommodityName, immCode);
            LastTradeDate = lastTradingDay.GetLastTradingDay(baseDate);
            RiskMaturityDate = LastTradeDate;
            OptionsExpiryDate = LastTradeDate;
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

        /// <summary>
        /// Gets the volatility at expiry.
        /// </summary>
        /// <value>The volatility at expiry.</value>
        public decimal VolatilityAtRiskMaturity { get; set; }

        /// <summary>
        /// The is a call flag.
        /// </summary>
        public bool IsCall { get; set; }

        /// <summary>
        /// The commodity identifier.
        /// </summary>
        public string CommodityIdentifier { get; set; }
    }
}