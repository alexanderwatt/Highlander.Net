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

namespace Highlander.Reporting.Models.V5r3.Futures
{
    public enum FuturesOptionMetrics
    {
        ForwardDelta,
        NPV,
        NPVChange,
        ImpliedQuote,
        MarketQuote,
        IndexAtMaturity,
        PandL,
        InitialMargin,
        VariationMargin,
        ImpliedStrike,
        OptionVolatility,
        SpotDelta
    }

    public interface IFuturesOptionAssetResults : IFuturesAssetResults
    {
        /// <summary>
        /// Gets the strike.
        /// </summary>
        Decimal ImpliedStrike { get; }

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        Decimal OptionVolatility { get; }

        /// <summary>
        /// Gets the spot delta.
        /// </summary>
        /// <value>The spot delta.</value>
        Decimal SpotDelta { get; }
    }
}