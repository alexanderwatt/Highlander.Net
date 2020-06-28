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
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Cash
{
    /// <summary>
    /// Deposit rates
    /// </summary>
    public abstract class PriceableCash : PriceableSimpleRateAsset
    {
        /// <summary>
        /// 
        /// </summary>
        public Period Term { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCash"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="cash">The deposit.</param>
        /// <param name="amount">The cah amount.</param>
        /// <param name="term">The term of the cash flow.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        protected PriceableCash(DateTime baseDate, Asset cash, Decimal amount, Period term, IBusinessCalendar paymentCalendar,
                                BusinessDayAdjustments businessDayAdjustments, BasicQuotation fixedRate)
            : base(cash.id, baseDate, amount, businessDayAdjustments, fixedRate)
        {
            Term = term;      
            RiskMaturityDate =
                paymentCalendar.Advance(BaseDate, OffsetHelper.FromInterval(Term, DayTypeEnum.Calendar),
                                  BusinessDayAdjustments.businessDayConvention);
            YearFraction = (decimal)Actual365.Instance.YearFraction(BaseDate, RiskMaturityDate);
        }
    }
}