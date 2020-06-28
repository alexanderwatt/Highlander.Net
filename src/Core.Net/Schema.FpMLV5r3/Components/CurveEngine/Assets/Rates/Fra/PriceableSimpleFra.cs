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
using Highlander.Codes.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using DayCounterHelper=Highlander.Reporting.Analytics.V5r3.DayCounters.DayCounterHelper;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Fra
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleFra : PriceableSimpleRateAsset
    {
        /// <summary>
        /// FixingDateOffset
        /// </summary>
        public RelativeDateOffset FixingDateOffset { get; set; }

        /// <summary>
        /// SimpleFra
        /// </summary>
        public SimpleFra SimpleFra { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SpotDate { get; set; }

        ///<summary>
        ///</summary>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public override decimal DiscountFactorAtMaturity => CalculationResults?.DiscountFactorAtMaturity ?? EndDiscountFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleFra"/> class.
        /// This is a special case for use with the factory for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.></param>
        /// <param name="notional">The notional. The default value is 1.00m.</param>
        /// <param name="normalisedRate">The fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableSimpleFra(DateTime baseDate, SimpleFraNodeStruct nodeStruct, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicQuotation normalisedRate)
            : base(nodeStruct.SimpleFra.id, baseDate, notional, nodeStruct.BusinessDayAdjustments, normalisedRate)
        {
            SimpleFra = nodeStruct.SimpleFra;
            FixingDateOffset = nodeStruct.SpotDate;
            UnderlyingRateIndex = nodeStruct.RateIndex;
            SpotDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            AdjustedStartDate = GetEffectiveDate(SpotDate, paymentCalendar, nodeStruct.SimpleFra.startTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);//GetSpotDate();
            RiskMaturityDate = GetEffectiveDate(SpotDate, paymentCalendar, nodeStruct.SimpleFra.endTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(SimpleFra.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
            TimeToExpiry = GetTimeToMaturity(baseDate, RiskMaturityDate);
        }

        /// <summary>
        /// Returns the time to expiry.
        /// </summary>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return  RiskMaturityDate;
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