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
using Highlander.CalendarEngine.V5r3.Dates;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Commodities.Futures
{
    /// <summary>
    /// Base class for priceable brent oil futures.
    /// </summary>
    public class PriceableLMEFuture : PriceableCommodityFuturesAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableLMEFuture"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="rollCalendar">THe rollCalendar.</param>
        /// <param name="fixedRate"></param>
        public PriceableLMEFuture(DateTime baseDate, CommodityFutureNodeStruct nodeStruct,
                                    IBusinessCalendar rollCalendar, BasicQuotation fixedRate)
            : base(baseDate, nodeStruct.Future, nodeStruct.BusinessDayAdjustments, fixedRate, new Offset
                {dayType = DayTypeEnum.Business,dayTypeSpecified = true, period = PeriodEnum.D, periodMultiplier = "0",
                    periodSpecified = true
                })
        {
            Id = nodeStruct.Future.id;
            CommodityType = Future.description;
            Future = nodeStruct.Future;
            PriceQuoteUnits = nodeStruct.PriceQuoteUnits;         
            ModelIdentifier = "CommoditiesFuturesAsset";
            SettlementBasis = "EFP Delivery with an option to cash settle.";
            ContractMonthPeriod = nodeStruct.ContractMonthPeriod;
            ContractSeries = "Daily:3M/Weekly:6M/Monthly:" + ContractMonthPeriod.ToString();
            ExchangeIdentifier = nodeStruct.Future.exchangeId.Value;
            var idParts = Id.Split('-');
            var immCode = idParts[3];
            if (int.TryParse(immCode, out int intResult))
            {
                //If the roll is less that 3M then daily roll.
                //Less the number of business days in the next 3 months i.e. approximately 60.
                //TODO This hsould be all moved to configuration!
                var dailyCutoverOffset = new Offset
                {
                    dayType = DayTypeEnum.Calendar,
                    dayTypeSpecified = true,
                    period = PeriodEnum.M,
                    periodMultiplier = "3",
                    periodSpecified = true
                };
                var dailyCutoverDate = rollCalendar.Advance(BaseDate, dailyCutoverOffset, BusinessDayConventionEnum.MODFOLLOWING);
                var businessDays = rollCalendar.BusinessDaysBetweenDates(BaseDate, dailyCutoverDate);
                if (intResult <= businessDays.Count)
                {
                    LastTradeDate = businessDays[intResult];
                }
                //TODO
                //If the roll is greater than 3M and less than 6M then weekly on a Wednesday.
                //Less the number of weeks in the following 3 months i.e. approximately 12
                //
                else
                {
                    //Remember thefirst expiry is after the 3 motn cutover!
                    //TODO this should be the 6 ContractMonthPeriod.
                    var rollDate = baseDate.AddMonths(intResult - businessDays.Count + 3);
                    var lastTradingDay = new LastTradingDate().GetLastTradingDay(rollDate.Month, rollDate.Year);
                    //Do the date adjustment logic.
                    LastTradeDate = rollCalendar.Advance(lastTradingDay, RollOffset,
                                                      BusinessDayConventionEnum.PRECEDING);
                }
            }
            //This means that the value should be a period.
            else
            {
                var term = PeriodHelper.Parse(immCode);
                LastTradeDate = GetEffectiveDate(BaseDate, rollCalendar, term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            }
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