/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace FpML.V5r10.Reporting.Models.ForeignExchange.FxSwap
{
    public enum FxSwapInstrumentMetrics
    {
        //NPV, 
        BaseCurrencyNPV
        , ForeignCurrencyNPV
    }

    public interface IFxSwapInstrumentResults
    {
        ///// <summary>
        ///// Gets the NPV in reporting currency.
        ///// </summary>
        ///// <value>The NPV.</value>
        //Decimal NPV { get; }

        /// <summary>
        /// Gets the base currency NPV.
        /// </summary>
        /// <value>The base xcurrency NPV.</value>
        Decimal BaseCurrencyNPV { get; }

        /// <summary>
        /// Gets the foreign currency NPV.
        /// </summary>
        /// <value>The foreign currency NPV.</value>
        Decimal ForeignCurrencyNPV { get; }

    }
}