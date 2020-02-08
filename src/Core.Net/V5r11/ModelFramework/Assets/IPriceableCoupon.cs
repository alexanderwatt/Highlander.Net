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

using System;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.ModelFramework.V5r3.Assets
{
    /// <summary>
    /// Base interface for a priceable rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableCoupon<AMP, AMR> : IPriceableCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the notional amount.
        /// </summary>
        /// <value>The notional amount.</value>
        Money NotionalAmount { get; }

        /// <summary>
        /// Start of the accrual period.
        /// </summary>
        DateTime AccrualStartDate
        {
            get;
        }

        /// <summary>
        /// End of the accrual period.
        /// </summary>
        DateTime AccrualEndDate
        {
            get;
        }

        /// <summary>
        /// Accrued amount at the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Money GetAccruedAmount(DateTime date);

        /// <summary>
        /// Forecast amount at the end date.
        /// </summary>
        /// <returns></returns>
        Money GetForecastAmount();

        ///<summary>
        /// Gts the currency.
        ///</summary>
        ///<returns></returns>
        Currency GetCurrency();
    }
}