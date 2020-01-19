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
using Highlander.CurveEngine.V5r3.Assets.Commodities.Futures;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Commodities.FuturesOptions
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