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

using System.Collections.Generic;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Assets
{
    /// <summary>
    /// Base interface for a priceable rate coupon
    /// </summary>
    /// <typeparam name="AMP">The type of the Analytic Model Parameters.</typeparam>
    /// <typeparam name="AMR">The type of the Analytic Model Results.</typeparam>
    public interface IPriceableFxRateCashflow<AMP, AMR> : IPriceableFloatingCashflow<AMP, AMR>
    {
        /// <summary>
        /// Gets the start fx rate.
        /// </summary>
        /// <value>The start index.</value>
        FxRate GetDealtFxRate();

        /// <summary>
        /// Gets the floating fx rate.
        /// </summary>
        /// <value>The floating fx rate.</value>
        FxRate GetFloatingFxRate();

        ///<summary>
        /// Gts the risk currencies: there may be more than one.
        ///</summary>
        ///<returns></returns>
        List<Currency> GetRiskCurrencies();
    }
}