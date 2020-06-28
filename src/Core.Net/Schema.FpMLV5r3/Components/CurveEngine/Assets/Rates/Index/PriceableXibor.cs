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
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Index
{
    /// <summary>
    /// Xibor Rates
    /// </summary>
    public class PriceableXibor : PriceableRateIndex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixedRate"></param>
        public PriceableXibor(DateTime baseDate, Decimal amount, XiborNodeStruct nodeStruct,
                              IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
                              BasicQuotation fixedRate)
            : base(baseDate, amount, nodeStruct.RateIndex, nodeStruct.BusinessDayAdjustments, nodeStruct.SpotDate, fixedRate)
        {
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.RateIndex.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(nodeStruct.RateIndex.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXibor"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="riskMaturityDate">The risk maturity date.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        public PriceableXibor(string id, Decimal amount, DateTime effectiveDate, DateTime riskMaturityDate, DayCountFraction dayCountFraction)
            : base(id, amount, effectiveDate, riskMaturityDate)
        {
            YearFraction = GetYearFraction(dayCountFraction.Value, effectiveDate, riskMaturityDate);
        }
    }
}