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

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Cash
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public class PriceableRepo : PriceableSimpleRateAsset
    {
        ///<summary>
        ///</summary>
        public RelativeDateOffset SpotDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public Deposit Deposit { get; set; }

        ///<summary>
        ///</summary>
        public Asset UnderlyingAsset { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRepo"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="amount">Notional Amount.</param>
        /// <param name="nodeStruct">The deposit nodeStruct.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        public PriceableRepo(DateTime baseDate, Decimal amount, RepoNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, 
                                IBusinessCalendar paymentCalendar, BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
            : base(nodeStruct.Deposit.id, baseDate, amount, businessDayAdjustments, fixedRate)
        {
            SpotDateOffset = nodeStruct.SpotDate;
            Deposit = nodeStruct.Deposit;
            UnderlyingAsset = nodeStruct.UnderlyingAsset;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            RiskMaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.Deposit.term, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            YearFraction = GetYearFraction(Deposit.dayCountFraction.Value, AdjustedStartDate, RiskMaturityDate);
        }
    }
}