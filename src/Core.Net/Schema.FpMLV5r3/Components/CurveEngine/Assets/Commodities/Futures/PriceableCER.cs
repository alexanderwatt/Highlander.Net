/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Commodities.Futures
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
            int intResult;
            //Catch the relative rolls.
            if (int.TryParse(immCode, out intResult))
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