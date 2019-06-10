/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceable rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableFloatingCashflow<AMP, AMR> : IPriceableCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the notional amount.
        /// </summary>
        /// <value>The notional amount.</value>
        Money NotionalAmount { get; }

        /// <summary>
        /// Gets the start index: if there is one, in which case the expected cash flow will be the difference.
        /// </summary>
        /// <value>The start index.</value>
        Decimal? GetStartIndex();

        /// <summary>
        /// Gets the floating index.
        /// </summary>
        /// <value>The floating index.</value>
        Decimal GetFloatingIndex();

        /// <summary>
        /// Gets the HasReset flag.
        /// </summary>
        /// <value>The HasReset flag.</value>
        bool HasReset { get; }

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