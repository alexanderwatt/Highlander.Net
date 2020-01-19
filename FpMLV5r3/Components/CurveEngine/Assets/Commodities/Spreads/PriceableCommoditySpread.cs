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
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using DayCounterHelper=Highlander.Reporting.Analytics.V5r3.DayCounters.DayCounterHelper;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Commodities.Spreads
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableCommoditySpread : PriceableSimpleCommoditySpreadAsset
    {
        /// <summary>
        /// The time to expiry
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