﻿/*
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

namespace Highlander.Reporting.ModelFramework.V5r3.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableFuturesOptionAssetController : IPriceableFuturesAssetController
    {
        /// <summary>
        /// Gets the volatility at expiry.
        /// </summary>
        /// <value>The volatility at expiry.</value>
        decimal VolatilityAtRiskMaturity { get; }

        /// <summary>
        /// The is a call flag.
        /// </summary>
        bool IsCall { get; }

        /// <summary>
        /// The commodity identifier.
        /// </summary>
        string CommodityIdentifier { get; }
    }
}