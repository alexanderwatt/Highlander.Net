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

using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;

#endregion

namespace Orion.ModelFramework.Instruments.Commodities
{
    /// <summary>
    /// Base interface for a priceable fx rate
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableCommodityCashflow<AMP, AMR> : IPriceableCashflow<AMP, AMR>
    {
        // Requirements for pricing
        /// <summary>
        /// The base currency amount.
        /// </summary>
        Money BaseCurrencyAmount { get; }

        // Requirements for pricing
        /// <summary>
        /// The reference currency amount.
        /// </summary>
        Money ReferenceCurrencyAmount { get; }

        ///<summary>
        /// Gets the base currency.
        ///</summary>
        ///<returns></returns>
        Currency GetBaseCurrency();

        ///<summary>
        /// Gets the base currency.
        ///</summary>
        ///<returns></returns>
        Currency GetReferenceCurrency();
    }
}